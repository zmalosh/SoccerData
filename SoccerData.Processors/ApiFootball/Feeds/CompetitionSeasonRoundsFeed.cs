using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Processors.ApiFootball.Feeds
{
	public class CompetitionSeasonRoundsFeed
	{
		public static string GetFeedUrlByLeagueId(int apiFootballLeagueId)
		{
			return $"https://api-football-v1.p.rapidapi.com/v2/fixtures/rounds/{apiFootballLeagueId}";
		}

		public static CompetitionSeasonRoundsFeed FromJson(string json) => JsonConvert.DeserializeObject<CompetitionSeasonRoundsFeed>(json, Converter.Settings);

		[JsonProperty("api")]
		public ApiResult Result { get; set; }

		public partial class ApiResult
		{
			[JsonProperty("results")]
			public int Count { get; set; }

			[JsonProperty("fixtures")]
			public List<string> Rounds { get; set; }
		}
	}

	public static partial class Serialize
	{
		public static string ToJson(this CompetitionSeasonRoundsFeed self) => JsonConvert.SerializeObject(self, Converter.Settings);
	}
}
