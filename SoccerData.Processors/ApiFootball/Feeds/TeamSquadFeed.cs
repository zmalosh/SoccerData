using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SoccerData.Processors.ApiFootball.Feeds
{
	public class TeamSquadFeed
	{
		public static string GetUrlFromTeamIdAndSeasonYears(int apiFootballTeamId, DateTime startDate, DateTime endDate)
		{
			if (startDate.Year == endDate.Year)
			{
				return $"https://api-football-v1.p.rapidapi.com/v2/players/squad/{apiFootballTeamId}/{startDate.Year}";
			}
			return $"https://api-football-v1.p.rapidapi.com/v2/players/squad/{apiFootballTeamId}/{startDate.Year}-{endDate.Year}";
		}

		[JsonProperty("api")]
		public ApiResult Result { get; set; }

		public class ApiResult
		{
			[JsonProperty("results")]
			public int Count { get; set; }

			[JsonProperty("players")]
			public List<ApiPlayer> Players { get; set; }
		}

		public class ApiPlayer
		{
			[JsonProperty("player_id")]
			public int PlayerId { get; set; }

			[JsonProperty("player_name")]
			public string PlayerName { get; set; }

			[JsonProperty("firstname")]
			public string Firstname { get; set; }

			[JsonProperty("lastname")]
			public string Lastname { get; set; }

			[JsonProperty("number")]
			public int? Number { get; set; }

			[JsonProperty("position")]
			public string Position { get; set; }

			[JsonProperty("age")]
			public int? Age { get; set; }

			[JsonIgnore]
			public DateTime? BirthDate
			{
				get
				{
					if (string.IsNullOrEmpty(this._birthdate_ddmmyyyy) || string.IsNullOrWhiteSpace(this._birthdate_ddmmyyyy)) { return null; }
					if (!DateTime.TryParseExact(this._birthdate_ddmmyyyy, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
					{
						return null;
					}
					return result.Date;
				}
			}

			[JsonProperty("birth_date")]
			private string _birthdate_ddmmyyyy { get; set; }

			[JsonProperty("birth_place")]
			public string BirthPlace { get; set; }

			[JsonProperty("birth_country")]
			public string BirthCountry { get; set; }

			[JsonProperty("nationality")]
			public string Nationality { get; set; }

			[JsonIgnore]
			public int? HeightInCm
			{
				get
				{
					if (string.IsNullOrEmpty(this._height) || string.IsNullOrWhiteSpace(this._height))
					{
						return null;
					}
					return int.Parse(this._height.Replace("cm", string.Empty).Trim());
				}
			}

			[JsonProperty("height")]
			private string _height { get; set; }

			[JsonIgnore]
			public int? WeightInKg
			{
				get
				{
					if (string.IsNullOrEmpty(this._weight) || string.IsNullOrWhiteSpace(this._weight))
					{
						return null;
					}
					return int.Parse(this._weight.Replace("kg", string.Empty).Trim());
				}
			}

			[JsonProperty("weight")]
			private string _weight { get; set; }
		}

		public static TeamSquadFeed FromJson(string json) => json == null ? null : JsonConvert.DeserializeObject<TeamSquadFeed>(json, Converter.Settings);
	}

	public static partial class Serialize
	{
		public static string ToJson(this TeamSquadFeed self) => JsonConvert.SerializeObject(self, Converter.Settings);
	}
}
