using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
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

			var orderedApiLeagues = feed.Result.Leagues.OrderBy(x => x.CountryCode).ThenBy(x => x.Name).ThenBy(x => x.Season).ToList();

			var existingCompetitions = dbContext.Competitions.Include(x => x.Country).ToDictionary(x => GetCompetitionKey(x), y => y);
			var existingCompetitionSeasons = dbContext.CompetitionSeasons.ToDictionary(x => x.ApiFootballId);

			var countryDict = dbContext.Countries.ToDictionary(x => x.ApiFootballCountryName, y => y.CountryId);

			bool isChanged = false;
			foreach (var apiLeague in orderedApiLeagues)
			{
				var key = GetCompetitionKey(apiLeague);
				if (!existingCompetitions.TryGetValue(key, out Competition dbCompetition))
				{
					dbCompetition = new Competition
					{
						CompetitionName = apiLeague.Name.Trim(),
						CompetitionType = apiLeague.Type.Trim(),
						CountryId = countryDict[apiLeague.Country],
						LogoUrl = apiLeague.Logo?.ToString()
					};

					existingCompetitions.Add(key, dbCompetition);
					dbContext.Competitions.Add(dbCompetition);
					isChanged = true;
				}
			}

			if (isChanged)
			{
				dbContext.SaveChanges();
				isChanged = false;
			}

			foreach (var apiLeague in orderedApiLeagues)
			{
				var key = GetCompetitionKey(apiLeague);
				if (!existingCompetitions.TryGetValue(key, out Competition dbCompetition))
				{
					// KEY SHOULD HAVE BEEN ADDED ABOVE
					throw new Exception("SOMEHOW THE COMPETITION IS NOT IN THE DB?!?!");
				}
				if (!existingCompetitionSeasons.ContainsKey(apiLeague.LeagueId))
				{
					var dbCompetitionSeason = new CompetitionSeason
					{
						ApiFootballId = apiLeague.LeagueId,
						CompetitionId = dbCompetition.CompetitionId,
						EndDate = apiLeague.SeasonEnd,
						HasFixtureEvents = apiLeague.Coverage.Fixtures.Events,
						HasLineups = apiLeague.Coverage.Fixtures.Lineups,
						HasOdds = apiLeague.Coverage.Odds,
						HasPlayers = apiLeague.Coverage.Players,
						HasPlayerStats = apiLeague.Coverage.Fixtures.PlayersStatistics,
						HasPredictions = apiLeague.Coverage.Predictions,
						HasStandings = apiLeague.Standings,
						HasTeamStats = apiLeague.Coverage.Fixtures.TeamStatistics,
						HasTopScorers = apiLeague.Coverage.TopScorers,
						IsCurrent = apiLeague.IsCurrent,
						Season = apiLeague.Season,
						StartDate = apiLeague.SeasonStart
					};

					existingCompetitionSeasons.Add(apiLeague.LeagueId, dbCompetitionSeason);
					if (dbCompetition.CompetitionSeasons == null)
					{
						dbCompetition.CompetitionSeasons = new List<CompetitionSeason>();
					}
					dbCompetition.CompetitionSeasons.Add(dbCompetitionSeason);
					isChanged = true;
				}
			}
			if (isChanged)
			{
				dbContext.SaveChanges();
			}
		}

		private string GetCompetitionKey(Competition dbCompetition)
		{
			return $"{dbCompetition.Country.ApiFootballCountryName ?? "(null)"}_{dbCompetition.CompetitionName}".ToUpperInvariant(); ;
		}

		private string GetCompetitionKey(Feeds.LeaguesFeed.League league)
		{
			return $"{league.Country ?? "(null)"}_{league.Name}".ToUpperInvariant();
		}

		private string GetCompetitionSeasonKey(int competitionId, int season)
		{
			return $"{competitionId}_{season}".ToUpperInvariant();
		}
	}
}
