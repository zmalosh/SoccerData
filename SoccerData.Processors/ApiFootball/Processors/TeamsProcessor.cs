using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SoccerData.Model;

namespace SoccerData.Processors.ApiFootball.Processors
{
	public class TeamsProcessor : IProcessor
	{
		private int CompetitionSeasonId;
		private readonly JsonUtility JsonUtility;

		private const string WorldCountryName = "WORLD";

		public TeamsProcessor(int competitionSeasonId)
		{
			this.CompetitionSeasonId = competitionSeasonId;
			this.JsonUtility = new JsonUtility(7 * 24 * 60 * 60, sourceType: JsonUtility.JsonSourceType.ApiFootball);
		}

		public void Run(SoccerDataContext dbContext)
		{
			var dbCompetitionSeason = dbContext.CompetitionSeasons.Include(x => x.Competition.Country).SingleOrDefault(x => x.CompetitionSeasonId == this.CompetitionSeasonId);
			if (dbCompetitionSeason == null)
			{
				return;
			}

			Console.WriteLine($"{dbCompetitionSeason.CompetitionSeasonId}-{dbCompetitionSeason.Season}-{dbCompetitionSeason.Competition.CompetitionName}");

			var url = Feeds.TeamsFeed.GetFeedUrlByLeagueId(dbCompetitionSeason.ApiFootballId);
			var rawJson = this.JsonUtility.GetRawJsonFromUrl(url);
			var feed = Feeds.TeamsFeed.FromJson(rawJson);

			var teamDict = dbContext.Teams.Include(x => x.TeamSeasons).ToDictionary(x => x.ApiFootballId);
			var venueDict = dbContext.Venues.Include(x => x.VenueSeasons)
											.ToDictionary(x => GetVenueKey(x.VenueName, x.VenueAddress));

			var worldCountryId = dbContext.Countries.Single(x => x.ApiFootballCountryName.ToUpper() == WorldCountryName).CountryId;

			int competitionSeasonId = dbCompetitionSeason.CompetitionSeasonId;
			int competitionCountryId = dbCompetitionSeason.Competition?.CountryId ?? worldCountryId;

			var feedTeams = feed.Result.Teams.OrderBy(x => x.Name).ToList();

			bool hasUpdates = false;
			foreach (var feedTeam in feed.Result.Teams)
			{
				Venue dbVenue = null;
				VenueSeason dbVenueSeason = null;
				#region VENUE SETTING
				string venueName = System.Web.HttpUtility.HtmlDecode(feedTeam.VenueName);
				string venueAddress = System.Web.HttpUtility.HtmlDecode(feedTeam.VenueAddress);
				string venueCity = System.Web.HttpUtility.HtmlDecode(feedTeam.VenueCity);

				var venueKey = GetVenueKey(venueName, venueAddress);
				if (!string.IsNullOrEmpty(venueKey))
				{
					if (!venueDict.TryGetValue(venueKey, out dbVenue))
					{
						dbVenue = new Venue
						{
							Capacity = feedTeam.VenueCapacity,
							SurfaceType = feedTeam.VenueSurface,
							VenueAddress = venueAddress,
							VenueCity = venueCity,
							VenueName = venueName,
							VenueNation = feedTeam.Country
						};
						venueDict.Add(venueKey, dbVenue);
						dbContext.Venues.Add(dbVenue);
					}

					if (dbVenue.VenueSeasons == null)
					{
						dbVenue.VenueSeasons = new List<VenueSeason>();
					}

					dbVenueSeason = dbVenue.VenueSeasons.SingleOrDefault(x => x.Season == dbCompetitionSeason.Season);
					if (dbVenueSeason == null)
					{
						dbVenueSeason = new VenueSeason
						{
							Capacity = feedTeam.VenueCapacity,
							Season = dbCompetitionSeason.Season,
							VenueName = venueName,
							SurfaceType = feedTeam.VenueSurface
						};
						dbVenue.VenueSeasons.Add(dbVenueSeason);
					}
				}
				#endregion VENUE SETTING
								
				if (!teamDict.TryGetValue(feedTeam.TeamId, out Team dbTeam))
				{
					dbTeam = new Team
					{
						ApiFootballId = feedTeam.TeamId,
						CountryName = feedTeam.Country,
						LogoUrl = feedTeam.Logo.ToString(),
						TeamName = feedTeam.Name,
						YearFounded = feedTeam.Founded,
						TeamSeasons = new List<TeamSeason>()
					};
					hasUpdates = true;
					teamDict.Add(feedTeam.TeamId, dbTeam);
					dbContext.Teams.Add(dbTeam);
				}

				if (dbTeam.TeamSeasons == null)
				{
					dbTeam.TeamSeasons = new List<TeamSeason>();
				}

				var dbTeamSeason = dbTeam.TeamSeasons.SingleOrDefault(x => x.CompetitionSeasonId == competitionSeasonId);
				if (dbTeamSeason == null)
				{
					dbTeamSeason = new TeamSeason
					{
						CompetitionSeasonId = competitionSeasonId,
						LogoUrl = feedTeam.Logo.ToString(),
						Season = dbCompetitionSeason.Season,
						TeamName = feedTeam.Name,
						VenueSeason = dbVenueSeason
					};
					hasUpdates = true;
					dbTeam.TeamSeasons.Add(dbTeamSeason);
				}
			}

			if (hasUpdates)
			{
				dbContext.SaveChanges();
			}
		}

		private string GetVenueKey(string venueName, string venueAddress)
		{
			if (string.IsNullOrEmpty(venueName))
			{
				return null;
			}
			string key = $"{venueName}_{venueAddress}";
			key = key.Replace(" ", string.Empty).Replace("-", string.Empty);
			return key.ToUpperInvariant();
		}
	}
}
