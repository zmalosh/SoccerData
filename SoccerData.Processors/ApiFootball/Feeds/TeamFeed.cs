using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Processors.ApiFootball.Feeds
{
	public class TeamFeed
	{
		public static string GetFeedUrlByFixtureId(int apiFootballTeamId)
		{
			return $"https://api-football-v1.p.rapidapi.com/v2/teams/team/{apiFootballTeamId}";
		}

		[JsonProperty("api")]
		public ApiResult Result { get; set; }

		public class ApiResult
		{
			[JsonProperty("results")]
			public int Count { get; set; }

			[JsonProperty("teams")]
			public List<ApiTeam> Teams { get; set; }
		}

		public class ApiTeam
		{
			[JsonProperty("team_id")]
			public int TeamId { get; set; }

			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("code")]
			public string Code { get; set; }

			[JsonProperty("logo")]
			public string Logo { get; set; }

			[JsonProperty("country")]
			public string Country { get; set; }

			[JsonProperty("is_national")]
			public bool IsNational { get; set; }

			[JsonProperty("founded")]
			public int? Founded { get; set; }

			[JsonProperty("venue_name")]
			public string VenueName { get; set; }

			[JsonProperty("venue_surface")]
			public string VenueSurface { get; set; }

			[JsonProperty("venue_address")]
			public string VenueAddress { get; set; }

			[JsonProperty("venue_city")]
			public string VenueCity { get; set; }

			[JsonProperty("venue_capacity")]
			public int? VenueCapacity { get; set; }
		}

		public static TeamFeed FromJson(string json) => JsonConvert.DeserializeObject<TeamFeed>(json, Converter.Settings);
	}

	public static partial class Serialize
	{
		public static string ToJson(this TeamFeed self) => JsonConvert.SerializeObject(self, Converter.Settings);
	}
}
