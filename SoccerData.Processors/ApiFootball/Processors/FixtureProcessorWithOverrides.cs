//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace SoccerData.Processors.ApiFootball.Processors
//{
//	class FixtureProcessorWithOverrides
//	{
//	}
//}
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using SoccerData.Model;
//using Microsoft.EntityFrameworkCore;
//using System.Diagnostics.CodeAnalysis;

//namespace SoccerData.Processors.ApiFootball.Processors
//{
//	public class FixtureProcessor : IProcessor
//	{
//		private readonly int ApiFootballFixtureId;
//		private readonly JsonUtility JsonUtility;
//		private readonly bool CheckEntitiesExist;

//		private const int NullIntDictKey = int.MinValue;

//		public FixtureProcessor(int apiFootballFixtureId, bool checkEntitiesExist = true)
//		{
//			this.ApiFootballFixtureId = apiFootballFixtureId;
//			this.CheckEntitiesExist = checkEntitiesExist;
//			this.JsonUtility = new JsonUtility(120 * 24 * 60 * 60, sourceType: JsonUtility.JsonSourceType.ApiFootball); // 230K+ FIXTURES.... SAVE FINISHED GAMES FOR A LONG TIME (120 DAYS?) TO AVOID QUOTA ISSUES
//		}

//		public void Run(SoccerDataContext dbContext)
//		{
//			var dbFixture = dbContext.Fixtures.Single(x => x.ApiFootballId == this.ApiFootballFixtureId);
//			var isFixtureFinal = string.Equals("Match Finished", dbFixture.Status, StringComparison.CurrentCultureIgnoreCase);

//			if (!dbFixture.HomeTeamSeasonId.HasValue
//				|| !dbFixture.AwayTeamSeasonId.HasValue
//				|| !isFixtureFinal)
//			{
//				return;
//			}

//			var url = Feeds.FixtureFeed.GetFeedUrlByFixtureId(this.ApiFootballFixtureId);
//			var rawJson = JsonUtility.GetRawJsonFromUrl(url);
//			var feed = Feeds.FixtureFeed.FromJson(rawJson);

//			Feeds.FixtureFeed.ApiFixture feedFixture = feed.Result.Fixtures.Single();

//			int dbFixtureId = dbFixture.FixtureId;
//			int dbHomeTeamSeasonId = dbFixture.HomeTeamSeasonId.Value;
//			int dbAwayTeamSeasonId = dbFixture.AwayTeamSeasonId.Value;
//			int apiAwayTeamId = feedFixture.AwayTeam.TeamId;
//			int apiHomeTeamId = feedFixture.HomeTeam.TeamId;
//			int apiLeagueId = feedFixture.LeagueId;

//			int? homeCoachId = null;
//			int? awayCoachId = null;

//			var apiPlayerBases = GetApiPlayerBases(feedFixture);
//			var dbPlayerSeasonDict = GetDbPlayerSeasonDict(dbContext, apiPlayerBases, dbFixture.CompetitionSeasonId);

//			bool hasUpdate = false;

//			Feeds.FixtureFeed.ApiLineup homeLineup = null;
//			Feeds.FixtureFeed.ApiLineup awayLineup = null;

//			#region GET FORMATIONS
//			string homeFormation = null;
//			string awayFormation = null;
//			if (feedFixture.Lineups != null && feedFixture.Lineups.Count == 2)
//			{
//				string homeTeamName = feedFixture.HomeTeam.TeamName;
//				string awayTeamName = feedFixture.AwayTeam.TeamName;

//				// MISMATCH BETWEEN PLAYING TEAM NAMES AND LINEUP DICT KEYS HAS OCCURRED (API fixtureID: 188155)
//				bool hasHomeTeamName = feedFixture.Lineups.ContainsKey(homeTeamName);
//				bool hasAwayTeamName = feedFixture.Lineups.ContainsKey(awayTeamName);
//				if (!hasHomeTeamName || !hasAwayTeamName)
//				{
//					if (hasHomeTeamName && !hasAwayTeamName)
//					{
//						awayTeamName = feedFixture.Lineups.Keys.Single(x => x != homeTeamName);
//					}
//					else if (!hasHomeTeamName && hasAwayTeamName)
//					{
//						homeTeamName = feedFixture.Lineups.Keys.Single(x => x != awayTeamName);
//					}
//					else
//					{
//						throw new KeyNotFoundException("INVALID KEYS FOUND FOR FIXTURE LINEUPS");
//					}
//				}

//				homeLineup = feedFixture.Lineups[homeTeamName];
//				awayLineup = feedFixture.Lineups[awayTeamName];
//				homeFormation = homeLineup.Formation;
//				awayFormation = awayLineup.Formation;
//			}
//			#endregion GET FORMATIONS

//			#region ENSURE COACHES EXIST
//			if (this.CheckEntitiesExist)
//			{
//				if (homeLineup != null || awayLineup != null)
//				{
//					var apiCoachIds = new[] { homeLineup.CoachId, awayLineup.CoachId };
//					var dbCoaches = dbContext.Coaches.Where(x => apiCoachIds.Contains(x.ApiFootballId)).ToDictionary(x => x.ApiFootballId, y => y);

//					if (homeLineup?.CoachId != null)
//					{
//						if (!dbCoaches.TryGetValue(homeLineup.CoachId.Value, out Coach dbHomeCoach))
//						{
//							dbHomeCoach = new Coach
//							{
//								ApiFootballId = homeLineup.CoachId.Value,
//								CoachName = homeLineup.Coach
//							};
//							dbContext.Coaches.Add(dbHomeCoach);
//							dbContext.SaveChanges();
//						}
//						homeCoachId = dbHomeCoach.CoachId;
//					}
//					if (awayLineup?.CoachId != null)
//					{
//						if (!dbCoaches.TryGetValue(awayLineup.CoachId.Value, out Coach dbAwayCoach))
//						{
//							dbAwayCoach = new Coach
//							{
//								ApiFootballId = awayLineup.CoachId.Value,
//								CoachName = awayLineup.Coach
//							};
//							dbContext.Coaches.Add(dbAwayCoach);
//							dbContext.SaveChanges();
//						}
//						awayCoachId = dbAwayCoach.CoachId;
//					}
//				}
//			}
//			#endregion ENSURE COACHES EXIST 

//			#region ENSURE PLAYERS EXIST
//			if (this.CheckEntitiesExist)
//			{
//				var missingApiPlayerIds = apiPlayerBases?.Select(x => x.PlayerId).Where(x => !dbPlayerSeasonDict.ContainsKey(x)).ToList();
//				if (missingApiPlayerIds != null && missingApiPlayerIds.Count > 0)
//				{
//					foreach (var missingApiPlayerId in missingApiPlayerIds)
//					{
//						var apiPlayerBase = apiPlayerBases.Single(x => x.PlayerId == missingApiPlayerId);

//						var dbPlayer = dbContext.Players.SingleOrDefault(x => x.ApiFootballId == missingApiPlayerId);
//						if (dbPlayer == null)
//						{
//							dbPlayer = new Player
//							{
//								ApiFootballId = missingApiPlayerId,
//								ApiFootballName = apiPlayerBase.PlayerName,
//								PlayerName = apiPlayerBase.PlayerName
//							};
//						}

//						var dbPlayerSeason = new PlayerSeason
//						{
//							Player = dbPlayer,
//							CompetitionSeasonId = dbFixture.CompetitionSeasonId
//						};
//						dbContext.Add(dbPlayerSeason);
//					}
//					dbContext.SaveChanges();
//					dbPlayerSeasonDict = GetDbPlayerSeasonDict(dbContext, apiPlayerBases, dbFixture.CompetitionSeasonId);
//				}
//			}
//			#endregion ENSURE PLAYERS EXIST

//			#region UPDATE FORAMATION AND COACH IF NECESSARY
//			if (homeCoachId.HasValue && dbFixture.HomeCoachId != homeCoachId)
//			{
//				dbFixture.HomeCoachId = homeCoachId;
//				hasUpdate = true;
//			}
//			if (awayCoachId.HasValue && dbFixture.AwayCoachId != awayCoachId)
//			{
//				dbFixture.AwayCoachId = awayCoachId;
//				hasUpdate = true;
//			}
//			if (!string.IsNullOrEmpty(homeFormation) && dbFixture.HomeFormation != homeFormation)
//			{
//				dbFixture.HomeFormation = homeFormation;
//				hasUpdate = true;
//			}
//			if (!string.IsNullOrEmpty(awayFormation) && dbFixture.AwayFormation != awayFormation)
//			{
//				dbFixture.AwayFormation = awayFormation;
//				hasUpdate = true;
//			}
//			#endregion UPDATE FORAMATION AND COACH IF NECESSARY

//			#region FIXTURE EVENTS
//			// HAVE EACH dbFixtureEvent AVAILABLE. ILookup IS AN IMMUTABLE TYPE, SO A DICTIONARY WITH THE COUNT IS ALSO NEEDED TO TRACK THE NUMBER OF OCCURANCES OF EACH EVENT.
//			// THE ILookup IS JUST TO FIND FIND THE DB REFERENCE FOR EACH EVENT TO MANIPULATE
//			var dbFixtureEventLookup = dbContext.FixtureEvents.Where(x => x.FixtureId == dbFixtureId).ToLookup(x => GetFixtureEventKey(x));
//			var dbFixtureEventToDeleteCountDict = dbContext.FixtureEvents.Where(x => x.FixtureId == dbFixtureId).ToList().GroupBy(x => GetFixtureEventKey(x)).ToDictionary(x => x.Key, y => y.Count());

//			var apiFixtureEvents = feedFixture.Events?.Where(x => x.TeamId.HasValue).ToList();
//			if (apiFixtureEvents != null && apiFixtureEvents.Count > 0)
//			{
//				foreach (var apiFixtureEvent in apiFixtureEvents)
//				{
//					int dbTeamSeasonId = apiFixtureEvent.TeamId == apiAwayTeamId ? dbAwayTeamSeasonId : dbHomeTeamSeasonId;
//					int? dbPlayerSeasonId = apiFixtureEvent.PlayerId.HasValue ? dbPlayerSeasonDict[apiFixtureEvent.PlayerId.Value] : (int?)null;
//					int? dbSecondaryPlayerSeasonId = apiFixtureEvent.SecondaryPlayerId.HasValue ? dbPlayerSeasonDict[apiFixtureEvent.SecondaryPlayerId.Value] : (int?)null;

//					// IT IS POSSIBLE TO HAVE MULTIPLE IDENTICAL EVENTS IN THE SAME MINUTE
//					// API FIXTURE ID 185030 - 2 GOALS BY SAME PLAYER IN SAME MINUTE
//					// USE LOOKUP TO DETERMINE CORRECT AMOUNT OF EXISTENCE
//					var eventKey = GetFixtureEventKey(apiFixtureEvent.Elapsed, apiFixtureEvent.ElapsedPlus, dbPlayerSeasonId, dbTeamSeasonId, apiFixtureEvent.EventType, apiFixtureEvent.EventDetail);
//					var dbCount = dbFixtureEventToDeleteCountDict.TryGetValue(eventKey, out int tempInt) ? tempInt : 0;
//					FixtureEvent dbFixtureEvent;
//					if (dbCount == 0)
//					{
//						dbFixtureEvent = new FixtureEvent
//						{
//							EventComment = apiFixtureEvent.EventComments,
//							EventDetail = apiFixtureEvent.EventDetail,
//							EventType = apiFixtureEvent.EventType,
//							FixtureId = dbFixtureId,
//							EventTime = apiFixtureEvent.Elapsed,
//							EventTimePlus = apiFixtureEvent.ElapsedPlus,
//							PlayerSeasonId = dbPlayerSeasonId,
//							SecondaryPlayerSeasonId = dbSecondaryPlayerSeasonId,
//							TeamSeasonId = dbTeamSeasonId
//						};
//						dbContext.FixtureEvents.Add(dbFixtureEvent);
//						hasUpdate = true;
//					}
//					else
//					{
//						dbFixtureEvent = dbFixtureEventLookup[eventKey].Skip(dbCount - 1).First(); // TAKE LAST ENTRY IN LOOKUP. AS THE COUNT IN THE dbFixtureEventCount DICTIONARY IS DECREMENTED, THE SELECTED EVENT WILL MOVE DOWN THE LIST
//						if (dbCount == 1)
//						{
//							dbFixtureEventToDeleteCountDict.Remove(eventKey);
//						}
//						else
//						{
//							dbFixtureEventToDeleteCountDict[eventKey] = dbCount - 1;
//						}

//						if ((!string.IsNullOrEmpty(apiFixtureEvent.EventComments) && dbFixtureEvent.EventComment != apiFixtureEvent.EventComments)
//							|| (!string.IsNullOrEmpty(apiFixtureEvent.EventDetail) && dbFixtureEvent.EventDetail != apiFixtureEvent.EventDetail)
//							|| (dbSecondaryPlayerSeasonId.HasValue && (!dbFixtureEvent.SecondaryPlayerSeasonId.HasValue || dbFixtureEvent.SecondaryPlayerSeasonId != dbSecondaryPlayerSeasonId))
//							|| (!dbSecondaryPlayerSeasonId.HasValue && dbFixtureEvent.SecondaryPlayerSeasonId.HasValue))
//						{
//							dbFixtureEvent.EventComment = apiFixtureEvent.EventComments;
//							dbFixtureEvent.EventDetail = apiFixtureEvent.EventDetail;
//							dbFixtureEvent.SecondaryPlayerSeasonId = dbSecondaryPlayerSeasonId;
//							hasUpdate = true;
//						}
//					}
//				}
//				if (dbFixtureEventToDeleteCountDict.Count > 0)
//				{
//					foreach (var dbFixtureEventCountEntry in dbFixtureEventToDeleteCountDict)
//					{
//						var dbFixtureEventLookupEntry = dbFixtureEventLookup[dbFixtureEventCountEntry.Key];
//						int dbFixtureEventCount = dbFixtureEventLookupEntry.Count();
//						if (dbFixtureEventCount >= 1)
//						{
//							for (int i = dbFixtureEventCount; i >= 1; i--)
//							{
//								var dbFixtureEvent = dbFixtureEventLookupEntry.Skip(i - 1).First();
//								dbContext.FixtureEvents.Remove(dbFixtureEvent);
//							}
//						}
//					}
//					hasUpdate = true;
//				}
//			}
//			#endregion FIXTURE EVENTS

//			#region TEAM BOXSCORE
//			var apiTeamStatsDict = feedFixture.TeamStatistics;
//			if (apiTeamStatsDict == null)
//			{
//				if (!dbFixture.HasTeamBoxscores.HasValue || dbFixture.HasTeamBoxscores.Value)
//				{
//					hasUpdate = true;
//				}
//				dbFixture.HasTeamBoxscores = false;
//			}
//			else
//			{
//				var dbTeamBoxscores = dbContext.TeamBoxscores.Where(x => x.FixtureId == dbFixtureId);

//				var dbHomeBoxscore = dbTeamBoxscores?.SingleOrDefault(x => x.TeamSeasonId == dbHomeTeamSeasonId);
//				var dbAwayBoxscore = dbTeamBoxscores?.SingleOrDefault(x => x.TeamSeasonId == dbAwayTeamSeasonId);

//				if (dbHomeBoxscore == null)
//				{
//					dbHomeBoxscore = new TeamBoxscore
//					{
//						FixtureId = dbFixtureId,
//						TeamSeasonId = dbHomeTeamSeasonId,
//						OppTeamSeasonId = dbAwayTeamSeasonId,
//						IsHome = true
//					};
//					dbContext.TeamBoxscores.Add(dbHomeBoxscore);
//					hasUpdate = true;
//				}
//				if (dbAwayBoxscore == null)
//				{
//					dbAwayBoxscore = new TeamBoxscore
//					{
//						FixtureId = dbFixtureId,
//						TeamSeasonId = dbAwayTeamSeasonId,
//						OppTeamSeasonId = dbHomeTeamSeasonId,
//						IsHome = false,
//					};
//					dbContext.TeamBoxscores.Add(dbAwayBoxscore);
//					hasUpdate = true;
//				}

//				if (PopulateTeamBoxscore(apiTeamStatsDict, x => x.Home, ref dbHomeBoxscore))
//				{
//					hasUpdate = true;
//					dbFixture.HasTeamBoxscores = true;
//				}
//				if (PopulateTeamBoxscore(apiTeamStatsDict, x => x.Away, ref dbAwayBoxscore))
//				{
//					hasUpdate = true;
//					dbFixture.HasTeamBoxscores = true;
//				}

//				if (!dbFixture.HasTeamBoxscores.HasValue)
//				{
//					dbFixture.HasTeamBoxscores = false;
//				}
//			}
//			#endregion TEAM BOXSCORE

//			#region PLAYER BOXSCORE
//			bool hasApiLineups = feedFixture.Lineups != null && feedFixture.Lineups.Count > 0;
//			bool hasApiPlayers = feedFixture.PlayerBoxscores != null && feedFixture.PlayerBoxscores.Count > 0;
//			if (hasApiLineups || hasApiPlayers)
//			{
//				var apiPlayerIds = apiPlayerBases.Select(x => x.PlayerId).ToList();
//				var apiLineupPlayers = feedFixture.Lineups.SelectMany(x => new[] { x.Value.Starters, x.Value.Substitutes ?? new List<Feeds.FixtureFeed.ApiLineupPlayer>() }).SelectMany(x => x).ToList();

//				var dbPlayerBoxscores = dbContext.PlayerBoxscores.Where(x => x.FixtureId == dbFixtureId).ToDictionary(x => x.PlayerSeason.Player.ApiFootballId);
//				var apiPlayerBoxscores = feedFixture.PlayerBoxscores?.OrderBy(x => x.PlayerId).ToDictionary(x => GetApiPlayerBoxscorePlayerId(apiLeagueId, this.ApiFootballFixtureId, x.PlayerId, x.PlayerName, x.IsSubstitute, x.Number, x.TeamId, apiLineupPlayers));

//				var apiSubstitutionEvents = apiFixtureEvents?.Where(x => string.Equals("subst", x.EventType, StringComparison.CurrentCultureIgnoreCase) && x.SecondaryPlayerId.HasValue).ToList();
//				var apiStarters = feedFixture.Lineups.SelectMany(x => x.Value.Starters ?? new List<Feeds.FixtureFeed.ApiLineupPlayer>()).Where(x => x.PlayerId.HasValue).ToList();
//				var apiLineupPlayerWrappers = apiStarters.Select(x => new { IsStarter = true, LineupPlayer = x }).ToList();
//				var apiSubstitutes = feedFixture.Lineups.SelectMany(x => x.Value.Substitutes ?? new List<Feeds.FixtureFeed.ApiLineupPlayer>()).Where(x => x.PlayerId.HasValue).ToList();
//				if (apiSubstitutes != null && apiSubstitutes.Count > 0)
//				{
//					apiLineupPlayerWrappers.AddRange(apiSubstitutes?.Select(x => new { IsStarter = false, LineupPlayer = x }));
//				}

//				foreach (var apiLineupPlayerWrapper in apiLineupPlayerWrappers)
//				{
//					var isStarterFromLineup = apiLineupPlayerWrapper.IsStarter;
//					var apiLineupPlayer = apiLineupPlayerWrapper.LineupPlayer;

//					int apiLineupPlayerId = GetApiLineupPlayerPlayerId(apiLeagueId, this.ApiFootballFixtureId, apiLineupPlayer.PlayerId, apiLineupPlayer.PlayerName, apiLineupPlayer.Number);

//					Feeds.FixtureFeed.ApiPlayerBoxscore apiPlayerBoxscore = null;
//					apiPlayerBoxscores?.TryGetValue(apiLineupPlayerId, out apiPlayerBoxscore);

//					int dbTeamSeasonId = apiLineupPlayer.TeamId == apiHomeTeamId ? dbHomeTeamSeasonId : dbAwayTeamSeasonId;
//					int dbPlayerSeasonId = dbPlayerSeasonDict[apiLineupPlayerId];

//					if (!dbPlayerBoxscores.TryGetValue(apiLineupPlayerId, out PlayerBoxscore dbPlayerBoxscore))
//					{
//						dbPlayerBoxscore = new PlayerBoxscore
//						{
//							FixtureId = dbFixtureId,
//							PlayerSeasonId = dbPlayerSeasonId,
//							TeamSeasonId = dbTeamSeasonId
//						};
//						dbContext.PlayerBoxscores.Add(dbPlayerBoxscore);
//						dbPlayerBoxscores.Add(apiLineupPlayerId, dbPlayerBoxscore);
//						hasUpdate = true;
//					}

//					bool populateHasUpdate = PopulatePlayerBoxscore(isStarterFromLineup, apiLineupPlayer, apiPlayerBoxscore, apiSubstitutionEvents, ref dbPlayerBoxscore);
//					hasUpdate = hasUpdate || populateHasUpdate;
//				}
//			}
//			#endregion PLAYER BOXSCORE

//			if (hasUpdate)
//			{
//				dbContext.SaveChanges();
//			}
//		}

//		private int GetApiLineupPlayerPlayerId(int apiLeagueId, int apiFixtureId, int? apiLineupPlayerId, string playerName, int? jerseyNumber)
//		{
//			if (apiLeagueId == 979)
//			{
//				if (apiLineupPlayerId.Value == 6811 && playerName == "Gabriel Popovic")
//				{
//					return 153638;
//				}
//				if (apiLineupPlayerId.Value == 6811 && playerName == "Mohamed Toure")
//				{
//					return 198352;
//				}
//				if (apiFixtureId == 245406 && playerName == "So Nishikawa")
//				{
//					return 207232;
//				}
//			}

//			if (apiLeagueId == 282)
//			{
//				if (apiFixtureId == 85473 && !apiLineupPlayerId.HasValue && playerName == "Felipe Cordeiro De Araujo")
//				{
//					return 54225;
//				}
//				if (apiFixtureId == 85473 && !apiLineupPlayerId.HasValue && playerName == "Márcio")
//				{
//					return 54811;
//				}
//				if (apiFixtureId == 85473 && !apiLineupPlayerId.HasValue && playerName == "Felipe Manoel")
//				{
//					return 54816;
//				}
//				if (apiFixtureId == 74799 && !apiLineupPlayerId.HasValue && playerName == "Klenisson")
//				{
//					return 54436;
//				}
//				if (apiFixtureId == 74799 && apiLineupPlayerId == 54436 && playerName == "Da Silva" && jerseyNumber == 3)
//				{
//					return 54636;
//				}
//			}

//			if (!apiLineupPlayerId.HasValue)
//			{
//				var a = 1;
//				throw new ArgumentNullException("apiLineupPlayerId");
//			}

//			return apiLineupPlayerId.Value;
//		}

//		private int? GetApiPlayerBoxscorePlayerId(int apiLeagueId, int apiFixtureId, int? apiPlayerBoxscoreId,
//			string playerName, bool? isSubstitute, int? jerseyNumber, int apiTeamId,
//			List<Feeds.FixtureFeed.ApiLineupPlayer> apiLineupPlayers)
//		{
//			if (apiLeagueId == 7)
//			{
//				if (!apiPlayerBoxscoreId.HasValue && apiFixtureId == 2516 && playerName == "Paulinho Ferreira")
//				{
//					return 9682;
//				}
//				if (!apiPlayerBoxscoreId.HasValue && apiFixtureId == 2516 && playerName == "Simon Helg")
//				{
//					return 9824;
//				}
//			}

//			if (apiLeagueId == 135)
//			{
//				if (apiFixtureId == 40491 && playerName == "Nail Umiarov")
//				{
//					return 1791;
//				}
//				if (apiFixtureId == 40491 && playerName == "Maksim Glushenkov")
//				{
//					return 1781;
//				}
//			}

//			if (apiLeagueId == 979)
//			{
//				if (apiFixtureId == 245367 && playerName == "Oskar Dillon")
//				{
//					return 193195;
//				}
//				if (apiFixtureId == 245382 && playerName == "Mohamed Toure")
//				{
//					return 198352;
//				}
//				if (apiFixtureId == 245406 && playerName == "So Nishikawa")
//				{
//					return 207232;
//				}
//				if (apiPlayerBoxscoreId.HasValue && apiPlayerBoxscoreId.Value == 6811 && playerName == "Gabriel Popovic")
//				{
//					return 153638;
//				}
//			}

//			if (apiLeagueId == 282)
//			{
//				if (apiFixtureId == 74799 && apiPlayerBoxscoreId == 54436)
//				{
//					if (playerName == "Da Silva" && jerseyNumber == 3)
//					{
//						return 54636;
//					}
//					if (playerName == "Klenisson" && jerseyNumber == 7)
//					{
//						return 54436;
//					}
//				}
//			}

//			if (apiLeagueId == 357)
//			{
//				if (apiFixtureId == 94650 && playerName == "Gabriel Pec")
//				{
//					return 143367;
//				}
//				if (apiFixtureId == 94641 && playerName == "Jonathan Luiz Moreira Rosa Junior")
//				{
//					return 140452;
//				}
//				if (apiFixtureId == 94612 && playerName == "Jean Carlos")
//				{
//					return 55148;
//				}
//				if (apiFixtureId == 94552 && playerName == "Darlan")
//				{
//					return 10499;
//				}
//				if (apiFixtureId == 94845 && playerName == "Diego Costa")
//				{
//					return 143363;
//				}
//				if ((apiFixtureId == 94836 || apiFixtureId == 94767 || apiFixtureId == 94814 || apiFixtureId == 94802 || apiFixtureId == 94713 || apiFixtureId == 94701) && playerName == "Igor Fernandes")
//				{
//					return 10519;
//				}
//				if ((apiFixtureId == 94814 || apiFixtureId == 94802 || apiFixtureId == 94713 || apiFixtureId == 94701) && playerName == "Igor" && jerseyNumber == 11)
//				{
//					return 54065;
//				}
//				if (apiFixtureId == 94817 && playerName == "Gabriel Veron")
//				{
//					return 180235;
//				}
//				if (apiFixtureId == 94521 && playerName == "Rick")
//				{
//					return 119666;
//				}
//				if (apiFixtureId == 94648 && playerName == "Gabriel Silva")
//				{
//					return 143364;
//				}
//				if (apiFixtureId == 94648 && playerName == "Fabinho")
//				{
//					return 143365;
//				}
//				if (apiFixtureId == 94647 && playerName == "Marcão")
//				{
//					return 10334;
//				}
//				if (apiFixtureId == 94684 && playerName == "Bruno Gomes")
//				{
//					return 143368;
//				}
//				if (apiFixtureId == 94660 && apiPlayerBoxscoreId == 31015 && playerName == "Bruno Alves de Souza")
//				{
//					return 53956;
//				}
//				if (apiFixtureId == 94660 && apiPlayerBoxscoreId == 31015 && playerName == "Bruno Alves")
//				{
//					return 9945;
//				}
//				if (apiFixtureId == 94607 && playerName == "Yago Fernando" && jerseyNumber == 33)
//				{
//					return 10337;
//				}
//				if (apiPlayerBoxscoreId.HasValue && apiPlayerBoxscoreId.Value == 10337 && jerseyNumber == 32)
//				{
//					return 10350;
//				}
//			}

//			if (apiLeagueId == 358)
//			{
//				if (apiPlayerBoxscoreId.HasValue && apiPlayerBoxscoreId.Value == 9756 && playerName == "Ramon" && (jerseyNumber == 40 || jerseyNumber == 3 || jerseyNumber == 4))
//				{
//					return 9525;
//				}
//			}

//			if (!apiPlayerBoxscoreId.HasValue)
//			{
//				var apiLineupPlayer = apiLineupPlayers.SingleOrDefault(x => x.TeamId == apiTeamId && jerseyNumber.HasValue && x.Number == jerseyNumber);
//				if (apiLineupPlayer != null)
//				{
//					return apiLineupPlayer.PlayerId;
//				}
//				return apiPlayerBoxscoreId;
//			}

//			if (apiLeagueId == 150)
//			{
//				if (apiFixtureId == 44613 && apiPlayerBoxscoreId.Value == 80343 && playerName.Contains("Willian"))
//				{
//					return 114630;
//				}
//				if (apiFixtureId == 44611 && apiPlayerBoxscoreId.Value == 10023 && playerName.Contains("Talisson"))
//				{
//					return 53978;
//				}
//				if (apiFixtureId == 44565 && apiPlayerBoxscoreId.Value == 80290 && playerName.Contains("Duarte"))
//				{
//					return 10351;
//				}
//				if (apiFixtureId == 44569 && apiPlayerBoxscoreId.Value == 12764 && playerName.Contains("Rafa"))
//				{
//					return 106494;
//				}
//			}

//			if (apiLeagueId == 6)
//			{
//				if ((apiFixtureId == 37403 || apiFixtureId == 37359) && apiPlayerBoxscoreId.Value == 9647 && isSubstitute.Value)
//				{
//					return 9857;
//				}
//				if ((apiFixtureId == 37339 || apiFixtureId == 37330) && apiPlayerBoxscoreId.Value == 77825 && playerName == "Raphael Alemão")
//				{
//					return 9446;
//				}
//			}

//			if (apiLeagueId == 82)
//			{
//				if (apiPlayerBoxscoreId.Value == 12689 && playerName == "Yi Zhang")
//				{
//					return 12697;
//				}
//				if (apiPlayerBoxscoreId.Value == 80853 && playerName == "Yaopeng Wang")
//				{
//					return 12988;
//				}
//				if (apiPlayerBoxscoreId.Value == 12958 && playerName == "Shuai Li")
//				{
//					return 12951;
//				}
//				if (apiPlayerBoxscoreId.Value == 80861 && playerName == "Lin Wang")
//				{
//					return 157171;
//				}
//				if (apiPlayerBoxscoreId.Value == 80861 && playerName == "Yun Wang")
//				{
//					return 157172;
//				}
//				if (apiPlayerBoxscoreId.Value == 80861 && playerName == "Wei Wang")
//				{
//					return 12970;
//				}
//				if (apiPlayerBoxscoreId.Value == 12757 && playerName == "Yang Liu")
//				{
//					return 12743;
//				}
//				if (apiPlayerBoxscoreId.Value == 12996 && playerName == "Guowen Sun")
//				{
//					return 12997;
//				}
//				if (apiPlayerBoxscoreId.Value == 80857 && playerName == "Xiaobing Zhang")
//				{
//					return 80870;
//				}
//				if (apiPlayerBoxscoreId.Value == 80872 && playerName == "Wei Long")
//				{
//					return 12907;
//				}
//				if (apiPlayerBoxscoreId.Value == 12958 && playerName == "Jianbin Li")
//				{
//					return 12985;
//				}
//				if (apiPlayerBoxscoreId.Value == 12958 && playerName == "Peng Li")
//				{
//					return 12957;
//				}
//				if (apiPlayerBoxscoreId.Value == 12828 && jerseyNumber.Value == 32)
//				{
//					return 12824;
//				}
//				if (apiPlayerBoxscoreId.Value == 80846 && playerName == "Zhizhao Chen")
//				{
//					return 12932;
//				}
//				if (apiPlayerBoxscoreId.Value == 12757 && playerName == "Junshuai Liu")
//				{
//					return 12742;
//				}
//				if (apiPlayerBoxscoreId.Value == 12688 && playerName == "Hai Yu")
//				{
//					return 12687;
//				}
//				if (apiPlayerBoxscoreId.Value == 78975 && playerName == "Fan Yang")
//				{
//					return 12843;
//				}
//				if (apiPlayerBoxscoreId.Value == 78975 && playerName == "Wanshun Yang")
//				{
//					return 12851;
//				}
//				if (apiPlayerBoxscoreId.Value == 80853 && playerName == "Jinxian Wang")
//				{
//					return 12998;
//				}
//				if (apiPlayerBoxscoreId.Value == 80853 && playerName == "Yaopeng Wang")
//				{
//					return 12988;
//				}
//				if (apiPlayerBoxscoreId.Value == 80831 && playerName == "Chenglin Zhang")
//				{
//					return 106678;
//				}
//				if (apiPlayerBoxscoreId.Value == 80831 && playerName == "Wenzhao Zhang")
//				{
//					return 13121;
//				}
//				if (apiPlayerBoxscoreId.Value == 13104 && playerName == "Hao Luo")
//				{
//					return 12808;
//				}
//				if (apiPlayerBoxscoreId.Value == 12944 && playerName == "Gong Zhang")
//				{
//					return 12945;
//				}
//				if (apiPlayerBoxscoreId.Value == 12944 && playerName == "Gong Zhang")
//				{
//					return 12945;
//				}
//				if (apiPlayerBoxscoreId.Value == 12944 && playerName == "Jiaqi Zhang")
//				{
//					return 78809;
//				}
//				if (apiPlayerBoxscoreId.Value == 12944 && playerName == "Jiajie Zhang")
//				{
//					return 78791;
//				}
//				if (apiPlayerBoxscoreId.Value == 80856 && playerName == "Ang Li")
//				{
//					return 12773;
//				}
//				if (apiPlayerBoxscoreId.Value == 80856 && playerName == "Haitao Li")
//				{
//					return 12770;
//				}
//				if (apiPlayerBoxscoreId.Value == 80865 && playerName == "Sipeng Zhang")
//				{
//					return 12440;
//				}
//			}

//			if (apiLeagueId == 297)
//			{
//				if (apiPlayerBoxscoreId.Value == 35502 && string.Equals(playerName, "Claudio Gonzalez", StringComparison.InvariantCultureIgnoreCase))
//				{
//					return 36383;
//				}
//			}

//			return apiPlayerBoxscoreId;
//		}

//		private Dictionary<int, int> GetDbPlayerSeasonDict(SoccerDataContext dbContext, List<ApiPlayerBase> apiPlayerBases, int dbCompetitionSeasonId)
//		{
//			if (apiPlayerBases == null || apiPlayerBases.Count == 0)
//			{
//				return null;
//			}

//			var apiPlayerIds = apiPlayerBases.Select(x => x.PlayerId).ToList();
//			return dbContext.PlayerSeasons
//							.Include(x => x.Player)
//							.Where(x => x.CompetitionSeasonId == dbCompetitionSeasonId && apiPlayerIds.Contains(x.Player.ApiFootballId))
//							.ToDictionary(x => x.Player.ApiFootballId, y => y.PlayerSeasonId);
//		}

//		private List<ApiPlayerBase> GetApiPlayerBases(Feeds.FixtureFeed.ApiFixture feedFixture)
//		{
//			var apiAllLineupPlayers = feedFixture.AllLineupPlayers;
//			var apiPlayerBasesFromPlayers = feedFixture.PlayerBoxscores?
//														.Where(x => x.PlayerId.HasValue)
//														.Select(x => new ApiPlayerBase(GetApiPlayerBoxscorePlayerId(feedFixture.LeagueId, this.ApiFootballFixtureId, x.PlayerId, x.PlayerName, x.IsSubstitute, x.Number, x.TeamId, apiAllLineupPlayers).Value, x.PlayerName))
//														.ToList();
//			var apiPlayerBasesFromEvents = feedFixture.Events?
//														.SelectMany(x => new[] { new { PlayerId = x.PlayerId, PlayerName = x.PlayerName }, new { PlayerId = x.SecondaryPlayerId, PlayerName = x.SecondaryPlayerName } })
//														.Where(x => x.PlayerId.HasValue && !string.IsNullOrEmpty(x.PlayerName))
//														.Select(x => new ApiPlayerBase(x.PlayerId.Value, x.PlayerName))
//														.ToList();

//			var apiPlayerBasesFromLineups = apiAllLineupPlayers?.Select(x => new ApiPlayerBase(GetApiLineupPlayerPlayerId(feedFixture.LeagueId, this.ApiFootballFixtureId, x.PlayerId, x.PlayerName, x.Number), x.PlayerName)).ToList();

//			var apiPlayerBases = apiPlayerBasesFromPlayers ?? apiPlayerBasesFromLineups ?? apiPlayerBasesFromEvents;
//			if (apiPlayerBasesFromPlayers != null)
//			{
//				if (apiPlayerBasesFromLineups != null)
//				{
//					apiPlayerBases.AddRange(apiPlayerBasesFromLineups);
//				}
//				if (apiPlayerBasesFromEvents != null)
//				{
//					apiPlayerBases.AddRange(apiPlayerBasesFromEvents);
//				}
//			}
//			else if (apiPlayerBasesFromLineups != null)
//			{
//				if (apiPlayerBasesFromEvents != null)
//				{
//					apiPlayerBases.AddRange(apiPlayerBasesFromEvents);
//				}
//			}
//			if (apiPlayerBases != null)
//			{
//				apiPlayerBases = apiPlayerBases.Distinct(new ApiPlayerBaseComparer()).ToList();
//			}
//			return apiPlayerBases;
//		}

//		// TEAM IS REQUIRED (API FIXTURE ID 131874)
//		private (int, int, int, int, string, string) GetFixtureEventKey(FixtureEvent fixtureEvent)
//		{
//			return (fixtureEvent.EventTime,
//					fixtureEvent.EventTimePlus ?? NullIntDictKey,
//					fixtureEvent.PlayerSeasonId ?? NullIntDictKey,
//					fixtureEvent.TeamSeasonId,
//					fixtureEvent.EventType,
//					fixtureEvent.EventDetail);
//		}

//		private (int, int, int, int, string, string) GetFixtureEventKey(int gameTime, int? gameTimePlus, int? playerSeasonId, int teamSeasonId, string eventType, string eventDetail)
//		{
//			return (gameTime,
//					gameTimePlus ?? NullIntDictKey,
//					playerSeasonId ?? NullIntDictKey,
//					teamSeasonId,
//					eventType,
//					eventDetail);
//		}

//		#region TEAM BOXSCORE HELPERS

//		/// <summary>
//		/// 
//		/// </summary>
//		/// <param name="apiStatsDict">Team statistics for fixture from API</param>
//		/// <param name="teamId">Team which accumulated desired stats</param>
//		/// <param name="oppTeamId">Opponent of team which accumulated desired stats</param>
//		/// <param name="isHome">Indicator for if the desired team is the home team</param>
//		/// <param name="statGetFunc">Function to return the desired stat from a Statistic object. Used to choose home or away value.</param>
//		/// <param name="dbTeamBoxscore">Object to populate</param>
//		/// <returns>true if an update has been made; else false</returns>
//		private bool PopulateTeamBoxscore(Dictionary<string, Feeds.FixtureFeed.ApiTeamStatistic> apiStatsDict,
//			Func<Feeds.FixtureFeed.ApiTeamStatistic, string> statGetFunc,
//			ref TeamBoxscore dbTeamBoxscore)
//		{
//			bool hasUpdate = false;

//			int? statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.ShotsOnGoal, apiStatsDict, statGetFunc);
//			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.ShotsOnGoal)
//			{
//				dbTeamBoxscore.ShotsOnGoal = statVal.Value;
//				hasUpdate = true;
//			}

//			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.ShotsOffGoal, apiStatsDict, statGetFunc);
//			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.ShotsOffGoal)
//			{
//				dbTeamBoxscore.ShotsOffGoal = statVal.Value;
//				hasUpdate = true;
//			}

//			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.TotalShots, apiStatsDict, statGetFunc);
//			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.ShotsTotal)
//			{
//				dbTeamBoxscore.ShotsTotal = statVal.Value;
//				hasUpdate = true;
//			}

//			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.BlockedShots, apiStatsDict, statGetFunc);
//			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.ShotsBlocked)
//			{
//				dbTeamBoxscore.ShotsBlocked = statVal.Value;
//				hasUpdate = true;
//			}

//			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.ShotsInsideBox, apiStatsDict, statGetFunc);
//			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.ShotsInsideBox)
//			{
//				dbTeamBoxscore.ShotsInsideBox = statVal.Value;
//				hasUpdate = true;
//			}

//			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.ShotsOutsideBox, apiStatsDict, statGetFunc);
//			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.ShotsOutsideBox)
//			{
//				dbTeamBoxscore.ShotsOutsideBox = statVal.Value;
//				hasUpdate = true;
//			}

//			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.FoulsCommitted, apiStatsDict, statGetFunc);
//			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.FoulsCommitted)
//			{
//				dbTeamBoxscore.FoulsCommitted = statVal.Value;
//				hasUpdate = true;
//			}

//			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.CornerKicks, apiStatsDict, statGetFunc);
//			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.CornerKicks)
//			{
//				dbTeamBoxscore.CornerKicks = statVal.Value;
//				hasUpdate = true;
//			}

//			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.Offsides, apiStatsDict, statGetFunc);
//			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.OffsidesCommitted)
//			{
//				dbTeamBoxscore.OffsidesCommitted = statVal.Value;
//				hasUpdate = true;
//			}

//			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.BallPossession, apiStatsDict, statGetFunc);
//			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.PossessionPct)
//			{
//				dbTeamBoxscore.PossessionPct = statVal.Value;
//				hasUpdate = true;
//			}

//			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.YellowCards, apiStatsDict, statGetFunc);
//			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.YellowCards)
//			{
//				dbTeamBoxscore.YellowCards = statVal.Value;
//				hasUpdate = true;
//			}

//			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.RedCards, apiStatsDict, statGetFunc);
//			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.RedCards)
//			{
//				dbTeamBoxscore.RedCards = statVal.Value;
//				hasUpdate = true;
//			}

//			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.GoalkeeperSaves, apiStatsDict, statGetFunc);
//			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.GoalieSaves)
//			{
//				dbTeamBoxscore.GoalieSaves = statVal.Value;
//				hasUpdate = true;
//			}

//			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.TotalPasses, apiStatsDict, statGetFunc);
//			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.PassesTotal)
//			{
//				dbTeamBoxscore.PassesTotal = statVal.Value;
//				hasUpdate = true;
//			}

//			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.AccuratePasses, apiStatsDict, statGetFunc);
//			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.PassesAccurate)
//			{
//				dbTeamBoxscore.PassesAccurate = statVal.Value;
//				hasUpdate = true;
//			}

//			statVal = GetStatValueByKey(Feeds.FixtureFeed.TeamStatKeys.PassCompPct, apiStatsDict, statGetFunc);
//			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.PassAccuracyPct)
//			{
//				dbTeamBoxscore.PassAccuracyPct = statVal.Value;
//				hasUpdate = true;
//			}

//			return hasUpdate;
//		}

//		private int? GetStatValueByKey(string key,
//			Dictionary<string, Feeds.FixtureFeed.ApiTeamStatistic> apiStatsDict,
//			Func<Feeds.FixtureFeed.ApiTeamStatistic, string> statGetFunc)
//		{
//			if (!apiStatsDict.TryGetValue(key, out Feeds.FixtureFeed.ApiTeamStatistic apiTeamStat))
//			{
//				return null;
//			}

//			var strValue = statGetFunc(apiTeamStat);
//			if (string.IsNullOrEmpty(strValue))
//			{
//				return null;
//			}

//			strValue = strValue.Replace("%", string.Empty);

//			if (int.TryParse(strValue, out int result))
//			{
//				return result;
//			}
//			return null;
//		}
//		#endregion TEAM BOXSCORE HELPERS

//		#region PLAYER BOXSCORE HELPERS
//		/// <summary>
//		///		Populates a provided PlayerBoxscore object.
//		///		Assumes there will always be a lineup if there is a Players node
//		/// </summary>
//		/// <param name="isStarterFromLineup">
//		///		true if player is listed under starters in api Lineups node. 
//		///		false if player is listed under substitutes in api Lineups node. 
//		/// </param>
//		/// <param name="apiLineupPlayer">player object from api Lineups node</param>
//		/// <param name="apiPlayerGameData">player object from api Players node</param>
//		/// <param name="dbPlayerBoxscore">
//		///		PlayerBoxscore object to populate.
//		///		Assumes FixtureId, PlayerSeasonId, and TeamSeasonId are already set</param>
//		/// <returns>true if an update has been made; else false</returns>
//		private bool PopulatePlayerBoxscore(bool isStarterFromLineup,
//			Feeds.FixtureFeed.ApiLineupPlayer apiLineupPlayer,
//			Feeds.FixtureFeed.ApiPlayerBoxscore apiPlayerGameData,
//			List<Feeds.FixtureFeed.ApiFixtureEvent> apiSubstitutionEvents,
//			ref PlayerBoxscore dbPlayerBoxscore)
//		{
//			bool hasUpdate = false;

//			string position = !string.IsNullOrEmpty(apiLineupPlayer?.Position) ? apiLineupPlayer.Position : apiPlayerGameData?.Position;
//			int? jerseyNumber = apiLineupPlayer.Number.HasValue ? apiLineupPlayer.Number : apiPlayerGameData?.Number;

//			bool? played = isStarterFromLineup ? true : (bool?)null;
//			if (!played.HasValue || !played.Value)
//			{
//				if (apiPlayerGameData != null)
//				{
//					played = apiPlayerGameData.MinutesPlayed > 0;
//				}
//				else if (apiSubstitutionEvents != null && apiSubstitutionEvents.Count > 0)
//				{
//					played = apiSubstitutionEvents.Any(x => x.SecondaryPlayerId == apiLineupPlayer.PlayerId);
//				}
//			}

//			// AVOID OVERWRITING MANUAL CHANGES FOR NULLABLE COLUMNS
//			if (dbPlayerBoxscore.IsStarter != isStarterFromLineup
//				|| dbPlayerBoxscore.IsBench != !isStarterFromLineup
//				|| (played.HasValue && (!dbPlayerBoxscore.Played.HasValue || dbPlayerBoxscore.Played != played))
//				|| (!string.IsNullOrEmpty(position) && (string.IsNullOrEmpty(dbPlayerBoxscore.Position) || dbPlayerBoxscore.Position != position))
//				|| (jerseyNumber.HasValue && (!dbPlayerBoxscore.JerseyNumber.HasValue || dbPlayerBoxscore.JerseyNumber != jerseyNumber)))
//			{
//				dbPlayerBoxscore.Position = position;
//				dbPlayerBoxscore.JerseyNumber = jerseyNumber;
//				dbPlayerBoxscore.Played = played;
//				dbPlayerBoxscore.IsStarter = isStarterFromLineup;
//				dbPlayerBoxscore.IsBench = !isStarterFromLineup;
//				hasUpdate = true;
//			}

//			if (apiPlayerGameData != null)
//			{
//				if (!dbPlayerBoxscore.ApiFootballLastUpdate.HasValue || dbPlayerBoxscore.ApiFootballLastUpdate.Value != apiPlayerGameData.UpdateAt)
//				{
//					dbPlayerBoxscore.ApiFootballLastUpdate = apiPlayerGameData.UpdateAt;
//					dbPlayerBoxscore.Assists = apiPlayerGameData.Goals?.Assists;
//					dbPlayerBoxscore.Blocks = apiPlayerGameData.Tackles?.Blocks;
//					dbPlayerBoxscore.DribblesAttempted = apiPlayerGameData.Dribbles?.Attempts;
//					dbPlayerBoxscore.DribblesPastDef = apiPlayerGameData.Dribbles?.Past;
//					dbPlayerBoxscore.DribblesSuccessful = apiPlayerGameData.Dribbles?.Success;
//					dbPlayerBoxscore.DuelsTotal = apiPlayerGameData.Duels?.Total;
//					dbPlayerBoxscore.DuelsWon = apiPlayerGameData.Duels?.Won;
//					dbPlayerBoxscore.FoulsCommitted = apiPlayerGameData.Fouls?.Committed;
//					dbPlayerBoxscore.FoulsSuffered = apiPlayerGameData.Fouls?.Drawn;
//					dbPlayerBoxscore.Goals = apiPlayerGameData.Goals?.Total;
//					dbPlayerBoxscore.GoalsConceded = apiPlayerGameData.Goals?.Conceded;
//					dbPlayerBoxscore.Interceptions = apiPlayerGameData.Tackles?.Interceptions;
//					dbPlayerBoxscore.IsCaptain = apiPlayerGameData.IsCaptain;
//					dbPlayerBoxscore.KeyPasses = apiPlayerGameData.Passes?.Key;
//					dbPlayerBoxscore.MinutesPlayed = apiPlayerGameData.MinutesPlayed;
//					dbPlayerBoxscore.PassAccuracy = apiPlayerGameData.Passes?.Accuracy;
//					dbPlayerBoxscore.PassAttempts = apiPlayerGameData.Passes?.Total;
//					dbPlayerBoxscore.PenaltiesCommitted = apiPlayerGameData.Penalty?.Commited;
//					dbPlayerBoxscore.PenaltiesMissed = apiPlayerGameData.Penalty?.Missed;
//					dbPlayerBoxscore.PenaltiesSaved = apiPlayerGameData.Penalty?.Saved;
//					dbPlayerBoxscore.PenaltiesScored = apiPlayerGameData.Penalty?.Success;
//					dbPlayerBoxscore.PenaltiesWon = apiPlayerGameData.Penalty?.Won;
//					dbPlayerBoxscore.Rating = apiPlayerGameData.Rating;
//					dbPlayerBoxscore.RedCards = apiPlayerGameData.Cards?.Red;
//					dbPlayerBoxscore.ShotsOnGoal = apiPlayerGameData.Shots?.On;
//					dbPlayerBoxscore.ShotsTaken = apiPlayerGameData.Shots?.Total;
//					dbPlayerBoxscore.Tackles = apiPlayerGameData.Tackles?.Total;
//					dbPlayerBoxscore.YellowCards = apiPlayerGameData.Cards?.Yellow;
//					hasUpdate = true;
//				}
//			}

//			return hasUpdate;
//		}
//		#endregion PLAYER BOXSCORE HELPERS

//		#region HELPER CLASSES
//		private class ApiPlayerBase
//		{
//			public int PlayerId { get; set; }
//			public string PlayerName { get; set; }

//			public ApiPlayerBase(int playerId, string playerName)
//			{
//				this.PlayerId = playerId;
//				this.PlayerName = playerName;
//			}
//		}

//		private class ApiPlayerBaseComparer : IEqualityComparer<ApiPlayerBase>
//		{
//			public bool Equals([AllowNull] ApiPlayerBase x, [AllowNull] ApiPlayerBase y)
//			{
//				return x.PlayerId == y.PlayerId;
//			}

//			public int GetHashCode([DisallowNull] ApiPlayerBase obj)
//			{
//				return base.GetHashCode();
//			}
//		}
//		#endregion HELPER CLASSES
//	}
//}
