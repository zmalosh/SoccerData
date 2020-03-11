using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SoccerData.Processors.ApiFootball.Feeds
{
	public class FixtureTeamStatsFeed
	{
		public static string GetFeedUrlByFixtureId(int apiFootballFixtureId)
		{
			return $"https://api-football-v1.p.rapidapi.com/v2/statistics/fixture/{apiFootballFixtureId}";
		}

		public static FixtureTeamStatsFeed FromJson(string json) => JsonConvert.DeserializeObject<FixtureTeamStatsFeed>(json, FixtureTeamStatsFeed.Converter.Settings);

		[JsonProperty("api")]
		public ApiResultWrapper ResultWrapper { get; set; }

		public class ApiResultWrapper
		{
			[JsonProperty("results")]
			public int Count { get; set; }

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

		public struct StatisticJsonNode
		{
			public List<object> AnythingArray;
			public Dictionary<string, Statistic> StatsDict;

			public static implicit operator StatisticJsonNode(List<object> AnythingArray) => new StatisticJsonNode { AnythingArray = AnythingArray };
			public static implicit operator StatisticJsonNode(Dictionary<string, Statistic> StatsDict) => new StatisticJsonNode { StatsDict = StatsDict };
		}

		internal static class Converter
		{
			public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
			{
				MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
				DateParseHandling = DateParseHandling.None,
				Converters =
			{
				StatisticsConverter.Singleton,
				new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
			},
			};
		}

		internal class StatisticsConverter : JsonConverter
		{
			public override bool CanConvert(Type t) => t == typeof(StatisticJsonNode) || t == typeof(StatisticJsonNode?);

			public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
			{
				switch (reader.TokenType)
				{
					case JsonToken.StartObject:
						var objectValue = serializer.Deserialize<Dictionary<string, Statistic>>(reader);
						return new StatisticJsonNode { StatsDict = objectValue };
					case JsonToken.StartArray:
						var arrayValue = serializer.Deserialize<List<object>>(reader);
						return new StatisticJsonNode { AnythingArray = arrayValue };
				}
				throw new Exception("Cannot unmarshal type StatisticJsonNode");
			}

			public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
			{
				var value = (StatisticJsonNode)untypedValue;
				if (value.AnythingArray != null)
				{
					serializer.Serialize(writer, value.AnythingArray);
					return;
				}
				if (value.StatsDict != null)
				{
					serializer.Serialize(writer, value.StatsDict);
					return;
				}
				throw new Exception("Cannot marshal type StatisticJsonNode");
			}

			public static readonly StatisticsConverter Singleton = new StatisticsConverter();
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
		public static string ToJson(this FixtureTeamStatsFeed self) => JsonConvert.SerializeObject(self, FixtureTeamStatsFeed.Converter.Settings);
	}
}
