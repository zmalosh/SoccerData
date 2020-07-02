using System;
using System.Collections.Generic;

using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SoccerData.Processors.ApiFootball.Feeds
{
	public class FixtureFeed
	{
		public static string GetFeedUrlByFixtureId(int apiFootballFixtureId)
		{
			return $"https://api-football-v1.p.rapidapi.com/v2/fixtures/id/{apiFootballFixtureId}";
		}

		[JsonProperty("api")]
		public ApiResult Result { get; set; }

		public class ApiResult
		{
			[JsonProperty("results")]
			public int Count { get; set; }

			[JsonProperty("fixtures")]
			public List<ApiFixture> Fixtures { get; set; }
		}

		public class ApiFixture
		{
			[JsonProperty("fixture_id")]
			public int FixtureId { get; set; }

			[JsonProperty("league_id")]
			public int LeagueId { get; set; }

			[JsonProperty("league")]
			public ApiLeague League { get; set; }

			[JsonProperty("event_date")]
			public DateTimeOffset EventDate { get; set; }

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
			public ApiTeam HomeTeam { get; set; }

			[JsonProperty("awayTeam")]
			public ApiTeam AwayTeam { get; set; }

			[JsonProperty("goalsHomeTeam")]
			public int? GoalsHomeTeam { get; set; }

			[JsonProperty("goalsAwayTeam")]
			public int? GoalsAwayTeam { get; set; }

			[JsonProperty("score")]
			public ApiScore Score { get; set; }

			[JsonProperty("events")]
			public List<ApiFixtureEvent> Events { get; set; }

			[JsonProperty("lineups")]
			public Dictionary<string, ApiLineup> Lineups { get; set; }

			[JsonIgnore]
			public List<ApiLineupPlayerWithStarterStatus> AllLineupPlayers
			{
				get
				{
					if (this._allLineupPlayers != null)
					{
						return this._allLineupPlayers;
					}

					// ONLY PROCESS IF 2 LINEUPS ARE SET
					if (this.Lineups == null || this.Lineups.Count(x => x.Value != null) != 2)
					{
						return null;
					}

					this._allLineupPlayers = new List<ApiLineupPlayerWithStarterStatus>();
					foreach (var lineup in this.Lineups)
					{
						if (lineup.Value != null)
						{
							// SOMETIMES TeamId IS NULL ON THE ApiLineupPlayer OBJECT
							int teamId = string.Equals(lineup.Key, this.AwayTeam.TeamName, StringComparison.InvariantCultureIgnoreCase) ? this.AwayTeam.TeamId : this.HomeTeam.TeamId;
							if (lineup.Value?.Starters != null)
							{
								foreach (var apiLineupPlayer in lineup.Value.Starters)
								{
									this._allLineupPlayers.Add(new ApiLineupPlayerWithStarterStatus(apiLineupPlayer, apiLineupPlayer.TeamId ?? teamId, true));
								}
								// ONLY PROCESS SUBS IF THERE ARE STARTERS
								if (lineup.Value.Substitutes != null)
								{
									foreach (var apiLineupPlayer in lineup.Value.Substitutes)
									{
										this._allLineupPlayers.Add(new ApiLineupPlayerWithStarterStatus(apiLineupPlayer, apiLineupPlayer.TeamId ?? teamId, false));
									}
								}
							}
						}
					}
					return this._allLineupPlayers;
				}
			}

			private List<ApiLineupPlayerWithStarterStatus> _allLineupPlayers = null;

			[JsonProperty("statistics")]
			public Dictionary<string, ApiTeamStatistic> TeamStatistics { get; set; }

			[JsonProperty("players")]
			public List<ApiPlayerBoxscore> PlayerBoxscores { get; set; }
		}

		public class ApiTeam
		{
			[JsonProperty("team_id")]
			public int TeamId { get; set; }

			[JsonProperty("team_name")]
			public string TeamName { get; set; }

			[JsonProperty("logo")]
			public string Logo { get; set; }
		}

		public class ApiFixtureEvent
		{
			[JsonProperty("elapsed")]
			public int Elapsed { get; set; }

			[JsonProperty("elapsed_plus")]
			public int? ElapsedPlus { get; set; }

			[JsonProperty("team_id")]
			public int? TeamId { get; set; }

			[JsonProperty("teamName")]
			public string TeamName { get; set; }

			[JsonProperty("player_id")]
			public int? PlayerId { get; set; } // PLAYER OUT ON SUBS

			[JsonProperty("player")]
			public string PlayerName { get; set; } // PLAYER OUT ON SUBS

			[JsonProperty("assist_id")]
			public int? SecondaryPlayerId { get; set; } // ASSIST ON GOAL EVENTS, PLAYER IN ON SUBS

			[JsonProperty("assist")]
			public string SecondaryPlayerName { get; set; } // ASSIST ON GOAL EVENTS, PLAYER IN ON SUBS

			[JsonProperty("type")]
			public string EventType { get; set; }

			[JsonProperty("detail")]
			public string EventDetail { get; set; }

			[JsonProperty("comments")]
			public string EventComments { get; set; }
		}

		public class ApiLeague
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

		public class ApiLineup
		{
			[JsonProperty("coach_id")]
			public int? CoachId { get; set; }

			[JsonProperty("coach")]
			public string Coach { get; set; }

			[JsonProperty("formation")]
			public string Formation { get; set; }

			[JsonProperty("startXI")]
			public List<ApiLineupPlayer> Starters { get; set; }

			[JsonProperty("substitutes")]
			public List<ApiLineupPlayer> Substitutes { get; set; }
		}

		public class ApiLineupPlayer
		{
			[JsonProperty("team_id")]
			public int? TeamId { get; set; }

			[JsonProperty("player_id")]
			public int? PlayerId { get; set; }

			[JsonProperty("player")]
			public string PlayerName { get; set; }

			[JsonProperty("number")]
			public int? Number { get; set; }

			[JsonProperty("pos")]
			public string Position { get; set; }

			public override string ToString()
			{
				return $"API Player ID: { this.PlayerId } ({ (!string.IsNullOrEmpty(this.PlayerName) ? this.PlayerName : "(null)")})";
			}
		}

		public class ApiLineupPlayerWithStarterStatus
		{
			public int TeamId { get; set; }
			public int? PlayerId { get; set; }
			public string PlayerName { get; set; }
			public int? Number { get; set; }
			public string Position { get; set; }
			public bool IsStarter { get; set; }

			public ApiLineupPlayerWithStarterStatus(ApiLineupPlayer player, int teamId, bool isStarter)
			{
				this.TeamId = teamId;
				this.PlayerId = player.PlayerId;
				this.PlayerName = player.PlayerName;
				this.Number = player.Number;
				this.Position = player.Position;
				this.IsStarter = isStarter;
			}

			public override string ToString()
			{
				return $"API Player ID: { this.PlayerId } ({ (!string.IsNullOrEmpty(this.PlayerName) ? this.PlayerName : "(null)")})";
			}
		}

		public class ApiPlayerBoxscore
		{
			[JsonProperty("event_id")]
			public int EventId { get; set; }

			[JsonProperty("updateAt")]
			public int UpdateAt { get; set; }

			[JsonProperty("player_id")]
			public int? PlayerId { get; set; }

			[JsonProperty("player_name")]
			public string PlayerName { get; set; }

			[JsonProperty("team_id")]
			public int TeamId { get; set; }

			[JsonProperty("team_name")]
			public string TeamName { get; set; }

			[JsonProperty("number")]
			public int Number { get; set; }

			[JsonProperty("position")]
			public string Position { get; set; }

			[JsonIgnore]
			public decimal? Rating { get { return decimal.TryParse(this._rating, out decimal dtp) ? dtp : (decimal?)null; } }

			[JsonProperty("rating")]
			private string _rating { get; set; }

			[JsonProperty("minutes_played")]
			public int MinutesPlayed { get; set; }

			[JsonIgnore]
			public bool? IsCaptain { get { return string.IsNullOrEmpty(this._captain) ? (bool?)null : this._captain?.ToUpper() == "TRUE"; } }

			[JsonProperty("captain")]
			private string _captain { get; set; }

			[JsonIgnore]
			public bool? IsSubstitute { get { return string.IsNullOrEmpty(this._substitute) ? (bool?)null : this._substitute?.ToUpper() == "TRUE"; } }

			[JsonProperty("substitute")]
			private string _substitute { get; set; }

			[JsonProperty("offsides")]
			public int? Offsides { get; set; }

			[JsonProperty("shots")]
			public ApiStatShots Shots { get; set; }

			[JsonProperty("goals")]
			public ApiStatGoals Goals { get; set; }

			[JsonProperty("passes")]
			public ApiStatPasses Passes { get; set; }

			[JsonProperty("tackles")]
			public ApiStatTackles Tackles { get; set; }

			[JsonProperty("duels")]
			public ApiStatDuels Duels { get; set; }

			[JsonProperty("dribbles")]
			public ApiStatDribbles Dribbles { get; set; }

			[JsonProperty("fouls")]
			public ApiStatFouls Fouls { get; set; }

			[JsonProperty("cards")]
			public ApiStatCards Cards { get; set; }

			[JsonProperty("penalty")]
			public ApiStatPenalty Penalty { get; set; }

			public override string ToString()
			{
				return $"API Player ID: { this.PlayerId } ({ (!string.IsNullOrEmpty(this.PlayerName) ? this.PlayerName : "(null)")})";
			}
		}

		public class ApiStatCards
		{
			[JsonProperty("yellow")]
			public int Yellow { get; set; }

			[JsonProperty("red")]
			public int Red { get; set; }
		}

		public class ApiStatDribbles
		{
			[JsonProperty("attempts")]
			public int Attempts { get; set; }

			[JsonProperty("success")]
			public int Success { get; set; }

			[JsonProperty("past")]
			public int Past { get; set; }
		}

		public class ApiStatDuels
		{
			[JsonProperty("total")]
			public int Total { get; set; }

			[JsonProperty("won")]
			public int Won { get; set; }
		}

		public class ApiStatFouls
		{
			[JsonProperty("drawn")]
			public int Drawn { get; set; }

			[JsonProperty("committed")]
			public int Committed { get; set; }
		}

		public class ApiStatGoals
		{
			[JsonProperty("total")]
			public int Total { get; set; }

			[JsonProperty("conceded")]
			public int Conceded { get; set; }

			[JsonProperty("assists")]
			public int Assists { get; set; }
		}

		public class ApiStatPasses
		{
			[JsonProperty("total")]
			public int Total { get; set; }

			[JsonProperty("key")]
			public int Key { get; set; }

			[JsonProperty("accuracy")]
			public int Accuracy { get; set; }
		}

		public class ApiStatPenalty
		{
			[JsonProperty("won")]
			public int Won { get; set; }

			[JsonProperty("commited")]
			public int Commited { get; set; }

			[JsonProperty("success")]
			public int Success { get; set; }

			[JsonProperty("missed")]
			public int Missed { get; set; }

			[JsonProperty("saved")]
			public int Saved { get; set; }
		}

		public class ApiStatShots
		{
			[JsonProperty("total")]
			public int Total { get; set; }

			[JsonProperty("on")]
			public int On { get; set; }
		}

		public class ApiStatTackles
		{
			[JsonProperty("total")]
			public int Total { get; set; }

			[JsonProperty("blocks")]
			public int Blocks { get; set; }

			[JsonProperty("interceptions")]
			public int Interceptions { get; set; }
		}

		public class ApiScore
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

		public class ApiTeamStatistic
		{
			[JsonProperty("home")]
			public string Home { get; set; }

			[JsonProperty("away")]
			public string Away { get; set; }
		}

		public static class TeamStatKeys
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

		public static FixtureFeed FromJson(string json) => JsonConvert.DeserializeObject<FixtureFeed>(json, Converter.Settings);
	}

	public static partial class Serialize
	{
		public static string ToJson(this FixtureFeed self) => JsonConvert.SerializeObject(self, Converter.Settings);
	}
}
