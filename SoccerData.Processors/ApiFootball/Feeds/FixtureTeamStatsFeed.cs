using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Processors.ApiFootball.Feeds
{
	public class FixtureTeamStatsFeed
	{
		public static string GetFeedUrlByFixtureId(int apiFootballFixtureId)
		{
			return $"https://api-football-v1.p.rapidapi.com/v2/statistics/fixture/{apiFootballFixtureId}";
		}

		public static FixtureTeamStatsFeed FromJson(string json) => JsonConvert.DeserializeObject<FixtureTeamStatsFeed>(json, Converter.Settings);

		[JsonProperty("api")]
		public ApiResultWrapper ResultWrapper { get; set; }

		public class ApiResultWrapper
		{
			[JsonProperty("results")]
			public long Results { get; set; }

			[JsonProperty("statistics")]
			public Dictionary<string, Statistic> Statistics { get; set; }
		}

		public class Statistic
		{
			[JsonProperty("home")]
			public string Home { get; set; }

			[JsonProperty("away")]
			public string Away { get; set; }
		}

		public static class StatKeys
		{
			public const string ShotsOnGoal = "Shots on Goal";
			public const string ShotsOffGoal = "Shots off Goal";
			public const string TotalShots = "Total Shots";
			public const string BlockedShots = "Blocked Shots";
			public const string ShotsInsideBox = "Shots insidebox";
			public const string ShotsOutsideBox = "Shots outsidebox";
			public const string FoulsCommitted = "Fouls";
			public const string CornerKicks = "Corner Kicks";
			public const string Offsides = "Offsides";
			public const string BallPossession = "Ball Possession";
			public const string YellowCards = "Yellow Cards";
			public const string RedCards = "Red Cards";
			public const string GoalkeeperSaves = "Goalkeeper Saves";
			public const string TotalPasses = "Total passes";
			public const string AccuratePasses = "Passes accurate";
			public const string PassCompPct = "Passes %";
		}		
	}

	public static partial class Serialize
	{
		public static string ToJson(this FixtureTeamStatsFeed self) => JsonConvert.SerializeObject(self, Converter.Settings);
	}
}
