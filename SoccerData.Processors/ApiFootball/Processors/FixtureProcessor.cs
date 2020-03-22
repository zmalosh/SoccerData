using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SoccerData.Model;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace SoccerData.Processors.ApiFootball.Processors
{
	public class FixtureProcessor : IProcessor
	{
		private readonly int ApiFootballFixtureId;
		private readonly JsonUtility JsonUtility;
		private readonly bool CheckEntitiesExist;

		private const int NullIntDictKey = int.MinValue;

		public FixtureProcessor(int apiFootballFixtureId, bool checkEntitiesExist = true)
		{
			this.ApiFootballFixtureId = apiFootballFixtureId;
			this.CheckEntitiesExist = checkEntitiesExist;
			this.JsonUtility = new JsonUtility(120 * 24 * 60 * 60, sourceType: JsonUtility.JsonSourceType.ApiFootball); // 230K+ FIXTURES.... SAVE FINISHED GAMES FOR A LONG TIME (120 DAYS?) TO AVOID QUOTA ISSUES
		}

		public void Run(SoccerDataContext dbContext)
		{
			var dbFixture = dbContext.Fixtures.Single(x => x.ApiFootballId == this.ApiFootballFixtureId);
			var isFixtureFinal = string.Equals("Match Finished", dbFixture.Status, StringComparison.CurrentCultureIgnoreCase);

			if (!dbFixture.HomeTeamSeasonId.HasValue
				|| !dbFixture.AwayTeamSeasonId.HasValue
				|| !isFixtureFinal)
			{
				return;
			}

			var url = Feeds.FixtureFeed.GetFeedUrlByFixtureId(this.ApiFootballFixtureId);
			var rawJson = JsonUtility.GetRawJsonFromUrl(url);
			var feed = Feeds.FixtureFeed.FromJson(rawJson);

			Feeds.FixtureFeed.ApiFixture feedFixture = feed.Result.Fixtures.Single();

			int dbFixtureId = dbFixture.FixtureId;
			int dbHomeTeamSeasonId = dbFixture.HomeTeamSeasonId.Value;
			int dbAwayTeamSeasonId = dbFixture.AwayTeamSeasonId.Value;
			int apiAwayTeamId = feedFixture.AwayTeam.TeamId;
			int apiHomeTeamId = feedFixture.HomeTeam.TeamId;

			int? homeCoachId = null;
			int? awayCoachId = null;

			var apiPlayerBases = GetApiPlayerBases(feedFixture);
			var dbPlayerSeasonDict = GetDbPlayerSeasonDict(dbContext, apiPlayerBases, dbFixture.CompetitionSeasonId);

			bool hasUpdate = false;

			Feeds.FixtureFeed.ApiLineup homeLineup = null;
			Feeds.FixtureFeed.ApiLineup awayLineup = null;

			#region GET FORMATIONS
			string homeFormation = null;
			string awayFormation = null;
			if (feedFixture.Lineups != null && feedFixture.Lineups.Count == 2)
			{
				string homeTeamName = feedFixture.HomeTeam.TeamName;
				string awayTeamName = feedFixture.AwayTeam.TeamName;

				// MISMATCH BETWEEN PLAYING TEAM NAMES AND LINEUP DICT KEYS HAS OCCURRED (API fixtureID: 188155)
				bool hasHomeTeamName = feedFixture.Lineups.ContainsKey(homeTeamName);
				bool hasAwayTeamName = feedFixture.Lineups.ContainsKey(awayTeamName);
				if (!hasHomeTeamName || !hasAwayTeamName)
				{
					if (hasHomeTeamName && !hasAwayTeamName)
					{
						awayTeamName = feedFixture.Lineups.Keys.Single(x => x != homeTeamName);
					}
					else if (!hasHomeTeamName && hasAwayTeamName)
					{
						homeTeamName = feedFixture.Lineups.Keys.Single(x => x != awayTeamName);
					}
					else
					{
						throw new KeyNotFoundException("INVALID KEYS FOUND FOR FIXTURE LINEUPS");
					}
				}

				homeLineup = feedFixture.Lineups[homeTeamName];
				awayLineup = feedFixture.Lineups[awayTeamName];
				homeFormation = homeLineup.Formation;
				awayFormation = awayLineup.Formation;
			}
			#endregion GET FORMATIONS


			#region ENSURE COACHES EXIST
			if (this.CheckEntitiesExist)
			{
				if (homeLineup != null || awayLineup != null)
				{
					var apiCoachIds = new[] { homeLineup.CoachId, awayLineup.CoachId };
					var dbCoaches = dbContext.Coaches.Where(x => apiCoachIds.Contains(x.ApiFootballId)).ToDictionary(x => x.ApiFootballId, y => y);

					if (homeLineup?.CoachId != null)
					{
						if (!dbCoaches.TryGetValue(homeLineup.CoachId.Value, out Coach dbHomeCoach))
						{
							dbHomeCoach = new Coach
							{
								ApiFootballId = homeLineup.CoachId.Value,
								CoachName = homeLineup.Coach
							};
							dbContext.Coaches.Add(dbHomeCoach);
							dbContext.SaveChanges();
						}
						homeCoachId = dbHomeCoach.CoachId;
					}
					if (awayLineup?.CoachId != null)
					{
						if (!dbCoaches.TryGetValue(awayLineup.CoachId.Value, out Coach dbAwayCoach))
						{
							dbAwayCoach = new Coach
							{
								ApiFootballId = awayLineup.CoachId.Value,
								CoachName = awayLineup.Coach
							};
							dbContext.Coaches.Add(dbAwayCoach);
							dbContext.SaveChanges();
						}
						awayCoachId = dbAwayCoach.CoachId;
					}
				}
			}
			#endregion ENSURE COACHES EXIST 

			#region ENSURE PLAYERS EXIST
			if (this.CheckEntitiesExist)
			{
				var missingApiPlayerIds = apiPlayerBases?.Select(x => x.PlayerId).Where(x => !dbPlayerSeasonDict.ContainsKey(x)).ToList();
				if (missingApiPlayerIds != null && missingApiPlayerIds.Count > 0)
				{
					foreach (var missingApiPlayerId in missingApiPlayerIds)
					{
						var apiPlayerBase = apiPlayerBases.Single(x => x.PlayerId == missingApiPlayerId);
						var dbPlayerSeason = new PlayerSeason
						{
							Player = new Player
							{
								ApiFootballId = missingApiPlayerId,
								PlayerName = apiPlayerBase.PlayerName
							},
							CompetitionSeasonId = dbFixture.CompetitionSeasonId
						};
						dbContext.Add(dbPlayerSeason);
					}
					dbContext.SaveChanges();
					dbPlayerSeasonDict = GetDbPlayerSeasonDict(dbContext, apiPlayerBases, dbFixture.CompetitionSeasonId);
				}
			}
			#endregion ENSURE PLAYERS EXIST

			#region UPDATE FORAMATION AND COACH IF NECESSARY
			if (homeCoachId.HasValue && dbFixture.HomeCoachId != homeCoachId)
			{
				dbFixture.HomeCoachId = homeCoachId;
				hasUpdate = true;
			}
			if (awayCoachId.HasValue && dbFixture.AwayCoachId != awayCoachId)
			{
				dbFixture.AwayCoachId = awayCoachId;
				hasUpdate = true;
			}
			if (!string.IsNullOrEmpty(homeFormation) && dbFixture.HomeFormation != homeFormation)
			{
				dbFixture.HomeFormation = homeFormation;
				hasUpdate = true;
			}
			if (!string.IsNullOrEmpty(awayFormation) && dbFixture.AwayFormation != awayFormation)
			{
				dbFixture.AwayFormation = awayFormation;
				hasUpdate = true;
			}
			#endregion UPDATE FORAMATION AND COACH IF NECESSARY

			#region FIXTURE EVENTS
			var dbFixtureEvents = dbContext.FixtureEvents.Where(x => x.FixtureId == dbFixtureId).ToDictionary(x => GetFixtureEventKey(x));
			var apiFixtureEvents = feedFixture.Events?.Where(x => x.TeamId.HasValue).ToList();
			if (apiFixtureEvents != null && apiFixtureEvents.Count > 0)
			{
				foreach (var apiFixtureEvent in apiFixtureEvents)
				{
					int dbTeamSeasonId = apiFixtureEvent.TeamId == apiAwayTeamId ? dbAwayTeamSeasonId : dbHomeTeamSeasonId;
					int? dbPlayerSeasonId = apiFixtureEvent.PlayerId.HasValue ? dbPlayerSeasonDict[apiFixtureEvent.PlayerId.Value] : (int?)null;
					int? dbSecondaryPlayerSeasonId = apiFixtureEvent.SecondaryPlayerId.HasValue ? dbPlayerSeasonDict[apiFixtureEvent.SecondaryPlayerId.Value] : (int?)null;

					var eventKey = GetFixtureEventKey(apiFixtureEvent.Elapsed, apiFixtureEvent.ElapsedPlus, dbPlayerSeasonId, dbTeamSeasonId, apiFixtureEvent.EventType, apiFixtureEvent.EventDetail);
					if (!dbFixtureEvents.TryGetValue(eventKey, out FixtureEvent dbFixtureEvent))
					{
						dbFixtureEvent = new FixtureEvent
						{
							EventComment = apiFixtureEvent.EventComments,
							EventDetail = apiFixtureEvent.EventDetail,
							EventType = apiFixtureEvent.EventType,
							FixtureId = dbFixtureId,
							EventTime = apiFixtureEvent.Elapsed,
							EventTimePlus = apiFixtureEvent.ElapsedPlus,
							PlayerSeasonId = dbPlayerSeasonId,
							SecondaryPlayerSeasonId = dbSecondaryPlayerSeasonId,
							TeamSeasonId = dbTeamSeasonId
						};
						dbContext.FixtureEvents.Add(dbFixtureEvent);
						hasUpdate = true;
					}
					else
					{
						dbFixtureEvents.Remove(eventKey);
						if ((!string.IsNullOrEmpty(apiFixtureEvent.EventComments) && dbFixtureEvent.EventComment != apiFixtureEvent.EventComments)
							|| (!string.IsNullOrEmpty(apiFixtureEvent.EventDetail) && dbFixtureEvent.EventDetail != apiFixtureEvent.EventDetail)
							|| (dbSecondaryPlayerSeasonId.HasValue && (!dbFixtureEvent.SecondaryPlayerSeasonId.HasValue || dbFixtureEvent.SecondaryPlayerSeasonId != dbSecondaryPlayerSeasonId))
							|| (!dbSecondaryPlayerSeasonId.HasValue && dbFixtureEvent.SecondaryPlayerSeasonId.HasValue))
						{
							dbFixtureEvent.EventComment = apiFixtureEvent.EventComments;
							dbFixtureEvent.EventDetail = apiFixtureEvent.EventDetail;
							dbFixtureEvent.SecondaryPlayerSeasonId = dbSecondaryPlayerSeasonId;
						}
						hasUpdate = true;
					}
				}
				if (dbFixtureEvents.Count > 0)
				{
					foreach (var dbFixtureEventKVP in dbFixtureEvents)
					{
						var dbFixtureEvent = dbFixtureEventKVP.Value;
						dbContext.FixtureEvents.Remove(dbFixtureEvent);
					}
					hasUpdate = true;
				}
			}
			#endregion FIXTURE EVENTS

			#region TEAM BOXSCORE

			var apiTeamStatsDict = feedFixture.TeamStatistics;
			if (apiTeamStatsDict == null)
			{
				if (!dbFixture.HasTeamBoxscores.HasValue || dbFixture.HasTeamBoxscores.Value)
				{
					hasUpdate = true;
				}
				dbFixture.HasTeamBoxscores = false;
			}
			else
			{
				var dbTeamBoxscores = dbContext.TeamBoxscores.Where(x => x.FixtureId == dbFixtureId);

				var dbHomeBoxscore = dbTeamBoxscores?.SingleOrDefault(x => x.TeamSeasonId == dbHomeTeamSeasonId);
				var dbAwayBoxscore = dbTeamBoxscores?.SingleOrDefault(x => x.TeamSeasonId == dbAwayTeamSeasonId);

				if (dbHomeBoxscore == null)
				{
					dbHomeBoxscore = new TeamBoxscore
					{
						FixtureId = dbFixtureId,
						TeamSeasonId = dbHomeTeamSeasonId,
						OppTeamSeasonId = dbAwayTeamSeasonId,
						IsHome = true
					};
					dbContext.TeamBoxscores.Add(dbHomeBoxscore);
					hasUpdate = true;
				}
				if (dbAwayBoxscore == null)
				{
					dbAwayBoxscore = new TeamBoxscore
					{
						FixtureId = dbFixtureId,
						TeamSeasonId = dbAwayTeamSeasonId,
						OppTeamSeasonId = dbHomeTeamSeasonId,
						IsHome = false,
					};
					dbContext.TeamBoxscores.Add(dbAwayBoxscore);
					hasUpdate = true;
				}

				if (PopulateTeamBoxscore(apiTeamStatsDict, x => x.Home, ref dbHomeBoxscore))
				{
					hasUpdate = true;
					dbFixture.HasTeamBoxscores = true;
				}
				if (PopulateTeamBoxscore(apiTeamStatsDict, x => x.Away, ref dbAwayBoxscore))
				{
					hasUpdate = true;
					dbFixture.HasTeamBoxscores = true;
				}

				if (!dbFixture.HasTeamBoxscores.HasValue)
				{
					dbFixture.HasTeamBoxscores = false;
				}
			}
			#endregion TEAM BOXSCORE

			if (hasUpdate)
			{
				dbContext.SaveChanges();
			}
		}

		private Dictionary<int, int> GetDbPlayerSeasonDict(SoccerDataContext dbContext, List<ApiPlayerBase> apiPlayerBases, int dbCompetitionSeasonId)
		{
			if (apiPlayerBases == null || apiPlayerBases.Count == 0)
			{
				return null;
			}

			var apiPlayerIds = apiPlayerBases.Select(x => x.PlayerId).ToList();
			return dbContext.PlayerSeasons
							.Include(x => x.Player)
							.Where(x => x.CompetitionSeasonId == dbCompetitionSeasonId && apiPlayerIds.Contains(x.Player.ApiFootballId))
							.ToDictionary(x => x.Player.ApiFootballId, y => y.PlayerSeasonId);
		}

		private List<ApiPlayerBase> GetApiPlayerBases(Feeds.FixtureFeed.ApiFixture feedFixture)
		{
			var apiPlayerBasesFromPlayers = feedFixture.Players?.Where(x => x.PlayerId.HasValue).Select(x => new ApiPlayerBase(x.PlayerId.Value, x.PlayerName)).ToList();
			var apiPlayerBasesFromEvents = feedFixture.Events?
														.SelectMany(x => new[] { new { PlayerId = x.PlayerId, PlayerName = x.PlayerName }, new { PlayerId = x.SecondaryPlayerId, PlayerName = x.SecondaryPlayerName } })
														.Where(x => x.PlayerId.HasValue && !string.IsNullOrEmpty(x.PlayerName))
														.Select(x => new ApiPlayerBase(x.PlayerId.Value, x.PlayerName))
														.ToList();

			var apiPlayerBasesFromLineups = feedFixture.Lineups?
															.Where(x => x.Value.Starters != null && x.Value.Starters.Count > 0)
															.SelectMany(x =>
																x.Value.Starters
																		.Where(y => y.PlayerId.HasValue && !string.IsNullOrEmpty(y.PlayerName))
																		.Select(y => new ApiPlayerBase(y.PlayerId.Value, y.PlayerName))
															)
															.ToList();
			if (apiPlayerBasesFromLineups != null)
			{
				var apiPlayerBaseSubstitutes = feedFixture.Lineups?
															.Where(x => x.Value.Substitutes != null && x.Value.Substitutes.Count > 0)
															.SelectMany(x =>
																x.Value.Substitutes
																		.Where(y => y.PlayerId.HasValue && !string.IsNullOrEmpty(y.PlayerName))
																		.Select(y => new ApiPlayerBase(y.PlayerId.Value, y.PlayerName))
															)
															.ToList();
			}

			var apiPlayerBases = apiPlayerBasesFromPlayers ?? apiPlayerBasesFromLineups ?? apiPlayerBasesFromEvents;
			if (apiPlayerBasesFromPlayers != null)
			{
				if (apiPlayerBasesFromLineups != null)
				{
					apiPlayerBases.AddRange(apiPlayerBasesFromLineups);
				}
				if (apiPlayerBasesFromEvents != null)
				{
					apiPlayerBases.AddRange(apiPlayerBasesFromEvents);
				}
			}
			else if (apiPlayerBasesFromLineups != null)
			{
				if (apiPlayerBasesFromEvents != null)
				{
					apiPlayerBases.AddRange(apiPlayerBasesFromEvents);
				}
			}
			if (apiPlayerBases != null)
			{
				apiPlayerBases = apiPlayerBases.Distinct(new ApiPlayerBaseComparer()).ToList();
			}
			return apiPlayerBases;
		}

		// TEAM IS REQUIRED (API FIXTURE ID 131874)
		private (int, int, int, int, string, string) GetFixtureEventKey(FixtureEvent fixtureEvent)
		{
			return (fixtureEvent.EventTime,
					fixtureEvent.EventTimePlus ?? NullIntDictKey,
					fixtureEvent.PlayerSeasonId ?? NullIntDictKey,
					fixtureEvent.TeamSeasonId,
					fixtureEvent.EventType,
					fixtureEvent.EventDetail);
		}

		private (int, int, int, int, string, string) GetFixtureEventKey(int gameTime, int? gameTimePlus, int? playerSeasonId, int teamSeasonId, string eventType, string eventDetail)
		{
			return (gameTime,
					gameTimePlus ?? NullIntDictKey,
					playerSeasonId ?? NullIntDictKey,
					teamSeasonId,
					eventType,
					eventDetail);
		}

		#region TEAM BOXSCORE HELPERS

		/// <summary>
		/// 
		/// </summary>
		/// <param name="apiStatsDict">Team statistics for fixture from API</param>
		/// <param name="teamId">Team which accumulated desired stats</param>
		/// <param name="oppTeamId">Opponent of team which accumulated desired stats</param>
		/// <param name="isHome">Indicator for if the desired team is the home team</param>
		/// <param name="statGetFunc">Function to return the desired stat from a Statistic object. Used to choose home or away value.</param>
		/// <param name="dbTeamBoxscore">Object to populate</param>
		/// <returns>true if an update has been made; else false</returns>
		private bool PopulateTeamBoxscore(Dictionary<string, Feeds.FixtureFeed.ApiTeamStatistic> apiStatsDict,
			Func<Feeds.FixtureFeed.ApiTeamStatistic, string> statGetFunc,
			ref TeamBoxscore dbTeamBoxscore)
		{
			bool hasUpdate = false;

			int? statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.ShotsOnGoal, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.ShotsOnGoal)
			{
				dbTeamBoxscore.ShotsOnGoal = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.ShotsOffGoal, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.ShotsOffGoal)
			{
				dbTeamBoxscore.ShotsOffGoal = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.TotalShots, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.ShotsTotal)
			{
				dbTeamBoxscore.ShotsTotal = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.BlockedShots, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.ShotsBlocked)
			{
				dbTeamBoxscore.ShotsBlocked = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.ShotsInsideBox, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.ShotsInsideBox)
			{
				dbTeamBoxscore.ShotsInsideBox = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.ShotsOutsideBox, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.ShotsOutsideBox)
			{
				dbTeamBoxscore.ShotsOutsideBox = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.FoulsCommitted, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.FoulsCommitted)
			{
				dbTeamBoxscore.FoulsCommitted = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.CornerKicks, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.CornerKicks)
			{
				dbTeamBoxscore.CornerKicks = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.Offsides, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.OffsidesCommitted)
			{
				dbTeamBoxscore.OffsidesCommitted = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.BallPossession, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.PossessionPct)
			{
				dbTeamBoxscore.PossessionPct = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.YellowCards, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.YellowCards)
			{
				dbTeamBoxscore.YellowCards = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.RedCards, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.RedCards)
			{
				dbTeamBoxscore.RedCards = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.GoalkeeperSaves, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.GoalieSaves)
			{
				dbTeamBoxscore.GoalieSaves = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.TotalPasses, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.PassesTotal)
			{
				dbTeamBoxscore.PassesTotal = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.AccuratePasses, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.PassesAccurate)
			{
				dbTeamBoxscore.PassesAccurate = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.PassCompPct, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.PassAccuracyPct)
			{
				dbTeamBoxscore.PassAccuracyPct = statVal.Value;
				hasUpdate = true;
			}

			return hasUpdate;
		}

		private int? GetStatValueByKey(string key,
			Dictionary<string, Feeds.FixtureFeed.ApiTeamStatistic> apiStatsDict,
			Func<Feeds.FixtureFeed.ApiTeamStatistic, string> statGetFunc)
		{
			if (!apiStatsDict.TryGetValue(key, out Feeds.FixtureFeed.ApiTeamStatistic apiTeamStat))
			{
				return null;
			}

			var strValue = statGetFunc(apiTeamStat);
			if (string.IsNullOrEmpty(strValue))
			{
				return null;
			}

			strValue = strValue.Replace("%", string.Empty);

			if (int.TryParse(strValue, out int result))
			{
				return result;
			}
			return null;
		}
		#endregion TEAM BOXSCORE HELPERS

		#region HELPER CLASSES
		private class ApiPlayerBase
		{
			public int PlayerId { get; set; }
			public string PlayerName { get; set; }

			public ApiPlayerBase(int playerId, string playerName)
			{
				this.PlayerId = playerId;
				this.PlayerName = playerName;
			}
		}

		private class ApiPlayerBaseComparer : IEqualityComparer<ApiPlayerBase>
		{
			public bool Equals([AllowNull] ApiPlayerBase x, [AllowNull] ApiPlayerBase y)
			{
				return x.PlayerId == y.PlayerId;
			}

			public int GetHashCode([DisallowNull] ApiPlayerBase obj)
			{
				return base.GetHashCode();
			}
		}
		#endregion HELPER CLASSES
	}
}
