using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SoccerData.Model;

namespace SoccerData.Processors.ApiFootball.Processors
{
	public class FixtureProcessor : IProcessor
	{
		private readonly int ApiFootballFixtureId;
		private readonly JsonUtility JsonUtility;

		public FixtureProcessor(int apiFootballFixtureId)
		{
			this.ApiFootballFixtureId = apiFootballFixtureId;
			this.JsonUtility = new JsonUtility(120 * 24 * 60 * 60, sourceType: JsonUtility.JsonSourceType.ApiFootball); // 230K+ FIXTURES.... SAVE FINISHED GAMES FOR A LONG TIME (120 DAYS?) TO AVOID QUOTA ISSUES
		}

		public void Run(SoccerDataContext dbContext)
		{
			var dbFixture = dbContext.Fixtures.Single(x => x.ApiFootballId == this.ApiFootballFixtureId);
			var isFixtureFinal = string.Equals("Match Finished", dbFixture.Status, StringComparison.CurrentCultureIgnoreCase);

			if (!dbFixture.HomeTeamSeasonId.HasValue
				|| !dbFixture.AwayTeamSeasonId.HasValue
				|| !isFixtureFinal)
			{
				return;
			}

			var url = Feeds.FixtureFeed.GetFeedUrlByFixtureId(this.ApiFootballFixtureId);
			var rawJson = JsonUtility.GetRawJsonFromUrl(url);
			var feed = Feeds.FixtureFeed.FromJson(rawJson);

			Feeds.FixtureFeed.ApiFixture feedFixture = feed.Result.Fixtures.Single();

			int dbFixtureId = dbFixture.FixtureId;
			int dbHomeTeamSeasonId = dbFixture.HomeTeamSeasonId.Value;
			int dbAwayTeamSeasonId = dbFixture.AwayTeamSeasonId.Value;

			bool hasUpdate = false;

			Feeds.FixtureFeed.ApiLineup homeLineup = null;
			Feeds.FixtureFeed.ApiLineup awayLineup = null;
			string homeFormation = null;
			string awayFormation = null;
			if (feedFixture.Lineups == null || feedFixture.Lineups.Count != 2)
			{
				homeLineup = feedFixture.Lineups[feedFixture.HomeTeam.TeamName];
				awayLineup = feedFixture.Lineups[feedFixture.AwayTeam.TeamName];
				homeFormation = homeLineup.Formation;
				awayFormation = awayLineup.Formation;
			}

			#region ENSURE COACHES EXIST
			int? homeCoachId = null;
			int? awayCoachId = null;
			if (homeLineup != null || awayLineup != null)
			{
				var apiCoachIds = new[] { homeLineup.CoachId, awayLineup.CoachId };
				var dbCoaches = dbContext.Coaches.Where(x => apiCoachIds.Contains(x.ApiFootballId)).ToDictionary(x => x.ApiFootballId, y => y);

				if (homeLineup?.CoachId != null)
				{
					if (!dbCoaches.TryGetValue(homeLineup.CoachId.Value, out Coach dbHomeCoach))
					{
						dbHomeCoach = new Coach
						{
							ApiFootballId = homeLineup.CoachId.Value,
							CoachName = homeLineup.Coach
						};
						dbContext.Coaches.Add(dbHomeCoach);
						dbContext.SaveChanges();
					}
					homeCoachId = dbHomeCoach.CoachId;
				}
				if (awayLineup?.CoachId != null)
				{
					if (!dbCoaches.TryGetValue(awayLineup.CoachId.Value, out Coach dbAwayCoach))
					{
						dbAwayCoach = new Coach
						{
							ApiFootballId = awayLineup.CoachId.Value,
							CoachName = awayLineup.Coach
						};
						dbContext.Coaches.Add(dbAwayCoach);
						dbContext.SaveChanges();
					}
					awayCoachId = dbAwayCoach.CoachId;
				}
			}
			#endregion ENSURE COACHES EXIST 

			#region TEAM BOXSCORE

			var apiTeamStatsDict = feedFixture.TeamStatistics;
			if (apiTeamStatsDict == null)
			{
				if (!dbFixture.HasTeamBoxscores.HasValue || dbFixture.HasTeamBoxscores.Value)
				{
					hasUpdate = true;
				}
				dbFixture.HasTeamBoxscores = false;
			}
			else
			{
				var dbTeamBoxscores = dbContext.TeamBoxscores.Where(x => x.FixtureId == dbFixtureId);

				var dbHomeBoxscore = dbTeamBoxscores?.SingleOrDefault(x => x.TeamSeasonId == dbHomeTeamSeasonId);
				var dbAwayBoxscore = dbTeamBoxscores?.SingleOrDefault(x => x.TeamSeasonId == dbAwayTeamSeasonId);

				if (dbHomeBoxscore == null)
				{
					dbHomeBoxscore = new TeamBoxscore
					{
						FixtureId = dbFixtureId,
						TeamSeasonId = dbHomeTeamSeasonId,
						OppTeamSeasonId = dbAwayTeamSeasonId,
						IsHome = true
					};
					dbContext.TeamBoxscores.Add(dbHomeBoxscore);
					hasUpdate = true;
				}
				if (dbAwayBoxscore == null)
				{
					dbAwayBoxscore = new TeamBoxscore
					{
						FixtureId = dbFixtureId,
						TeamSeasonId = dbAwayTeamSeasonId,
						OppTeamSeasonId = dbHomeTeamSeasonId,
						IsHome = false,
					};
					dbContext.TeamBoxscores.Add(dbAwayBoxscore);
					hasUpdate = true;
				}

				if (PopulateTeamBoxscore(homeCoachId, homeFormation, apiTeamStatsDict, x => x.Home, ref dbHomeBoxscore))
				{
					hasUpdate = true;
					dbFixture.HasTeamBoxscores = true;
				}
				if (PopulateTeamBoxscore(awayCoachId, awayFormation, apiTeamStatsDict, x => x.Away, ref dbAwayBoxscore))
				{
					hasUpdate = true;
					dbFixture.HasTeamBoxscores = true;
				}

				if (!dbFixture.HasTeamBoxscores.HasValue)
				{
					dbFixture.HasTeamBoxscores = false;
				}
			}
			#endregion TEAM BOXSCORE

			if (hasUpdate)
			{
				dbContext.SaveChanges();
			}
		}

		#region TEAM BOXSCORE HELPERS

		/// <summary>
		/// 
		/// </summary>
		/// <param name="apiStatsDict">Team statistics for fixture from API</param>
		/// <param name="teamId">Team which accumulated desired stats</param>
		/// <param name="oppTeamId">Opponent of team which accumulated desired stats</param>
		/// <param name="isHome">Indicator for if the desired team is the home team</param>
		/// <param name="statGetFunc">Function to return the desired stat from a Statistic object. Used to choose home or away value.</param>
		/// <param name="dbTeamBoxscore">Object to populate</param>
		/// <returns>true if an update has been made; else false</returns>
		private bool PopulateTeamBoxscore(int? coachId, string teamFormation,
			Dictionary<string, Feeds.FixtureFeed.ApiTeamStatistic> apiStatsDict,
			Func<Feeds.FixtureFeed.ApiTeamStatistic, string> statGetFunc,
			ref TeamBoxscore dbTeamBoxscore)
		{
			bool hasUpdate = false;

			if (coachId.HasValue && (!dbTeamBoxscore.CoachId.HasValue || dbTeamBoxscore.CoachId != coachId.Value))
			{
				dbTeamBoxscore.CoachId = coachId;
				hasUpdate = true;
			}

			if (!string.IsNullOrEmpty(teamFormation) && dbTeamBoxscore.Formation != teamFormation)
			{
				dbTeamBoxscore.Formation = teamFormation;
				hasUpdate = true;
			}

			int? statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.ShotsOnGoal, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.ShotsOnGoal)
			{
				dbTeamBoxscore.ShotsOnGoal = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.ShotsOffGoal, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.ShotsOffGoal)
			{
				dbTeamBoxscore.ShotsOffGoal = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.TotalShots, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.ShotsTotal)
			{
				dbTeamBoxscore.ShotsTotal = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.BlockedShots, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.ShotsBlocked)
			{
				dbTeamBoxscore.ShotsBlocked = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.ShotsInsideBox, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.ShotsInsideBox)
			{
				dbTeamBoxscore.ShotsInsideBox = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.ShotsOutsideBox, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.ShotsOutsideBox)
			{
				dbTeamBoxscore.ShotsOutsideBox = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.FoulsCommitted, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.FoulsCommitted)
			{
				dbTeamBoxscore.FoulsCommitted = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.CornerKicks, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.CornerKicks)
			{
				dbTeamBoxscore.CornerKicks = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.Offsides, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.OffsidesCommitted)
			{
				dbTeamBoxscore.OffsidesCommitted = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.BallPossession, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.PossessionPct)
			{
				dbTeamBoxscore.PossessionPct = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.YellowCards, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.YellowCards)
			{
				dbTeamBoxscore.YellowCards = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.RedCards, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.RedCards)
			{
				dbTeamBoxscore.RedCards = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.GoalkeeperSaves, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.GoalieSaves)
			{
				dbTeamBoxscore.GoalieSaves = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.TotalPasses, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.PassesTotal)
			{
				dbTeamBoxscore.PassesTotal = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.AccuratePasses, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.PassesAccurate)
			{
				dbTeamBoxscore.PassesAccurate = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.PassCompPct, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.PassAccuracyPct)
			{
				dbTeamBoxscore.PassAccuracyPct = statVal.Value;
				hasUpdate = true;
			}

			return hasUpdate;
		}

		private int? GetStatValueByKey(string key,
			Dictionary<string, Feeds.FixtureFeed.ApiTeamStatistic> apiStatsDict,
			Func<Feeds.FixtureFeed.ApiTeamStatistic, string> statGetFunc)
		{
			if (!apiStatsDict.TryGetValue(key, out Feeds.FixtureFeed.ApiTeamStatistic apiTeamStat))
			{
				return null;
			}

			var strValue = statGetFunc(apiTeamStat);
			if (string.IsNullOrEmpty(strValue))
			{
				return null;
			}

			strValue = strValue.Replace("%", string.Empty);

			if (int.TryParse(strValue, out int result))
			{
				return result;
			}
			return null;
		}
		#endregion TEAM BOXSCORE HELPERS
	}
}
