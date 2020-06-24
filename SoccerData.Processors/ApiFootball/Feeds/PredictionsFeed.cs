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

			[JsonProperty("fixtures")]
			public List<ApiPrediction> Predictions { get; set; }
		}
		public class ApiPrediction
		{
			[JsonProperty("match_winner")]
			public string MatchWinner { get; set; }

			[JsonProperty("under_over")]
			public string UnderOver { get; set; }

			[JsonProperty("goals_home")]
			public string GoalsHome { get; set; }

			[JsonProperty("goals_away")]
			public string GoalsAway { get; set; }

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
			[JsonProperty("home")]
			public string Home { get; set; }

			[JsonProperty("away")]
			public string Away { get; set; }
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
			public LastH2H LastH2H { get; set; }
		}

		public class ApiPredictionTeamAllLastMatches
		{
			[JsonProperty("matchs")]
			public LastH2H Matches { get; set; }

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
			public double Home { get; set; }

			[JsonProperty("away")]
			public double Away { get; set; }

			[JsonProperty("total")]
			public double Total { get; set; }
		}

		public class LastH2H
		{
			[JsonProperty("matchsPlayed", NullValueHandling = NullValueHandling.Ignore)]
			public ApiPredictionStatTotalBreakdown MatchesPlayed { get; set; }

			[JsonProperty("wins")]
			public ApiPredictionStatTotalBreakdown Wins { get; set; }

			[JsonProperty("draws")]
			public ApiPredictionStatTotalBreakdown Draws { get; set; }

			[JsonProperty("loses")]
			public ApiPredictionStatTotalBreakdown Loses { get; set; }

			[JsonProperty("played", NullValueHandling = NullValueHandling.Ignore)]
			public ApiPredictionStatTotalBreakdown Played { get; set; }
		}

		public class ApiPredictionTeamLast5Matches
		{
			[JsonIgnore]
			public double? Forme
			{
				get { return double.TryParse(this._forme.Replace("%", string.Empty), out double dtp) ? dtp : (double?)null; }
			}

			[JsonProperty("forme")]
			private string _forme { get; set; }

			[JsonIgnore]
			public double? Attack
			{
				get { return double.TryParse(this._attack.Replace("%", string.Empty), out double dtp) ? dtp : (double?)null; }
			}

			[JsonProperty("att")]
			public string _attack { get; set; }

			[JsonIgnore]
			public double? Defense
			{
				get { return double.TryParse(this._defense.Replace("%", string.Empty), out double dtp) ? dtp : (double?)null; }
			}

			[JsonProperty("def")]
			public string _defense { get; set; }

			[JsonProperty("goals")]
			public int Goals { get; set; }

			[JsonProperty("goals_avg")]
			public double GoalsAvg { get; set; }

			[JsonProperty("goals_against")]
			public int GoalsAgainst { get; set; }

			[JsonProperty("goals_against_avg")]
			public double GoalsAgainstAvg { get; set; }
		}

		public class ApiPredictionWinningPercent
		{
			[JsonIgnore]
			public double? Away
			{
				get { return double.TryParse(this._away.Replace("%", string.Empty), out double dtp) ? dtp : (double?)null; }
			}

			[JsonIgnore]
			public double? Draws
			{
				get { return double.TryParse(this._draws.Replace("%", string.Empty), out double dtp) ? dtp : (double?)null; }
			}

			[JsonIgnore]
			public double? Home
			{
				get { return double.TryParse(this._home.Replace("%", string.Empty), out double dtp) ? dtp : (double?)null; }
			}

			[JsonProperty("home")]
			private string _home { get; set; }

			[JsonProperty("draws")]
			private string _draws { get; set; }

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
