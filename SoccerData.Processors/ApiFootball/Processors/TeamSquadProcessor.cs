using SoccerData.Model;
using System.Collections.Generic;
using System.Linq;

namespace SoccerData.Processors.ApiFootball.Processors
{
	public class TeamSquadProcessor : IProcessor
	{
		private readonly int ApiFootballTeamId;
		private readonly int ApiFootballLeagueId;
		private CompetitionSeason DbCompetitionSeason;
		private readonly JsonUtility JsonUtility;

		private const int CacheDays = 30;

		public TeamSquadProcessor(int apiFootballTeamId, int apiFootballLeagueId, int? cacheLengthSec = CacheDays * 24 * 60 * 60)
		{
			this.ApiFootballTeamId = apiFootballTeamId;
			this.ApiFootballLeagueId = apiFootballLeagueId;
			this.DbCompetitionSeason = null;
			this.JsonUtility = new JsonUtility(cacheLengthSec, sourceType: JsonUtility.JsonSourceType.ApiFootball); // 230K+ FIXTURES.... SAVE FINISHED GAMES FOR A LONG TIME (120 DAYS?) TO AVOID QUOTA ISSUES
		}

		public TeamSquadProcessor(int apiFootballTeamId, CompetitionSeason dbCompetitionSeason, int? cacheLengthSec = CacheDays * 24 * 60 * 60)
		{
			this.ApiFootballTeamId = apiFootballTeamId;
			this.ApiFootballLeagueId = dbCompetitionSeason.ApiFootballId;
			this.DbCompetitionSeason = dbCompetitionSeason;
			this.JsonUtility = new JsonUtility(cacheLengthSec, sourceType: JsonUtility.JsonSourceType.ApiFootball); // 230K+ FIXTURES.... SAVE FINISHED GAMES FOR A LONG TIME (120 DAYS?) TO AVOID QUOTA ISSUES
		}

		public void Run(SoccerDataContext dbContext)
		{
			if (!this.DbCompetitionSeason.StartDate.HasValue || !this.DbCompetitionSeason.EndDate.HasValue)
			{
				// NULL DATES INDICATES NO GAMES YET.
				return;
			}

			if (this.DbCompetitionSeason == null)
			{
				this.DbCompetitionSeason = dbContext.CompetitionSeasons.Single(x => x.ApiFootballId == this.ApiFootballLeagueId);
			}

			var url = Feeds.TeamSquadFeed.GetUrlFromTeamIdAndSeasonYears(this.ApiFootballTeamId, this.DbCompetitionSeason.StartDate.Value, this.DbCompetitionSeason.EndDate.Value);
			var rawJson = JsonUtility.GetRawJsonFromUrl(url);
			var feed = Feeds.TeamSquadFeed.FromJson(rawJson);

			if (feed == null || feed.Result.Count == 0) { return; }

			int dbCompetitionSeasonId = this.DbCompetitionSeason.CompetitionSeasonId;
			var apiPlayerIds = feed.Result.Players.Select(x => x.PlayerId).ToList();
			var playerDict = dbContext.Players.Where(x => apiPlayerIds.Contains(x.ApiFootballId)).ToDictionary(x => x.ApiFootballId, y => y);
			var playerSeasonDict = dbContext.PlayerSeasons.Where(x => x.CompetitionSeasonId == dbCompetitionSeasonId && apiPlayerIds.Contains(x.Player.ApiFootballId)).ToDictionary(x => x.Player.ApiFootballId, y => y);

			bool hasUpdate = false;
			var apiPlayers = feed.Result.Players.ToList();
			foreach (var apiPlayer in apiPlayers)
			{
				bool isNewPlayer = false;
				PlayerSeason dbPlayerSeason = null;
				if (!playerDict.TryGetValue(apiPlayer.PlayerId, out Player dbPlayer))
				{
					dbPlayerSeason = new PlayerSeason
					{
						CompetitionSeasonId = this.DbCompetitionSeason.CompetitionSeasonId,
						Height = apiPlayer.HeightInCm,
						JerseyNumber = apiPlayer.Number,
						Position = apiPlayer.Position,
						Weight = apiPlayer.WeightInKg
					};

					dbPlayer = new Player
					{
						ApiFootballId = apiPlayer.PlayerId,
						BirthCity = apiPlayer.BirthPlace,
						BirthCountry = apiPlayer.BirthCountry,
						DateOfBirth = apiPlayer.BirthDate,
						FirstName = apiPlayer.Firstname,
						LastName = apiPlayer.Lastname,
						Nationality = apiPlayer.Nationality,
						PlayerName = apiPlayer.PlayerName,
						ApiFootballName = apiPlayer.PlayerName,
						PlayerSeasons = new List<PlayerSeason> { dbPlayerSeason }
					};
					playerDict.Add(apiPlayer.PlayerId, dbPlayer);
					playerSeasonDict.Add(apiPlayer.PlayerId, dbPlayerSeason);
					dbContext.Players.Add(dbPlayer);
					hasUpdate = true;
					isNewPlayer = true;
				}

				if (!isNewPlayer)
				{
					if ((!string.IsNullOrEmpty(apiPlayer.BirthPlace) && dbPlayer.BirthCity != apiPlayer.BirthPlace)
						|| (!string.IsNullOrEmpty(apiPlayer.BirthCountry) && dbPlayer.BirthCountry != apiPlayer.BirthCountry)
						|| (apiPlayer.BirthDate.HasValue && dbPlayer.DateOfBirth != apiPlayer.BirthDate)
						|| (!string.IsNullOrEmpty(apiPlayer.Firstname) && dbPlayer.FirstName != apiPlayer.Firstname)
						|| (!string.IsNullOrEmpty(apiPlayer.Lastname) && dbPlayer.LastName != apiPlayer.Lastname)
						|| (!string.IsNullOrEmpty(apiPlayer.PlayerName) && dbPlayer.ApiFootballName != apiPlayer.PlayerName)
						|| (!string.IsNullOrEmpty(apiPlayer.Nationality) && dbPlayer.Nationality != apiPlayer.Nationality))
					{
						// ALLOW PlayerName TO BE UPDATED AND NOT OVERWRITTEN
						dbPlayer.ApiFootballId = apiPlayer.PlayerId;
						dbPlayer.BirthCity = apiPlayer.BirthPlace;
						dbPlayer.BirthCountry = apiPlayer.BirthCountry;
						dbPlayer.DateOfBirth = apiPlayer.BirthDate;
						dbPlayer.FirstName = apiPlayer.Firstname;
						dbPlayer.LastName = apiPlayer.Lastname;
						dbPlayer.Nationality = apiPlayer.Nationality;
						dbPlayer.ApiFootballName = apiPlayer.PlayerName;
						if (string.IsNullOrEmpty(dbPlayer.PlayerName) && !string.IsNullOrEmpty(apiPlayer.PlayerName))
						{
							dbPlayer.PlayerName = apiPlayer.PlayerName;
						}
						hasUpdate = true;
					}

					if (!playerSeasonDict.TryGetValue(apiPlayer.PlayerId, out dbPlayerSeason))
					{
						dbPlayerSeason = new PlayerSeason
						{
							CompetitionSeasonId = this.DbCompetitionSeason.CompetitionSeasonId,
							Height = apiPlayer.HeightInCm,
							JerseyNumber = apiPlayer.Number,
							PlayerId = dbPlayer.PlayerId,
							Position = apiPlayer.Position,
							Weight = apiPlayer.WeightInKg
						};
						playerSeasonDict.Add(dbPlayer.ApiFootballId, dbPlayerSeason);
						dbContext.PlayerSeasons.Add(dbPlayerSeason);
						hasUpdate = true;
					}
					else if ((apiPlayer.WeightInKg.HasValue && apiPlayer.WeightInKg != dbPlayerSeason.Weight)
						|| (apiPlayer.HeightInCm.HasValue && apiPlayer.HeightInCm != dbPlayerSeason.Height)
						|| (apiPlayer.Number.HasValue && apiPlayer.Number != dbPlayerSeason.JerseyNumber)
						|| (!string.IsNullOrEmpty(apiPlayer.Position) && apiPlayer.Position != dbPlayerSeason.Position))
					{
						dbPlayerSeason.Weight = apiPlayer.WeightInKg;
						dbPlayerSeason.Height = apiPlayer.HeightInCm;
						dbPlayerSeason.JerseyNumber = apiPlayer.Number;
						dbPlayerSeason.Position = apiPlayer.Position;
						hasUpdate = true;
					}
				}
			}

			if (hasUpdate)
			{
				dbContext.SaveChanges();
			}
		}
	}
}
