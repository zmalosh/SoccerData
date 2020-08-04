using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SoccerData.Processors.ApiFootball.Feeds
{
	public class OddsLabelsFeed
	{
		public static string GetFeedUrl()
		{
			return $"https://api-football-v1.p.rapidapi.com/v2/odds/labels/";
		}

		public static OddsLabelsFeed FromJson(string json) => JsonConvert.DeserializeObject<OddsLabelsFeed>(json, Converter.Settings);

		[JsonProperty("api")]
		public ApiResult Result { get; set; }

		public class ApiResult
		{
			[JsonProperty("results")]
			public int Count { get; set; }

			[JsonProperty("labels")]
			public List<ApiOddsLabel> OddsLabels { get; set; }
		}

		public class ApiOddsLabel
		{
			[JsonProperty("id")]
			public int Id { get; set; }

			[JsonProperty("label")]
			public string Name { get; set; }
		}
	}

	public static partial class Serialize
	{
		public static string ToJson(this OddsLabelsFeed self) => JsonConvert.SerializeObject(self, Converter.Settings);
	}
}
