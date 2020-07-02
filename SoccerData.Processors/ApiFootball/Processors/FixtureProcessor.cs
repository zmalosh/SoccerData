using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SoccerData.Model;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace SoccerData.Processors.ApiFootball.Processors
{
	public class FixtureProcessor : IProcessor
	{
		private readonly int ApiFootballFixtureId;
		private readonly JsonUtility JsonUtility;
		private readonly bool CheckEntitiesExist;

		private const int CompareResult_Equals = 0;

		private const int NullIntDictKey = int.MinValue;

		public FixtureProcessor(int apiFootballFixtureId, bool checkEntitiesExist = true, int? cacheLengthSec = 120 * 24 * 60 * 60)
		{
			this.ApiFootballFixtureId = apiFootballFixtureId;
			this.CheckEntitiesExist = checkEntitiesExist;
			this.JsonUtility = new JsonUtility(cacheLengthSec, sourceType: JsonUtility.JsonSourceType.ApiFootball); // 230K+ FIXTURES.... SAVE FINISHED GAMES FOR A LONG TIME (120 DAYS?) TO AVOID QUOTA ISSUES
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

			Feeds.FixtureFeed.ApiFixture feedFixture = feed.Result.Fixtures.SingleOrDefault();

			if (feedFixture == null)
			{
				return;
			}

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
				bool hasHomeTeamName = feedFixture.Lineups.Any(x => string.Equals(x.Key, homeTeamName, StringComparison.InvariantCultureIgnoreCase));
				bool hasAwayTeamName = feedFixture.Lineups.Any(x => string.Equals(x.Key, awayTeamName, StringComparison.InvariantCultureIgnoreCase));
				if (!hasHomeTeamName || !hasAwayTeamName)
				{
					if (hasHomeTeamName && !hasAwayTeamName)
					{
						awayTeamName = feedFixture.Lineups.Keys.Single(x => !string.Equals(x, homeTeamName, StringComparison.InvariantCultureIgnoreCase));
					}
					else if (!hasHomeTeamName && hasAwayTeamName)
					{
						homeTeamName = feedFixture.Lineups.Keys.Single(x => !string.Equals(x, awayTeamName, StringComparison.InvariantCultureIgnoreCase));
					}
					else
					{
						throw new KeyNotFoundException("INVALID KEYS FOUND FOR FIXTURE LINEUPS");
					}
				}

				homeLineup = feedFixture.Lineups.Single(x => string.Equals(x.Key, homeTeamName, StringComparison.InvariantCultureIgnoreCase)).Value;
				awayLineup = feedFixture.Lineups.Single(x => string.Equals(x.Key, awayTeamName, StringComparison.InvariantCultureIgnoreCase)).Value;
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
							dbCoaches.Add(dbHomeCoach.CoachId, dbHomeCoach); // DUE TO BAD DATA, HOME COACH AND AWAY COACH MAY BE THE SAME (API GAME 126635)
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

						var dbPlayer = dbContext.Players.SingleOrDefault(x => x.ApiFootballId == missingApiPlayerId);
						if (dbPlayer == null)
						{
							dbPlayer = new Player
							{
								ApiFootballId = missingApiPlayerId,
								ApiFootballName = apiPlayerBase.PlayerName,
								PlayerName = apiPlayerBase.PlayerName
							};
						}

						var dbPlayerSeason = new PlayerSeason
						{
							Player = dbPlayer,
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
			// HAVE EACH dbFixtureEvent AVAILABLE. ILookup IS AN IMMUTABLE TYPE, SO A DICTIONARY WITH THE COUNT IS ALSO NEEDED TO TRACK THE NUMBER OF OCCURANCES OF EACH EVENT.
			// THE ILookup IS JUST TO FIND FIND THE DB REFERENCE FOR EACH EVENT TO MANIPULATE
			var dbFixtureEventLookup = dbContext.FixtureEvents.Where(x => x.FixtureId == dbFixtureId).ToLookup(x => GetFixtureEventKey(x));
			var dbFixtureEventToDeleteCountDict = dbContext.FixtureEvents.Where(x => x.FixtureId == dbFixtureId).ToList().GroupBy(x => GetFixtureEventKey(x)).ToDictionary(x => x.Key, y => y.Count());

			var apiFixtureEvents = feedFixture.Events?.Where(x => x.TeamId.HasValue).ToList();
			if (apiFixtureEvents != null && apiFixtureEvents.Count > 0)
			{
				foreach (var apiFixtureEvent in apiFixtureEvents)
				{
					int dbTeamSeasonId = apiFixtureEvent.TeamId == apiAwayTeamId ? dbAwayTeamSeasonId : dbHomeTeamSeasonId;
					int? dbPlayerSeasonId = null;
					if (dbPlayerSeasonDict != null && apiFixtureEvent.PlayerId.HasValue && dbPlayerSeasonDict.TryGetValue(apiFixtureEvent.PlayerId.Value, out int intPlayerSeasonId))
					{
						dbPlayerSeasonId = intPlayerSeasonId;
					}
					int? dbSecondaryPlayerSeasonId = null;
					if (dbPlayerSeasonDict != null && apiFixtureEvent.SecondaryPlayerId.HasValue && dbPlayerSeasonDict.TryGetValue(apiFixtureEvent.SecondaryPlayerId.Value, out intPlayerSeasonId))
					{
						dbSecondaryPlayerSeasonId = intPlayerSeasonId;
					}

					// IT IS POSSIBLE TO HAVE MULTIPLE IDENTICAL EVENTS IN THE SAME MINUTE
					// API FIXTURE ID 185030 - 2 GOALS BY SAME PLAYER IN SAME MINUTE
					// USE LOOKUP TO DETERMINE CORRECT AMOUNT OF EXISTENCE
					var eventKey = GetFixtureEventKey(apiFixtureEvent.Elapsed, apiFixtureEvent.ElapsedPlus, dbPlayerSeasonId, dbTeamSeasonId, apiFixtureEvent.EventType, apiFixtureEvent.EventDetail);
					var dbCount = dbFixtureEventToDeleteCountDict.TryGetValue(eventKey, out int tempInt) ? tempInt : 0;
					FixtureEvent dbFixtureEvent;
					if (dbCount == 0)
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
						dbFixtureEvent = dbFixtureEventLookup[eventKey].Skip(dbCount - 1).First(); // TAKE LAST ENTRY IN LOOKUP. AS THE COUNT IN THE dbFixtureEventCount DICTIONARY IS DECREMENTED, THE SELECTED EVENT WILL MOVE DOWN THE LIST
						if (dbCount == 1)
						{
							dbFixtureEventToDeleteCountDict.Remove(eventKey);
						}
						else
						{
							dbFixtureEventToDeleteCountDict[eventKey] = dbCount - 1;
						}

						if ((!string.IsNullOrEmpty(apiFixtureEvent.EventComments) && dbFixtureEvent.EventComment != apiFixtureEvent.EventComments)
							|| (!string.IsNullOrEmpty(apiFixtureEvent.EventDetail) && dbFixtureEvent.EventDetail != apiFixtureEvent.EventDetail)
							|| (dbSecondaryPlayerSeasonId.HasValue && (!dbFixtureEvent.SecondaryPlayerSeasonId.HasValue || dbFixtureEvent.SecondaryPlayerSeasonId != dbSecondaryPlayerSeasonId))
							|| (!dbSecondaryPlayerSeasonId.HasValue && dbFixtureEvent.SecondaryPlayerSeasonId.HasValue))
						{
							dbFixtureEvent.EventComment = apiFixtureEvent.EventComments;
							dbFixtureEvent.EventDetail = apiFixtureEvent.EventDetail;
							dbFixtureEvent.SecondaryPlayerSeasonId = dbSecondaryPlayerSeasonId;
							hasUpdate = true;
						}
					}
				}
				if (dbFixtureEventToDeleteCountDict.Count > 0)
				{
					foreach (var dbFixtureEventCountEntry in dbFixtureEventToDeleteCountDict)
					{
						var dbFixtureEventLookupEntry = dbFixtureEventLookup[dbFixtureEventCountEntry.Key];
						int dbFixtureEventCount = dbFixtureEventLookupEntry.Count();
						if (dbFixtureEventCount >= 1)
						{
							for (int i = dbFixtureEventCount; i >= 1; i--)
							{
								var dbFixtureEvent = dbFixtureEventLookupEntry.Skip(i - 1).First();
								dbContext.FixtureEvents.Remove(dbFixtureEvent);
							}
						}
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

			#region PLAYER BOXSCORE
			if (apiPlayerBases != null && apiPlayerBases.Count > 0)
			{
				var dbPlayerBoxscores = dbContext.PlayerBoxscores.Where(x => x.FixtureId == dbFixtureId && x.PlayerSeason != null && x.PlayerSeason.Player != null).ToDictionary(x => x.PlayerSeason.Player.ApiFootballId, y => y);
				bool hasApiPlayerBoxscores = feedFixture?.PlayerBoxscores != null;
				bool hasApiLineups = feedFixture?.AllLineupPlayers != null;
				foreach (var apiPlayerBase in apiPlayerBases)
				{
					var dbPlayerSeasonId = dbPlayerSeasonDict[apiPlayerBase.PlayerId];
					if (!dbPlayerBoxscores.TryGetValue(apiPlayerBase.PlayerId, out PlayerBoxscore dbPlayerBoxscore))
					{
						dbPlayerBoxscore = new PlayerBoxscore
						{
							PlayerSeasonId = dbPlayerSeasonId,
							IsStarter = apiPlayerBase.IsStarter,
							FixtureId = dbFixtureId,
							TeamSeasonId = apiPlayerBase.TeamId == feedFixture.HomeTeam.TeamId ? dbHomeTeamSeasonId : dbAwayTeamSeasonId
						};
						dbContext.PlayerBoxscores.Add(dbPlayerBoxscore);
						hasUpdate = true;
					}

					if (hasApiPlayerBoxscores || hasApiLineups)
					{
						Feeds.FixtureFeed.ApiPlayerBoxscore apiPlayerBoxscore = null;
						if (apiPlayerBase.BoxscorePlayerId.HasValue && apiPlayerBase.JerseyNumber.HasValue)
						{
							apiPlayerBoxscore = feedFixture.PlayerBoxscores.Where(x => x.PlayerId.HasValue).FirstOrDefault(x => x.Number == apiPlayerBase.JerseyNumber && x.TeamId == apiPlayerBase.TeamId);
						}

						Feeds.FixtureFeed.ApiLineupPlayerWithStarterStatus apiPlayerLineup = null;
						if (apiPlayerBase.LineupPlayerId.HasValue && apiPlayerBase.JerseyNumber.HasValue)
						{
							apiPlayerLineup = feedFixture.AllLineupPlayers.Where(x => x.PlayerId.HasValue).FirstOrDefault(x => x.Number == apiPlayerBase.JerseyNumber && x.TeamId == apiPlayerBase.TeamId);
						}

						if (apiPlayerBoxscore != null || apiPlayerLineup != null)
						{
							if (PopulatePlayerBoxscore(apiPlayerBoxscore, apiPlayerLineup, ref dbPlayerBoxscore))
							{
								hasUpdate = true;
							}
						}
					}
				}
			}
			#endregion PLAYER BOXSCORE

			if (hasUpdate)
			{
				dbContext.SaveChanges();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name">Player's Full Name</param>
		/// <returns>Reduces player's name to initial of first name and final last name</returns>
		private string ShortenName(string name)
		{
			var arrName = name.Split(" ").ToList();
			if (arrName.Count == 1)
			{
				return name; // NO SHORTENING AVAILABLE
			}
			string firstInitial = arrName[0].First() + ".";
			string result = firstInitial + " " + arrName[arrName.Count - 1];
			return result;
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
			List<ApiPlayerBase> apiPlayerBases = null;
			// USE LINEUPS AS BASE IF POSSIBLE, INCLUDING MAPPING FROM BOXSCORE BY JERSEY NUMBER AND TEAM
			if (feedFixture.Lineups != null && feedFixture.Lineups.Count > 0)
			{
				if (feedFixture.PlayerBoxscores != null && feedFixture.PlayerBoxscores.Count > 0)
				{
					// ALL PLAYERS SHOULD BE IN LINEUP. THERE SHOULD BE A MATCH FROM LINEUP TO BOXSCORE. VERIFY MATCHES
					var apiBoxscorePlayerMapping = from apiLineupPlayer in feedFixture.AllLineupPlayers
												   join apiBoxscorePlayer in feedFixture.PlayerBoxscores
														on new { apiLineupPlayer.TeamId, Number = apiLineupPlayer.Number }
														equals new { apiBoxscorePlayer.TeamId, Number = (int?)apiBoxscorePlayer.Number }
														into pbx
												   from pbx2 in pbx.DefaultIfEmpty() // pbx2 IS PLAYER BOXSCORES BUT WITH NULL VALUES IF NO MATCH (FORCE LEFT JOIN INSTEAD OF INNER JOIN)
												   select new
												   {
													   TeamId = apiLineupPlayer.TeamId,
													   PlayerName = apiLineupPlayer.PlayerName,
													   JerseyNumber = apiLineupPlayer.Number,
													   LineupPlayerId = apiLineupPlayer.PlayerId,
													   BoxscorePlayerId = pbx2?.PlayerId,
													   IsStarter = apiLineupPlayer.IsStarter
												   };
					apiPlayerBases = apiBoxscorePlayerMapping
											.Where(x => x.LineupPlayerId.HasValue)
											.Select(x => new ApiPlayerBase(x.LineupPlayerId.Value, x.TeamId, x.PlayerName, x.JerseyNumber, x.LineupPlayerId, x.BoxscorePlayerId, x.IsStarter)).ToList();
				}
				else
				{
					apiPlayerBases = feedFixture.AllLineupPlayers?
													.Where(x => x.PlayerId.HasValue)
													.Select(x => new ApiPlayerBase(x.PlayerId.Value, x.TeamId, x.PlayerName, x.Number, x.PlayerId.Value, null, x.IsStarter)).ToList();
				}
			}
			else if (feedFixture.PlayerBoxscores != null && feedFixture.PlayerBoxscores.Count > 0)
			{
				apiPlayerBases = feedFixture.PlayerBoxscores.Select(x => new ApiPlayerBase(x.PlayerId.Value, x.TeamId, x.PlayerName, x.Number, null, x.PlayerId, !(x.IsSubstitute ?? true))).ToList();
			}

			if (apiPlayerBases == null || apiPlayerBases.Count == 0)
			{
				return null;
			}

			if (apiPlayerBases.GroupBy(x => x.PlayerId).Any(y => y.Count() > 1))
			{
				var groupedApiPlayerBases = apiPlayerBases.GroupBy(x => x.PlayerId);
				apiPlayerBases = new List<ApiPlayerBase>();
				foreach (var groupedApiPlayerBase in groupedApiPlayerBases)
				{
					if (groupedApiPlayerBase.Count() == 1)
					{
						apiPlayerBases.Add(groupedApiPlayerBase.First());
					}
					else
					{
						apiPlayerBases.Add(groupedApiPlayerBase.OrderBy(y => y.BoxscorePlayerId.HasValue ? 0 : 1).ThenBy(y => y.IsStarter ? 0 : 1).First());
					}
				}
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

		#region PLAYER BOXSCORE HELPERS
		private bool PopulatePlayerBoxscore(
			Feeds.FixtureFeed.ApiPlayerBoxscore apiPlayerBoxscore,
			Feeds.FixtureFeed.ApiLineupPlayerWithStarterStatus apiPlayerLineup,
			ref PlayerBoxscore dbPlayerBoxscore)
		{
			int? playerNumber = null;
			string position = null;

			if (apiPlayerBoxscore != null)
			{
				playerNumber = apiPlayerBoxscore.Number;
				position = apiPlayerBoxscore.Position;
			}
			else if (apiPlayerLineup != null)
			{
				playerNumber = apiPlayerLineup.Number;
				position = apiPlayerLineup.Position;
			}

			bool hasUpdate = false;
			if (dbPlayerBoxscore.JerseyNumber != playerNumber)
			{
				dbPlayerBoxscore.JerseyNumber = playerNumber;
				hasUpdate = true;
			}
			if (dbPlayerBoxscore.Position != position)
			{
				dbPlayerBoxscore.Position = position;
				hasUpdate = true;
			}

			// SET ROSTER TYPES IF LINEUP IS AVAILABLE
			if (apiPlayerLineup != null)
			{
				// CAN SET ROSTER TYPE AND HAD TO PLAY AS STARTER
				if (apiPlayerLineup.IsStarter && (!dbPlayerBoxscore.IsStarter.HasValue || !dbPlayerBoxscore.IsStarter.Value || !dbPlayerBoxscore.Played.HasValue || !dbPlayerBoxscore.Played.Value || !dbPlayerBoxscore.IsBench.HasValue || dbPlayerBoxscore.IsBench.Value))
				{
					dbPlayerBoxscore.IsStarter = true;
					dbPlayerBoxscore.IsBench = false;
					dbPlayerBoxscore.Played = true;
					hasUpdate = true;
				}

				// CAN SET ROSTER TYPE, BUT NOT IF PLAYED
				if (!apiPlayerLineup.IsStarter && (!dbPlayerBoxscore.IsStarter.HasValue || dbPlayerBoxscore.IsStarter.Value || !dbPlayerBoxscore.IsBench.HasValue || !dbPlayerBoxscore.IsBench.Value))
				{
					dbPlayerBoxscore.IsStarter = false;
					dbPlayerBoxscore.IsBench = true;
					hasUpdate = true;
				}
			}

			if (apiPlayerBoxscore != null)
			{
				if (apiPlayerBoxscore.IsCaptain != dbPlayerBoxscore.IsCaptain
					|| apiPlayerBoxscore.MinutesPlayed != dbPlayerBoxscore.MinutesPlayed
					|| apiPlayerBoxscore.Offsides != dbPlayerBoxscore.Offsides
					|| apiPlayerBoxscore.Rating != dbPlayerBoxscore.Rating
					|| apiPlayerBoxscore.UpdateAt != dbPlayerBoxscore.ApiFootballLastUpdate)
				{
					dbPlayerBoxscore.IsCaptain = apiPlayerBoxscore.IsCaptain;
					dbPlayerBoxscore.MinutesPlayed = apiPlayerBoxscore.MinutesPlayed;
					dbPlayerBoxscore.Offsides = apiPlayerBoxscore.Offsides;
					dbPlayerBoxscore.Rating = apiPlayerBoxscore.Rating;
					dbPlayerBoxscore.ApiFootballLastUpdate = apiPlayerBoxscore.UpdateAt;
					dbPlayerBoxscore.Played = apiPlayerBoxscore.MinutesPlayed > 0;
					hasUpdate = true;
				}

				if (apiPlayerBoxscore.Cards != null)
				{
					if (apiPlayerBoxscore.Cards.Red != dbPlayerBoxscore.RedCards
						|| apiPlayerBoxscore.Cards.Yellow != dbPlayerBoxscore.YellowCards)
					{
						dbPlayerBoxscore.RedCards = apiPlayerBoxscore.Cards.Red;
						dbPlayerBoxscore.YellowCards = apiPlayerBoxscore.Cards.Yellow;
						hasUpdate = true;
					}
				}

				if (apiPlayerBoxscore.Dribbles != null)
				{
					if (apiPlayerBoxscore.Dribbles.Attempts != dbPlayerBoxscore.DribblesAttempted
						|| apiPlayerBoxscore.Dribbles.Past != dbPlayerBoxscore.DribblesPastDef
						|| apiPlayerBoxscore.Dribbles.Success != dbPlayerBoxscore.DribblesSuccessful)
					{
						dbPlayerBoxscore.DribblesAttempted = apiPlayerBoxscore.Dribbles.Attempts;
						dbPlayerBoxscore.DribblesPastDef = apiPlayerBoxscore.Dribbles.Past;
						dbPlayerBoxscore.DribblesSuccessful = apiPlayerBoxscore.Dribbles.Success;
						hasUpdate = true;
					}
				}

				if (apiPlayerBoxscore.Duels != null)
				{
					if (apiPlayerBoxscore.Duels.Total != dbPlayerBoxscore.DuelsTotal
						|| apiPlayerBoxscore.Duels.Won != dbPlayerBoxscore.DuelsWon)
					{
						dbPlayerBoxscore.DuelsTotal = apiPlayerBoxscore.Duels.Total;
						dbPlayerBoxscore.DuelsWon = apiPlayerBoxscore.Duels.Won;
						hasUpdate = true;
					}
				}

				if (apiPlayerBoxscore.Fouls != null)
				{
					if (apiPlayerBoxscore.Fouls.Committed != dbPlayerBoxscore.FoulsCommitted
						|| apiPlayerBoxscore.Fouls.Drawn != dbPlayerBoxscore.FoulsSuffered)
					{
						dbPlayerBoxscore.FoulsCommitted = apiPlayerBoxscore.Fouls.Committed;
						dbPlayerBoxscore.FoulsSuffered = apiPlayerBoxscore.Fouls.Drawn;
						hasUpdate = true;
					}
				}

				if (apiPlayerBoxscore.Goals != null)
				{
					if (apiPlayerBoxscore.Goals.Assists != dbPlayerBoxscore.Assists
						|| apiPlayerBoxscore.Goals.Conceded != dbPlayerBoxscore.GoalsConceded
						|| apiPlayerBoxscore.Goals.Total != dbPlayerBoxscore.Goals)
					{
						dbPlayerBoxscore.Assists = apiPlayerBoxscore.Goals.Assists;
						dbPlayerBoxscore.GoalsConceded = apiPlayerBoxscore.Goals.Conceded;
						dbPlayerBoxscore.Goals = apiPlayerBoxscore.Goals.Total;
						hasUpdate = true;
					}
				}

				if (apiPlayerBoxscore.Passes != null)
				{
					if (apiPlayerBoxscore.Passes.Accuracy != dbPlayerBoxscore.PassAccuracy
						|| apiPlayerBoxscore.Passes.Key != dbPlayerBoxscore.KeyPasses
						|| apiPlayerBoxscore.Passes.Total != dbPlayerBoxscore.PassAttempts)
					{
						dbPlayerBoxscore.PassAccuracy = apiPlayerBoxscore.Passes.Accuracy;
						dbPlayerBoxscore.KeyPasses = apiPlayerBoxscore.Passes.Key;
						dbPlayerBoxscore.PassAttempts = apiPlayerBoxscore.Passes.Total;
						hasUpdate = true;
					}
				}

				if (apiPlayerBoxscore.Penalty != null)
				{
					if (apiPlayerBoxscore.Penalty.Commited != dbPlayerBoxscore.PenaltiesCommitted
						|| apiPlayerBoxscore.Penalty.Missed != dbPlayerBoxscore.PenaltiesMissed
						|| apiPlayerBoxscore.Penalty.Saved != dbPlayerBoxscore.PenaltiesSaved
						|| apiPlayerBoxscore.Penalty.Success != dbPlayerBoxscore.PenaltiesScored
						|| apiPlayerBoxscore.Penalty.Won != dbPlayerBoxscore.PenaltiesWon)
					{
						dbPlayerBoxscore.PenaltiesCommitted = apiPlayerBoxscore.Penalty.Commited;
						dbPlayerBoxscore.PenaltiesMissed = apiPlayerBoxscore.Penalty.Missed;
						dbPlayerBoxscore.PenaltiesSaved = apiPlayerBoxscore.Penalty.Saved;
						dbPlayerBoxscore.PenaltiesScored = apiPlayerBoxscore.Penalty.Success;
						dbPlayerBoxscore.PenaltiesWon = apiPlayerBoxscore.Penalty.Won;
						hasUpdate = true;
					}
				}

				if (apiPlayerBoxscore.Shots != null)
				{
					if (apiPlayerBoxscore.Shots.On != dbPlayerBoxscore.ShotsOnGoal
						|| apiPlayerBoxscore.Shots.Total != dbPlayerBoxscore.ShotsTaken)
					{
						dbPlayerBoxscore.ShotsOnGoal = apiPlayerBoxscore.Shots.On;
						dbPlayerBoxscore.ShotsTaken = apiPlayerBoxscore.Shots.Total;
						hasUpdate = true;
					}
				}

				if (apiPlayerBoxscore.Tackles != null)
				{
					if (apiPlayerBoxscore.Tackles.Blocks != dbPlayerBoxscore.Blocks
						|| apiPlayerBoxscore.Tackles.Interceptions != dbPlayerBoxscore.Interceptions
						|| apiPlayerBoxscore.Tackles.Total != dbPlayerBoxscore.Tackles)
					{
						dbPlayerBoxscore.Blocks = apiPlayerBoxscore.Tackles.Blocks;
						dbPlayerBoxscore.Interceptions = apiPlayerBoxscore.Tackles.Interceptions;
						dbPlayerBoxscore.Tackles = apiPlayerBoxscore.Tackles.Total;
						hasUpdate = true;
					}
				}
			}

			return hasUpdate;
		}
		#endregion PLAYER BOXSCORE HELPERS

		#region HELPER CLASSES

		private class ApiPlayerBase
		{
			public int PlayerId { get; set; }
			public int TeamId { get; set; }
			public string PlayerName { get; set; }
			public int? JerseyNumber { get; set; }
			public int? LineupPlayerId { get; set; }
			public int? BoxscorePlayerId { get; set; }
			public bool IsStarter { get; set; }

			public ApiPlayerBase(int playerId, int teamId, string playerName, int? jerseyNumber, int? lineupPlayerId, int? boxscorePlayerId, bool isStarter)
			{
				this.PlayerId = playerId;
				this.TeamId = teamId;
				this.PlayerName = playerName;
				this.JerseyNumber = jerseyNumber;
				this.LineupPlayerId = lineupPlayerId;
				this.BoxscorePlayerId = boxscorePlayerId;
				this.IsStarter = isStarter;
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
