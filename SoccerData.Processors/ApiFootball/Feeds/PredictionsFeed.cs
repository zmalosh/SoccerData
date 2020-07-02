using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Processors.ApiFootball.Feeds
{
	public class PredictionsFeed
	{
		public static string GetFeedUrlByFixtureId(int apiFootballFixtureId)
		{
			return $"https://api-football-v1.p.rapidapi.com/v2/predictions/{apiFootballFixtureId}";
		}

		[JsonProperty("api")]
		public ApiResult Result { get; set; }

		public class ApiResult
		{
			[JsonProperty("results")]
			public int Count { get; set; }

			[JsonProperty("predictions")]
			public List<ApiPrediction> Predictions { get; set; }
		}
		public class ApiPrediction
		{
			[JsonProperty("match_winner")]
			public string MatchWinner { get; set; }

			[JsonIgnore]
			public decimal? GameTotal { get { return decimal.TryParse(this._gameTotal, out decimal dtp) ? dtp : (decimal?)null; } }

			[JsonProperty("under_over")]
			private string _gameTotal { get; set; }

			[JsonIgnore]
			public decimal? GoalsHome { get { return decimal.TryParse(this._goalsHome, out decimal dtp) ? dtp : (decimal?)null; } }

			[JsonIgnore]
			public decimal? GoalsAway { get { return decimal.TryParse(this._goalsAway, out decimal dtp) ? dtp : (decimal?)null; } }

			[JsonProperty("goals_home")]
			public string _goalsHome { get; set; }

			[JsonProperty("goals_away")]
			public string _goalsAway { get; set; }

			[JsonProperty("advice")]
			public string Advice { get; set; }

			[JsonProperty("winning_percent")]
			public ApiPredictionWinningPercent WinningPercent { get; set; }

			[JsonProperty("teams")]
			public ApiPredictionFixture Teams { get; set; }

			[JsonProperty("h2h")]
			public List<ApiPredictionH2H> H2H { get; set; }

			[JsonProperty("comparison")]
			public ApiPredictionComparison Comparison { get; set; }
		}

		public class ApiPredictionComparison
		{
			[JsonProperty("forme")]
			public ApiPredictionTeamValues Forme { get; set; }

			[JsonProperty("att")]
			public ApiPredictionTeamValues Att { get; set; }

			[JsonProperty("def")]
			public ApiPredictionTeamValues Def { get; set; }

			[JsonProperty("fish_law")]
			public ApiPredictionTeamValues FishLaw { get; set; }

			[JsonProperty("h2h")]
			public ApiPredictionTeamValues H2H { get; set; }

			[JsonProperty("goals_h2h")]
			public ApiPredictionTeamValues GoalsH2H { get; set; }
		}

		public class ApiPredictionTeamValues
		{
			[JsonIgnore]
			public int? HomeWinPct { get { return int.TryParse(this._home.Replace("%", string.Empty), out int itp) ? itp : (int?)null; } }

			[JsonIgnore]
			public int? AwayWinPct { get { return int.TryParse(this._away.Replace("%", string.Empty), out int itp) ? itp : (int?)null; } }

			[JsonProperty("home")]
			private string _home { get; set; }

			[JsonProperty("away")]
			private string _away { get; set; }
		}

		public class ApiPredictionH2H
		{
			[JsonProperty("fixture_id")]
			public int FixtureId { get; set; }

			[JsonProperty("league_id")]
			public int LeagueId { get; set; }

			[JsonProperty("league")]
			public ApiPredictionLeague League { get; set; }

			[JsonProperty("event_date")]
			public DateTime EventDate { get; set; }

			[JsonProperty("event_timestamp")]
			public int? EventTimestamp { get; set; }

			[JsonProperty("firstHalfStart")]
			public int? FirstHalfStart { get; set; }

			[JsonProperty("secondHalfStart")]
			public int? SecondHalfStart { get; set; }

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

			[JsonProperty("referee")]
			public string Referee { get; set; }

			[JsonProperty("homeTeam")]
			public ApiPredictionSimpleTeam HomeTeam { get; set; }

			[JsonProperty("awayTeam")]
			public ApiPredictionSimpleTeam AwayTeam { get; set; }

			[JsonProperty("goalsHomeTeam")]
			public int? GoalsHomeTeam { get; set; }

			[JsonProperty("goalsAwayTeam")]
			public int? GoalsAwayTeam { get; set; }

			[JsonProperty("score")]
			public ApiResultScore Score { get; set; }
		}

		public class ApiPredictionSimpleTeam
		{
			[JsonProperty("team_id")]
			public int TeamId { get; set; }

			[JsonProperty("team_name")]
			public string TeamName { get; set; }

			[JsonProperty("logo")]
			public string Logo { get; set; }
		}

		public class ApiPredictionLeague
		{
			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("country")]
			public string Country { get; set; }

			[JsonProperty("logo")]
			public string Logo { get; set; }

			[JsonProperty("flag")]
			public string Flag { get; set; }
		}

		public class ApiResultScore
		{
			[JsonProperty("halftime")]
			public string Halftime { get; set; }

			[JsonProperty("fulltime")]
			public string Fulltime { get; set; }

			[JsonProperty("extratime")]
			public string Extratime { get; set; }

			[JsonProperty("penalty")]
			public string Penalty { get; set; }
		}

		public class ApiPredictionFixture
		{
			[JsonProperty("home")]
			public ApiPredictionTeam Home { get; set; }

			[JsonProperty("away")]
			public ApiPredictionTeam Away { get; set; }
		}

		public class ApiPredictionTeam
		{
			[JsonProperty("team_id")]
			public int TeamId { get; set; }

			[JsonProperty("team_name")]
			public string TeamName { get; set; }

			[JsonProperty("last_5_matches")]
			public ApiPredictionTeamLast5Matches Last5_Matches { get; set; }

			[JsonProperty("all_last_matches")]
			public ApiPredictionTeamAllLastMatches AllLastMatches { get; set; }

			[JsonProperty("last_h2h")]
			public ApiPredictionLastMatchesH2H LastH2H { get; set; }
		}

		public class ApiPredictionTeamAllLastMatches
		{
			[JsonProperty("matchs")]
			public ApiPredictionLastMatchesH2H Matches { get; set; }

			[JsonProperty("goals")]
			public ApiPredictionTeamGoalsTotal Goals { get; set; }

			[JsonProperty("goalsAvg")]
			public ApiPredictionTeamGoalsAvg GoalsAvg { get; set; }
		}

		public class ApiPredictionTeamGoalsTotal
		{
			[JsonProperty("goalsFor")]
			public ApiPredictionStatTotalBreakdown GoalsFor { get; set; }

			[JsonProperty("goalsAgainst")]
			public ApiPredictionStatTotalBreakdown GoalsAgainst { get; set; }
		}

		public class ApiPredictionStatTotalBreakdown
		{
			[JsonProperty("home")]
			public int Home { get; set; }

			[JsonProperty("away")]
			public int Away { get; set; }

			[JsonProperty("total")]
			public int Total { get; set; }
		}

		public class ApiPredictionTeamGoalsAvg
		{
			[JsonProperty("goalsFor")]
			public ApiPredictionStatAvgBreakdown GoalsFor { get; set; }

			[JsonProperty("goalsAgainst")]
			public ApiPredictionStatAvgBreakdown GoalsAgainst { get; set; }
		}

		public class ApiPredictionStatAvgBreakdown
		{
			[JsonProperty("home")]
			public decimal Home { get; set; }

			[JsonProperty("away")]
			public decimal Away { get; set; }

			[JsonProperty("total")]
			public decimal Total { get; set; }
		}

		public class ApiPredictionLastMatchesH2H
		{
			[JsonProperty("matchsPlayed", NullValueHandling = NullValueHandling.Ignore)]
			public ApiPredictionStatTotalBreakdown MatchesPlayed { get; set; }

			[JsonProperty("wins", NullValueHandling = NullValueHandling.Ignore)]
			public ApiPredictionStatTotalBreakdown Wins { get; set; }

			[JsonProperty("draws", NullValueHandling = NullValueHandling.Ignore)]
			public ApiPredictionStatTotalBreakdown Draws { get; set; }

			[JsonProperty("loses", NullValueHandling = NullValueHandling.Ignore)]
			public ApiPredictionStatTotalBreakdown Losses { get; set; }

			[JsonProperty("played", NullValueHandling = NullValueHandling.Ignore)]
			public ApiPredictionStatTotalBreakdown Played { get; set; }
		}

		public class ApiPredictionTeamLast5Matches
		{
			[JsonIgnore]
			public int? Forme
			{
				get { return int.TryParse(this._forme.Replace("%", string.Empty), out int itp) ? itp : (int?)null; }
			}

			[JsonProperty("forme")]
			private string _forme { get; set; }

			[JsonIgnore]
			public int? Attack
			{
				get { return int.TryParse(this._attack.Replace("%", string.Empty), out int itp) ? itp : (int?)null; }
			}

			[JsonProperty("att")]
			public string _attack { get; set; }

			[JsonIgnore]
			public int? Defense
			{
				get { return int.TryParse(this._defense.Replace("%", string.Empty), out int itp) ? itp : (int?)null; }
			}

			[JsonProperty("def")]
			public string _defense { get; set; }

			[JsonProperty("goals")]
			public int? Goals { get; set; }

			[JsonProperty("goals_avg")]
			public decimal? GoalsAvg { get; set; }

			[JsonProperty("goals_against")]
			public int? GoalsAgainst { get; set; }

			[JsonProperty("goals_against_avg")]
			public decimal? GoalsAgainstAvg { get; set; }
		}

		public class ApiPredictionWinningPercent
		{
			[JsonIgnore]
			public int? Away
			{
				get { return int.TryParse(this._away.Replace("%", string.Empty), out int itp) ? itp : (int?)null; }
			}

			[JsonIgnore]
			public int? Draw
			{
				get { return int.TryParse(this._draw.Replace("%", string.Empty), out int itp) ? itp : (int?)null; }
			}

			[JsonIgnore]
			public int? Home
			{
				get { return int.TryParse(this._home.Replace("%", string.Empty), out int itp) ? itp : (int?)null; }
			}

			[JsonProperty("home")]
			private string _home { get; set; }

			[JsonProperty("draws")]
			private string _draw { get; set; }

			[JsonProperty("away")]
			private string _away { get; set; }
		}

		public static PredictionsFeed FromJson(string json) => JsonConvert.DeserializeObject<PredictionsFeed>(json, Converter.Settings);
	}

	public static partial class Serialize
	{
		public static string ToJson(this PredictionsFeed self) => JsonConvert.SerializeObject(self, Converter.Settings);
	}
}
