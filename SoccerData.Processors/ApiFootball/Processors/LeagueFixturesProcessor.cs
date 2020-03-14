using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SoccerData.Model;

namespace SoccerData.Processors.ApiFootball.Processors
{
	public class LeagueFixturesProcessor : IProcessor
	{
		private int CompetitionSeasonId;
		private readonly JsonUtility JsonUtility;

		public LeagueFixturesProcessor(int competitionSeasonId)
		{
			this.CompetitionSeasonId = competitionSeasonId;
			this.JsonUtility = new JsonUtility(24 * 60 * 60, sourceType: JsonUtility.JsonSourceType.ApiFootball);
		}

		public void Run(SoccerDataContext dbContext)
		{
			var dbCompetitionSeason = dbContext.CompetitionSeasons
												.Include(x => x.CompetitionSeasonRounds)
												.Include(x => x.Fixtures)
												.SingleOrDefault(x => x.CompetitionSeasonId == this.CompetitionSeasonId);
			if (dbCompetitionSeason == null)
			{
				return;
			}

			Console.WriteLine($"LFx-{dbCompetitionSeason.CompetitionSeasonId}-{dbCompetitionSeason.Season}-{dbCompetitionSeason.Competition.CompetitionName}");

			var url = Feeds.LeagueFixturesFeed.GetFeedUrlByLeagueId(dbCompetitionSeason.ApiFootballId);
			var rawJson = JsonUtility.GetRawJsonFromUrl(url);
			var feed = Feeds.LeagueFixturesFeed.FromJson(rawJson);

			var teamIds = feed.Result.Fixtures.SelectMany(x => new[] { x.AwayTeam.TeamId, x.HomeTeam.TeamId }).Distinct().ToList();
			var fixtureDict = dbCompetitionSeason.Fixtures.ToDictionary(x => x.ApiFootballId);
			var teamDict = dbContext.TeamSeasons.Where(x => x.CompetitionSeasonId == this.CompetitionSeasonId && teamIds.Contains(x.Team.ApiFootballId)).ToDictionary(x => x.Team.ApiFootballId, y => y.TeamSeasonId);
			var venueDict = dbContext.VenueSeasons.Where(x => x.Season == dbCompetitionSeason.Season).ToList();
			var roundDict = dbCompetitionSeason.CompetitionSeasonRounds.ToDictionary(x => x.ApiFootballKey, y => y.CompetitionSeasonRoundId);

			var feedFixtures = feed.Result.Fixtures.OrderBy(x => x.EventTimestamp).ThenBy(x => x.HomeTeam?.TeamName).ToList();

			#region ADD MISSING TEAMS INDIVIDUALLY (THEY ARE NOT IN THE API TEAMS LIST)
			var apiTeamIds = feedFixtures.SelectMany(x => new[] { x.AwayTeam.TeamId, x.HomeTeam.TeamId }).ToList();
			var missingApiTeamIds = apiTeamIds.Where(x => !teamDict.ContainsKey(x)).ToList();
			if (missingApiTeamIds != null && missingApiTeamIds.Count > 0)
			{
				foreach (var missingApiTeamId in missingApiTeamIds)
				{
					var dbTeam = dbContext.Teams.SingleOrDefault(x => x.ApiFootballId == missingApiTeamId);
					if (dbTeam == null)
					{
						var teamProcessor = new TeamProcessor(missingApiTeamId);
						teamProcessor.Run(dbContext);
						dbTeam = dbContext.Teams.SingleOrDefault(x => x.ApiFootballId == missingApiTeamId);
					}

					var dbTeamSeason = dbContext.TeamSeasons.SingleOrDefault(x => x.TeamId == dbTeam.TeamId && x.CompetitionSeasonId == this.CompetitionSeasonId);
					if (dbTeamSeason == null)
					{
						// CANNOT SET VENUE HERE FROM dbTeam OR API TEAM CALL BECAUSE OF POSSIBLE SEASON MISMATCH
						dbTeamSeason = new TeamSeason
						{
							CompetitionSeasonId = this.CompetitionSeasonId,
							TeamId = dbTeam.TeamId,
							LogoUrl = dbTeam.LogoUrl,
							Season = dbCompetitionSeason.Season,
							TeamName = dbTeam.TeamName
						};
						dbContext.TeamSeasons.Add(dbTeamSeason);
						dbContext.SaveChanges();
					}
				}
			}
			#endregion ADD MISSING TEAMS INDIVIDUALLY 

			foreach (var feedFixture in feedFixtures)
			{
				if (!fixtureDict.TryGetValue(feedFixture.FixtureId, out Fixture dbFixture))
				{
					// ASSUME ROUNDS ARE POPULATED FROM CompetitionSeasonRoundsFeed
					var dbRoundId = roundDict[feedFixture.Round];

					dbFixture = new Fixture
					{
						ApiFootballId = feedFixture.FixtureId,
						CompetitionSeasonId = dbCompetitionSeason.CompetitionSeasonId,
						CompetitionSeasonRoundId = dbRoundId
					};
					fixtureDict.Add(feedFixture.FixtureId, dbFixture);
					dbContext.Fixtures.Add(dbFixture);
				}

				// ALLOW TEAMS TO CHANGE UNTIL GAME STARTS
				// ASSUME TEAMS ARE POPULATED FROM TeamsFeed
				#region TEAMS
				if (!dbFixture.HomeTeamSeasonId.HasValue
					|| !dbFixture.AwayTeamSeasonId.HasValue
					|| (feedFixture.HomeTeam?.TeamId != null && dbFixture.HomeTeamSeason.Team.ApiFootballId != feedFixture.HomeTeam.TeamId)
					|| (feedFixture.AwayTeam?.TeamId != null && dbFixture.AwayTeamSeason.Team.ApiFootballId != feedFixture.AwayTeam.TeamId))
				{
					if (feedFixture.HomeTeam != null)
					{
						int homeTeamSeasonId = teamDict[feedFixture.HomeTeam.TeamId];
						if (dbFixture.HomeTeamSeasonId != homeTeamSeasonId)
						{
							dbFixture.HomeTeamSeasonId = homeTeamSeasonId;
						}
					}

					if (feedFixture.AwayTeam != null)
					{
						int awayTeamSeasonId = teamDict[feedFixture.AwayTeam.TeamId];
						if (dbFixture.AwayTeamSeasonId != awayTeamSeasonId)
						{
							dbFixture.AwayTeamSeasonId = awayTeamSeasonId;
						}
					}
				}
				#endregion TEAMS

				#region SCORING
				if (feedFixture.GoalsHomeTeam.HasValue && (!dbFixture.HomeScore.HasValue || feedFixture.GoalsHomeTeam.Value != dbFixture.HomeScore))
				{
					dbFixture.HomeScore = feedFixture.GoalsHomeTeam.Value;
				}
				if (feedFixture.GoalsAwayTeam.HasValue && (!dbFixture.AwayScore.HasValue || feedFixture.GoalsAwayTeam.Value != dbFixture.AwayScore))
				{
					dbFixture.AwayScore = feedFixture.GoalsAwayTeam.Value;
				}

				if (feedFixture.Score != null)
				{
					if (feedFixture.Score.Halftime != null)
					{
						if (!dbFixture.HomeHalfTimeScore.HasValue
							|| !dbFixture.AwayHalfTimeScore.HasValue
							|| feedFixture.Score.Halftime != $"{dbFixture.HomeHalfTimeScore}-{dbFixture.AwayHalfTimeScore}")
						{
							var arrScores = feedFixture.Score.Halftime.Split('-').ToList();
							if (arrScores != null && arrScores.Count == 2 && int.TryParse(arrScores[0], out int score1) && int.TryParse(arrScores[1], out int score2))
							{
								dbFixture.HomeHalfTimeScore = score1;
								dbFixture.AwayHalfTimeScore = score2;
							}
						}
					}

					if (feedFixture.GoalsHomeTeam.HasValue && feedFixture.GoalsAwayTeam.HasValue)
					{
						if (!dbFixture.HomeFullTimeScore.HasValue
							|| !dbFixture.AwayFullTimeScore.HasValue
							|| dbFixture.HomeScore != feedFixture.GoalsHomeTeam
							|| dbFixture.AwayScore != feedFixture.GoalsAwayTeam)
						{
							dbFixture.HomeFullTimeScore = feedFixture.GoalsHomeTeam;
							dbFixture.AwayFullTimeScore = feedFixture.GoalsAwayTeam;
						}
					}

					if (feedFixture.Score.ExtraTime != null)
					{
						if (!dbFixture.HomeExtraTimeScore.HasValue
							|| !dbFixture.AwayExtraTimeScore.HasValue
							|| feedFixture.Score.ExtraTime != $"{dbFixture.HomeExtraTimeScore}-{dbFixture.AwayExtraTimeScore}")
						{
							var arrScores = feedFixture.Score.ExtraTime.Split('-').ToList();
							if (arrScores != null && arrScores.Count == 2 && int.TryParse(arrScores[0], out int score1) && int.TryParse(arrScores[1], out int score2))
							{
								dbFixture.HomeExtraTimeScore = score1;
								dbFixture.AwayExtraTimeScore = score2;
							}
						}
					}

					if (feedFixture.Score.Penalty != null)
					{
						if (!dbFixture.HomePenaltiesScore.HasValue
							|| !dbFixture.AwayPenaltiesScore.HasValue
							|| feedFixture.Score.Penalty != $"{dbFixture.HomePenaltiesScore}-{dbFixture.AwayPenaltiesScore}")
						{
							var arrScores = feedFixture.Score.Penalty.Split('-').ToList();
							if (arrScores != null && arrScores.Count == 2 && int.TryParse(arrScores[0], out int score1) && int.TryParse(arrScores[1], out int score2))
							{
								dbFixture.HomePenaltiesScore = score1;
								dbFixture.AwayPenaltiesScore = score2;
							}
						}
					}
				}
				#endregion SCORING

				if (feedFixture.EventDate.HasValue && dbFixture.GameTimeUtc != feedFixture.EventDate.Value.UtcDateTime)
				{
					dbFixture.GameTimeUtc = feedFixture.EventDate.Value.UtcDateTime;
				}

				if (dbFixture.Status != feedFixture.Status || dbFixture.StatusShort != feedFixture.StatusShort)
				{
					dbFixture.Status = feedFixture.Status;
					dbFixture.StatusShort = feedFixture.StatusShort;
				}

				if (feedFixture.Elapsed.HasValue && dbFixture.TimeElapsed != feedFixture.Elapsed.Value)
				{
					dbFixture.TimeElapsed = feedFixture.Elapsed.Value;
				}

				if (feedFixture.FirstHalfStart.HasValue && dbFixture.FirstHalfStartUtc != feedFixture.FirstHalfStart.Value)
				{
					dbFixture.FirstHalfStartUtc = feedFixture.FirstHalfStart.Value.UtcDateTime;
				}

				if (feedFixture.SecondHalfStart.HasValue && dbFixture.SecondHalfStartUtc != feedFixture.SecondHalfStart.Value)
				{
					dbFixture.SecondHalfStartUtc = feedFixture.SecondHalfStart.Value.UtcDateTime;
				}

				if (feedFixture.Referee != null && dbFixture.Referee != feedFixture.Referee)
				{
					dbFixture.Referee = feedFixture.Referee;
				}
			}
			dbContext.SaveChanges();
		}
	}
}
