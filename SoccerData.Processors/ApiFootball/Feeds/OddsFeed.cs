using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SoccerData.Processors.ApiFootball.Feeds
{
	public class OddsFeed
	{
		public static string GetFeedUrlByDate(DateTime date, int? page = null)
		{
			if (page.HasValue)
			{
				return $"https://api-football-v1.p.rapidapi.com/v2/odds/date/{date.ToString("yyyy-MM-dd")}?page={page}";
			}
			return $"https://api-football-v1.p.rapidapi.com/v2/odds/date/{date.ToString("yyyy-MM-dd")}";
		}

		public static string GetFeedUrlByFixtureId(int apiFixtureId, int? page = null)
		{
			if (page.HasValue)
			{
				return $"https://api-football-v1.p.rapidapi.com/v2/odds/fixture/{apiFixtureId}?page={page}";
			}
			return $"https://api-football-v1.p.rapidapi.com/v2/odds/fixture/{apiFixtureId}";
		}

		public static string GetFeedUrlByLeagueId(int apiLeagueId, int? page = null)
		{
			if (page.HasValue)
			{
				return $"https://api-football-v1.p.rapidapi.com/v2/odds/league/{apiLeagueId}?page={page}";
			}
			return $"https://api-football-v1.p.rapidapi.com/v2/odds/league/{apiLeagueId}";
		}

		public static OddsFeed FromJson(string json) => JsonConvert.DeserializeObject<OddsFeed>(json, Converter.Settings);

		[JsonProperty("api")]
		public ApiResult Result { get; set; }

		public class ApiResult
		{
			[JsonProperty("results")]
			public int Count { get; set; }

			[JsonProperty("paging")]
			public ApiPaging Paging { get; set; }

			[JsonProperty("odds")]
			public List<ApiOddsEntry> OddsEntries { get; set; }
		}

		public class ApiOddsEntry
		{
			[JsonProperty("fixture")]
			public ApiOddsFixture Fixture { get; set; }

			[JsonProperty("bookmakers")]
			public List<ApiOddsBookmaker> Bookmakers { get; set; }
		}

		public class ApiOddsBookmaker
		{
			[JsonProperty("bookmaker_id")]
			public int BookmakerId { get; set; }

			[JsonProperty("bookmaker_name")]
			public string BookmakerName { get; set; }

			[JsonProperty("bets")]
			public List<ApiOddsListing> BetListings { get; set; }
		}

		public class ApiOddsListing
		{
			[JsonProperty("label_id")]
			public int LabelId { get; set; }

			[JsonProperty("label_name")]
			public string LabelName { get; set; }

			[JsonProperty("values")]
			public List<ApiOddsListingOption> ListingOptions { get; set; }
		}

		public class ApiOddsListingOption
		{
			[JsonProperty("value")]
			public string OptionName { get; set; }

			[JsonProperty("odd")]
			public string Payout { get; set; }
		}

		public class ApiOddsFixture
		{
			[JsonProperty("league_id")]
			public int LeagueId { get; set; }

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
		public static string ToJson(this OddsFeed self) => JsonConvert.SerializeObject(self, Converter.Settings);
	}
}
