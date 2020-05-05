using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SoccerData.Model;

namespace SoccerData.Processors.ApiFootball.Processors
{
	public class TransfersProcessor : IProcessor
	{
		private readonly int ApiFootballTeamId;
		private readonly JsonUtility JsonUtility;

		public TransfersProcessor(int apiFootballTeamId)
		{
			this.ApiFootballTeamId = apiFootballTeamId;
			this.JsonUtility = new JsonUtility(120 * 24 * 60 * 60, sourceType: JsonUtility.JsonSourceType.ApiFootball);
		}

		public void Run(SoccerDataContext dbContext)
		{
			var url = Feeds.TransfersFeed.GetUrlFromTeamId(this.ApiFootballTeamId);
			var rawJson = JsonUtility.GetRawJsonFromUrl(url);
			var feed = Feeds.TransfersFeed.FromJson(rawJson);

			if (feed?.Result?.Transfers == null || feed.Result.Transfers.Count == 0) { return; } // TRANSFERS NOT SUPPORTED

			var apiTransfers = feed.Result.Transfers.Distinct(new Feeds.TransfersFeed.ApiTransferEqualityComparer()).ToList();

			#region ADD MISSING TEAMS
			var apiTeams = apiTransfers.SelectMany(x => new[] { x.TeamIn, x.TeamOut })
										.Where(x => x.TeamId.HasValue)
										.Distinct(new Feeds.TransfersFeed.ApiTeamEqualityComparer())
										.ToList();
			var apiTeamIds = apiTeams.Select(x => x.TeamId.Value);
			var teamsDict = dbContext.Teams.Where(x => apiTeamIds.Contains(x.ApiFootballId)).ToDictionary(x => x.ApiFootballId, y => y.TeamId);
			var apiMissingTeamIds = apiTeamIds.Where(x => !teamsDict.ContainsKey(x)).ToList();
			if (apiMissingTeamIds != null && apiMissingTeamIds.Count != 0)
			{
				foreach (var apiMissingTeamId in apiMissingTeamIds)
				{
					var teamProcessor = new Processors.TeamProcessor(apiMissingTeamId);
					teamProcessor.Run(dbContext);
				}
				dbContext.SaveChanges();
				teamsDict = dbContext.Teams.Where(x => apiTeamIds.Contains(x.ApiFootballId)).ToDictionary(x => x.ApiFootballId, y => y.TeamId);
			}
			#endregion ADD MISSING TEAMS

			#region ADD MISSING PLAYERS
			var apiPlayers = apiTransfers.Select(x => new { PlayerId = x.PlayerId, PlayerName = x.PlayerName }).Distinct().ToList();
			var apiPlayerIds = apiPlayers.Select(x => x.PlayerId).Distinct().ToList();
			var playersDict = dbContext.Players.Where(x => apiPlayerIds.Contains(x.ApiFootballId)).ToDictionary(x => x.ApiFootballId, y => y.PlayerId);
			var apiMissingPlayerIds = apiPlayerIds.Where(x => !playersDict.ContainsKey(x)).ToList();
			if (apiMissingPlayerIds != null && apiMissingPlayerIds.Count != 0)
			{
				foreach (var apiMissingPlayerId in apiMissingPlayerIds)
				{
					var apiPlayer = apiPlayers.First(x => x.PlayerId == apiMissingPlayerId);
					var dbPlayer = new Player
					{
						ApiFootballId = apiPlayer.PlayerId,
						ApiFootballName = apiPlayer.PlayerName,
						PlayerName = apiPlayer.PlayerName
					};
					dbContext.Players.Add(dbPlayer);
				}
				dbContext.SaveChanges();
				playersDict = dbContext.Players.Where(x => apiPlayerIds.Contains(x.ApiFootballId)).ToDictionary(x => x.ApiFootballId, y => y.PlayerId);
			}
			#endregion ADD MISSING PLAYERS

			var requestedTeamId = teamsDict[this.ApiFootballTeamId];
			var dbTransfers = dbContext.Transfers.Where(x => x.DestTeamId == requestedTeamId || x.SourceTeamId == requestedTeamId).ToList();

			if (dbTransfers.Count == apiTransfers.Count) { return; }

			foreach (var apiTransfer in apiTransfers)
			{
				int? apiTeamOutId = apiTransfer.TeamOut?.TeamId;
				int? apiTeamInId = apiTransfer.TeamIn?.TeamId;

				var dbPlayerId = playersDict[apiTransfer.PlayerId];
				var dbSourceTeamId = apiTeamOutId.HasValue ? teamsDict[apiTeamOutId.Value] : (int?)null;
				var dbDestTeamId = apiTeamInId.HasValue ? teamsDict[apiTeamInId.Value] : (int?)null;
				var dbTransfer = dbTransfers.SingleOrDefault(x => x.PlayerId == dbPlayerId
																	&& x.SourceTeamId == dbSourceTeamId
																	&& x.DestTeamId == dbDestTeamId
																	&& x.TransferDate?.Date == apiTransfer.TransferDate?.Date
																	&& x.TransferType == apiTransfer.TransferType);
				if (dbTransfer == null)
				{
					dbTransfer = new Transfer
					{
						DestTeamId = dbDestTeamId,
						SourceTeamId = dbSourceTeamId,
						PlayerId = dbPlayerId,
						TransferDate = apiTransfer.TransferDate?.Date,
						TransferType = apiTransfer.TransferType
					};
					dbContext.Transfers.Add(dbTransfer);
				}
				else
				{
					dbTransfers.Remove(dbTransfer);
				}
			}
			if (dbTransfers.Count > 0)
			{
				dbContext.Transfers.RemoveRange(dbTransfers);
			}
			dbContext.SaveChanges();
		}
	}
}
