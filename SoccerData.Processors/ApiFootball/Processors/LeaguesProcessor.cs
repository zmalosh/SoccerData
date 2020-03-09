using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SoccerData.Model;

namespace SoccerData.Processors.ApiFootball.Processors
{
	public class LeaguesProcessor : IProcessor
	{
		private readonly JsonUtility JsonUtility;

		public LeaguesProcessor()
		{
			this.JsonUtility = new JsonUtility(24 * 60 * 60, sourceType: JsonUtility.JsonSourceType.ApiFootball);
		}

		public void Run(SoccerDataContext dbContext)
		{
			var url = Feeds.LeaguesFeed.GetFeedUrl();
			var rawJson = JsonUtility.GetRawJsonFromUrl(url);
			var feed = Feeds.LeaguesFeed.FromJson(rawJson);

			var orderedLeagues = feed.Result.Leagues.OrderBy(x => x.Season).ThenBy(x => x.CountryCode).ThenBy(x => x.Name).ToList();

			var existingCompetitions = dbContext.Competitions.ToDictionary(x => GetCompetitionKey(x), y => y);
			var existingCompetitionSeasons = dbContext.CompetitionSeasons.ToDictionary(x => x.ApiFootballId);

			var countryDict = dbContext.Countries.ToDictionary(x => x.CountryAbbr ?? "(null)", y => y.CountryId);

			foreach (var league in orderedLeagues)
			{
				var key = GetCompetitionKey(league);
				if (!existingCompetitions.TryGetValue(key, out Competition dbCompetition))
				{
					dbCompetition = new Competition
					{
						CompetitionName = league.Name.Trim(),
						CompetitionType = league.Type.Trim(),
						CountryId = countryDict[league.CountryCode ?? "(null)"],
						LogoUrl = league.Logo?.ToString()
					};

					existingCompetitions.Add(key, dbCompetition);
					dbContext.Competitions.Add(dbCompetition);
					dbContext.SaveChanges();
				}

				if (!existingCompetitionSeasons.ContainsKey(league.LeagueId))
				{
					var dbCompetitionSeason = new CompetitionSeason
					{
						ApiFootballId = league.LeagueId,
						CompetitionId = dbCompetition.CompetitionId,
						EndDate = league.SeasonEnd,
						HasFixtures = league.Coverage.Fixtures.Events,
						HasLineups = league.Coverage.Fixtures.Lineups,
						HasOdds = league.Coverage.Odds,
						HasPlayers = league.Coverage.Players,
						HasPlayerStats = league.Coverage.Fixtures.PlayersStatistics,
						HasPredictions = league.Coverage.Predictions,
						HasStandings = league.Standings,
						HasTeamStats = league.Coverage.Fixtures.TeamStatistics,
						HasTopScorers = league.Coverage.TopScorers,
						IsCurrent = league.IsCurrent,
						Season = league.Season,
						StartDate = league.SeasonStart
					};

					existingCompetitionSeasons.Add(league.LeagueId, dbCompetitionSeason);
					if (dbCompetition.CompetitionSeasons == null)
					{
						dbCompetition.CompetitionSeasons = new List<CompetitionSeason>();
					}
					dbCompetition.CompetitionSeasons.Add(dbCompetitionSeason);
					dbContext.SaveChanges();
				}
			}
		}

		private string GetCompetitionKey(Competition dbCompetition)
		{
			return $"{dbCompetition.Country.CountryAbbr ?? "(null)"}_{dbCompetition.CompetitionName}";
		}

		private string GetCompetitionKey(Feeds.LeaguesFeed.League league)
		{
			return $"{league.CountryCode ?? "(null)"}_{league.Name}";
		}

		private string GetCompetitionSeasonKey(int competitionId, int season)
		{
			return $"{competitionId}_{season}";
		}
	}
}
