using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Processors.ApiFootball.Feeds
{
	public class FixturesFeed
	{
		public static string GetFeedUrlByLeagueId(int apiFootballLeagueId)
		{
			return $"https://api-football-v1.p.rapidapi.com/v2/fixtures/league/{apiFootballLeagueId}";
		}

		public static string GetFeedUrlByTeamId(int apiFootballTeamId)
		{
			return $"https://api-football-v1.p.rapidapi.com/v2/fixtures/team/{apiFootballTeamId}";
		}

		public static FixturesFeed FromJson(string json) => JsonConvert.DeserializeObject<FixturesFeed>(json, Converter.Settings);

		[JsonProperty("api")]
		public ApiResult Result { get; set; }

		public class ApiResult
		{
			[JsonProperty("results")]
			public int Count { get; set; }

			[JsonProperty("fixtures")]
			public List<Fixture> Fixtures { get; set; }
		}

		public class Fixture
		{
			[JsonProperty("fixture_id")]
			public int FixtureId { get; set; }

			[JsonProperty("league_id")]
			public int LeagueId { get; set; }

			[JsonProperty("event_date")]
			public DateTimeOffset? EventDate { get; set; }

			[JsonIgnore]
			public DateTimeOffset? EventTimestamp
			{
				get
				{
					return this._firstHalfStart.HasValue
							? DateTimeOffset.FromUnixTimeSeconds(this._eventTimestamp.Value)
							: (DateTimeOffset?)null;
				}
			}

			[JsonProperty("event_timestamp")]
			private int? _eventTimestamp { get; set; }

			[JsonIgnore]
			public DateTimeOffset? FirstHalfStart
			{
				get
				{
					return this._firstHalfStart.HasValue
							? DateTimeOffset.FromUnixTimeSeconds(this._firstHalfStart.Value)
							: (DateTimeOffset?)null;
				}
			}

			[JsonProperty("firstHalfStart")]
			private int? _firstHalfStart { get; set; }

			[JsonIgnore]
			public DateTimeOffset? SecondHalfStart
			{
				get
				{
					return this._secondHalfStart.HasValue
							? DateTimeOffset.FromUnixTimeSeconds(this._secondHalfStart.Value)
							: (DateTimeOffset?)null;
				}
			}

			[JsonProperty("secondHalfStart")]
			private int? _secondHalfStart { get; set; }

			[JsonProperty("round")]
			public string Round { get; set; }

			[JsonProperty("status")]
			public string Status { get; set; }

			[JsonProperty("statusShort")]
			public string StatusShort { get; set; }

			[JsonProperty("elapsed")]
			public int? Elapsed { get; set; }

			[JsonProperty("venue")]
			public string Venue { get; set; }

			[JsonIgnore]
			public string Referee { get { return string.IsNullOrEmpty(this._referee) || this._referee == "(null)" ? null : this._referee; } }

			[JsonProperty("referee")]
			private string _referee { get; set; }

			[JsonProperty("homeTeam")]
			public Team HomeTeam { get; set; }

			[JsonProperty("awayTeam")]
			public Team AwayTeam { get; set; }

			[JsonProperty("goalsHomeTeam")]
			public int? GoalsHomeTeam { get; set; }

			[JsonProperty("goalsAwayTeam")]
			public int? GoalsAwayTeam { get; set; }

			[JsonProperty("score")]
			public Score Score { get; set; }
		}

		public class Team
		{
			[JsonProperty("team_id")]
			public int TeamId { get; set; }

			[JsonProperty("team_name")]
			public string TeamName { get; set; }

			[JsonProperty("logo")]
			public string Logo { get; set; }
		}

		public class Score
		{
			[JsonIgnore]
			public string Halftime { get { return string.IsNullOrEmpty(this._halftime) || this._halftime == "(null)" ? null : this._halftime; } }

			[JsonProperty("halftime")]
			private string _halftime { get; set; }

			[JsonIgnore]
			public string Fulltime { get { return string.IsNullOrEmpty(this._fulltime) || this._fulltime == "(null)" ? null : this._fulltime; } }

			[JsonProperty("fulltime")]
			private string _fulltime { get; set; }

			[JsonIgnore]
			public string ExtraTime { get { return string.IsNullOrEmpty(this._extraTime) || this._extraTime == "(null)" ? null : this._extraTime; } }

			[JsonProperty("extratime")]
			private string _extraTime { get; set; }

			[JsonIgnore]
			public string Penalty { get { return string.IsNullOrEmpty(this._penalty) || this._penalty == "(null)" ? null : this._penalty; } }

			[JsonProperty("penalty")]
			private string _penalty { get; set; }
		}
	}

	public static partial class Serialize
	{
		public static string ToJson(this FixturesFeed self) => JsonConvert.SerializeObject(self, Converter.Settings);
	}
}
