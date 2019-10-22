using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SoccerData.Model;

namespace SoccerData.Processors.ApiFootball.Processors
{
	public class LeaguesProcessor : IProcessor
	{
		public void Run(SoccerDataContext dbContext)
		{
			var url = Feeds.LeaguesFeed.GetFeedUrl();
			var rawJson = JsonUtility.GetRawJsonFromUrl(url);
			var feed = Feeds.LeaguesFeed.FromJson(rawJson);

			var orderedLeagues = feed.Result.Leagues.OrderBy(x => x.CountryCode).ThenBy(x => x.Name).ToList();

			var existingLeagues = dbContext.Competitions.ToDictionary(x => GetCompetitionKey(x), y => y);

			var countryDict = dbContext.Countries.ToDictionary(x => x.CountryAbbr ?? "(null)", y => y.CountryId);

			foreach (var league in orderedLeagues)
			{
				var key = GetCompetitionKey(league);
				if (!existingLeagues.ContainsKey(key))
				{
					var dbCompetition = new Competition
					{
						ApiFootballId = league.LeagueId,
						CompetitionName = league.Name.Trim(),
						CompetitionType = league.Type.Trim(),
						CountryId = countryDict[league.CountryCode ?? "(null)"],
						LogoUrl = league.Logo?.ToString()
					};

					existingLeagues.Add(key, dbCompetition);
					dbContext.Competitions.Add(dbCompetition);
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
	}
}
