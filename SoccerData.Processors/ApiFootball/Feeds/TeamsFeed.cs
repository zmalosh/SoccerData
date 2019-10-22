using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SoccerData.Processors.ApiFootball.Feeds
{
	public class TeamsFeed
	{
		public static string GetFeedUrlByLeagueId(int apiFootballLeagueId)
		{
			return $"https://api-football-v1.p.rapidapi.com/v2/teams/league/{apiFootballLeagueId}";
		}

		public static string GetFeedUrlByTeamId(int apiFootballTeamId)
		{
			return $"https://api-football-v1.p.rapidapi.com/v2/teams/team/{apiFootballTeamId}";
		}

		public static TeamsFeed FromJson(string json) => JsonConvert.DeserializeObject<TeamsFeed>(json, Converter.Settings);

		[JsonProperty("api")]
		public ApiResult Api { get; set; }

		public class ApiResult
		{
			[JsonProperty("results")]
			public long Results { get; set; }

			[JsonProperty("teams")]
			public List<Team> Teams { get; set; }
		}

		public class Team
		{
			[JsonProperty("team_id")]
			public long TeamId { get; set; }

			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("code")]
			public string Code { get; set; }

			[JsonProperty("logo")]
			public Uri Logo { get; set; }

			[JsonProperty("country")]
			public string Country { get; set; }

			[JsonProperty("founded")]
			public long Founded { get; set; }

			[JsonProperty("venue_name")]
			public string VenueName { get; set; }

			[JsonProperty("venue_surface")]
			public string VenueSurface { get; set; }

			[JsonProperty("venue_address")]
			public string VenueAddress { get; set; }

			[JsonProperty("venue_city")]
			public string VenueCity { get; set; }

			[JsonProperty("venue_capacity")]
			public long VenueCapacity { get; set; }
		}
	}

	public static partial class Serialize
	{
		public static string ToJson(this TeamsFeed self) => JsonConvert.SerializeObject(self, Converter.Settings);
	}
}
