using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SoccerData.Processors.ApiFootball.Feeds
{
	public class OddsBookmakersFeed
	{
		public static string GetFeedUrl()
		{
			return $"https://api-football-v1.p.rapidapi.com/v2/odds/bookmakers/";
		}

		public static OddsBookmakersFeed FromJson(string json) => JsonConvert.DeserializeObject<OddsBookmakersFeed>(json, Converter.Settings);

		[JsonProperty("api")]
		public ApiResult Result { get; set; }

		public class ApiResult
		{
			[JsonProperty("results")]
			public int Count { get; set; }

			[JsonProperty("bookmakers")]
			public List<ApiBookmaker> Bookmakers { get; set; }
		}

		public class ApiBookmaker
		{
			[JsonProperty("id")]
			public int Id { get; set; }

			[JsonProperty("name")]
			public string Name { get; set; }
		}
	}

	public static partial class Serialize
	{
		public static string ToJson(this OddsBookmakersFeed self) => JsonConvert.SerializeObject(self, Converter.Settings);
	}
}
