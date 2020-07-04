using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Processors.ApiFootball.Feeds
{
	class OddsFixtureMappingsFeed
	{
		public static string GetFeedUrl(int? page = null)
		{
			return $"https://api-football-v1.p.rapidapi.com/v2/odds/bookmakers/";
		}

		public static OddsFixtureMappingsFeed FromJson(string json) => JsonConvert.DeserializeObject<OddsFixtureMappingsFeed>(json, Converter.Settings);

		[JsonProperty("api")]
		public ApiResult Result { get; set; }

		public class ApiResult
		{
			[JsonProperty("results")]
			public int Count { get; set; }

			[JsonProperty("paging")]
			public ApiPaging Paging { get; set; }

			[JsonProperty("mapping")]
			public List<ApiFixtureMapping> FixtureMappings { get; set; }
		}

		public class ApiFixtureMapping
		{
			[JsonProperty("fixture_id")]
			public int FixtureId { get; set; }

			[JsonProperty("updateAt")]
			public long UpdateAt { get; set; }
		}

		public class ApiPaging
		{
			[JsonProperty("current")]
			public int CurrentPageNumber { get; set; }

			[JsonProperty("total")]
			public int TotalPageCount { get; set; }
		}
	}

	public static partial class Serialize
	{
		public static string ToJson(this OddsFixtureMappingsFeed self) => JsonConvert.SerializeObject(self, Converter.Settings);
	}
}
