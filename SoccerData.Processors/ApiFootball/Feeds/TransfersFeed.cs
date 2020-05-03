using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Processors.ApiFootball.Feeds
{
	public class TransfersFeed
	{
		public static string GetUrlFromTeamId(int apiFootballTeamId)
		{
			return $"https://api-football-v1.p.rapidapi.com/v2/players/fixture/{apiFootballTeamId}";
		}

		[JsonProperty("api")]
		public ApiResult Api { get; set; }

		public class ApiResult
		{
			[JsonProperty("results")]
			public int Count { get; set; }

			[JsonProperty("transfers")]
			public List<Transfer> Transfers { get; set; }
		}

		public class Transfer
		{
			[JsonProperty("player_id")]
			public int PlayerId { get; set; }

			[JsonProperty("player_name")]
			public string PlayerName { get; set; }

			[JsonProperty("transfer_date")]
			public DateTime TransferDate { get; set; }

			[JsonProperty("type")]
			public string Type { get; set; }

			[JsonProperty("team_in")]
			public Team TeamIn { get; set; }

			[JsonProperty("team_out")]
			public Team TeamOut { get; set; }

			[JsonProperty("lastUpdate")]
			public int LastUpdate { get; set; }
		}

		public class Team
		{
			[JsonProperty("team_id")]
			public int? TeamId { get; set; }

			[JsonProperty("team_name")]
			public string TeamName { get; set; }
		}

		public static TransfersFeed FromJson(string json) => JsonConvert.DeserializeObject<TransfersFeed>(json, Converter.Settings);
	}

	public static partial class Serialize
	{
		public static string ToJson(this TransfersFeed self) => JsonConvert.SerializeObject(self, Converter.Settings);
	}
}
