using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SoccerData.Model;

namespace SoccerData.Processors.ApiFootball.Processors
{
	public class TeamProcessor : IProcessor
	{
		private readonly int ApiFootballTeamId;
		private readonly JsonUtility JsonUtility;

		public TeamProcessor(int apiFootballTeamId)
		{
			this.ApiFootballTeamId = apiFootballTeamId;
			this.JsonUtility = new JsonUtility(120 * 24 * 60 * 60, sourceType: JsonUtility.JsonSourceType.ApiFootball); // 230K+ FIXTURES.... SAVE FINISHED GAMES FOR A LONG TIME (120 DAYS?) TO AVOID QUOTA ISSUES
		}

		public void Run(SoccerDataContext dbContext)
		{
			var url = Feeds.TeamFeed.GetFeedUrlByTeamId(this.ApiFootballTeamId);
			var rawJson = JsonUtility.GetRawJsonFromUrl(url);
			var feed = Feeds.TeamFeed.FromJson(rawJson);

			if (feed?.Result == null || feed.Result.Count == 0) { throw new ArgumentException($"TEAM DOES NOT EXIST IN API: {this.ApiFootballTeamId}"); }

			var feedTeam = feed.Result.Teams.Single();

			var dbCountry = dbContext.Countries.SingleOrDefault(x => x.ApiFootballCountryName == (feedTeam.Country ?? "World").Replace(' ', '-'));
			if (dbCountry == null)
			{
				dbCountry = new Country
				{
					ApiFootballCountryName = feedTeam.Country.Replace(' ', '-'),
					CountryName = feedTeam.Country,
					CountryAbbr = feedTeam.Code
				};
			}

			var dbTeam = dbContext.Teams.SingleOrDefault(x => x.ApiFootballId == this.ApiFootballTeamId);
			if (dbTeam == null)
			{
				dbTeam = new Team
				{
					ApiFootballId = this.ApiFootballTeamId,
					Country = dbCountry,
					LogoUrl = feedTeam.Logo,
					TeamName = feedTeam.Name,
					YearFounded = feedTeam.Founded
				};
				dbContext.Teams.Add(dbTeam);
				dbContext.SaveChanges();
			}
			else
			{
				if (dbTeam.TeamName != feedTeam.Name
					|| dbTeam.LogoUrl != feedTeam.Logo
					|| (!dbTeam.YearFounded.HasValue && feedTeam.Founded.HasValue))
				{
					dbTeam.TeamName = feedTeam.Name;
					dbTeam.LogoUrl = feedTeam.Logo;
					dbTeam.YearFounded = feedTeam.Founded;
					dbContext.SaveChanges();
				}
			}
		}
	}
}
