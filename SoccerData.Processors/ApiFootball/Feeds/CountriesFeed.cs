using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SoccerData.Processors.ApiFootball.Feeds
{
	public class CountriesFeed
	{
		public static string GetFeedUrl()
		{
			return "https://api-football-v1.p.rapidapi.com/v2/countries";
		}

		public static CountriesFeed FromJson(string json) => JsonConvert.DeserializeObject<CountriesFeed>(json, Converter.Settings);

		[JsonProperty("api")]
		public ApiResult Result { get; set; }

		public class ApiResult
		{
			[JsonProperty("results")]
			public int Count { get; set; }

			[JsonProperty("countries")]
			public List<Country> Countries { get; set; }
		}

		public class Country
		{
			[JsonProperty("country")]
			public string CountryName { get; set; }

			[JsonProperty("code")]
			public string Code { get; set; }

			[JsonProperty("flag")]
			public Uri Flag { get; set; }
		}
	}

	public static partial class Serialize
	{
		public static string ToJson(this CountriesFeed self) => JsonConvert.SerializeObject(self, Converter.Settings);
	}
}