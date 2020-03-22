using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SoccerData.Processors.ApiFootball.Feeds
{
	public class LeaguesFeed
	{
		public static string GetFeedUrl()
		{
			return "https://api-football-v1.p.rapidapi.com/v2/leagues";
		}
		public static LeaguesFeed FromJson(string json) => JsonConvert.DeserializeObject<LeaguesFeed>(json, Converter.Settings);

		[JsonProperty("api")]
		public ApiResult Result { get; set; }

		public partial class ApiResult
		{
			[JsonProperty("results")]
			public int Count { get; set; }

			[JsonProperty("leagues")]
			public List<League> Leagues { get; set; }
		}

		public partial class League
		{
			[JsonProperty("league_id")]
			public int LeagueId { get; set; }

			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("type")]
			public string Type { get; set; }

			[JsonProperty("country")]
			public string Country { get; set; }

			[JsonProperty("country_code")]
			public string CountryCode { get; set; }

			[JsonProperty("season")]
			public int Season { get; set; }

			[JsonProperty("season_start")]
			public DateTime? SeasonStart { get; set; }

			[JsonProperty("season_end")]
			public DateTime? SeasonEnd { get; set; }

			[JsonProperty("logo")]
			public string Logo { get; set; }

			[JsonProperty("flag")]
			public Uri Flag { get; set; }

			[JsonIgnore]
			public bool Standings { get { return this._standings == 1; } }

			[JsonProperty("standings")]
			private int _standings { get; set; }

			[JsonIgnore]
			public bool IsCurrent { get { return this._isCurrent == 1; } }

			[JsonProperty("is_current")]
			private int _isCurrent { get; set; }

			[JsonProperty("coverage")]
			public Coverage Coverage { get; set; }
		}

		public partial class Coverage
		{
			[JsonProperty("fixtures")]
			public Fixtures Fixtures { get; set; }

			[JsonProperty("standings")]
			public bool Standings { get; set; }

			[JsonProperty("players")]
			public bool Players { get; set; }

			[JsonProperty("topScorers")]
			public bool TopScorers { get; set; }

			[JsonProperty("predictions")]
			public bool Predictions { get; set; }

			[JsonProperty("odds")]
			public bool Odds { get; set; }
		}

		public partial class Fixtures
		{
			[JsonProperty("events")]
			public bool Events { get; set; }

			[JsonProperty("lineups")]
			public bool Lineups { get; set; }

			[JsonProperty("statistics")]
			public bool TeamStatistics { get; set; }

			[JsonProperty("players_statistics")]
			public bool PlayersStatistics { get; set; }
		}
	}

	public static partial class Serialize
	{
		public static string ToJson(this LeaguesFeed self) => JsonConvert.SerializeObject(self, Converter.Settings);
	}
}
