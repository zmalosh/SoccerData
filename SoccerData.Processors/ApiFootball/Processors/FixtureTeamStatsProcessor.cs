using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SoccerData.Model;

namespace SoccerData.Processors.ApiFootball.Processors
{
	public class FixtureTeamStatsProcessor : IProcessor
	{
		private readonly int ApiFootballFixtureId;
		private readonly JsonUtility JsonUtility;

		public FixtureTeamStatsProcessor(int apiFootballFixtureId)
		{
			this.ApiFootballFixtureId = apiFootballFixtureId;
			this.JsonUtility = new JsonUtility(120 * 24 * 60 * 60, sourceType: JsonUtility.JsonSourceType.ApiFootball); // 230K+ FIXTURES.... SAVE FINISHED GAMES FOR A LONG TIME (120 DAYS?) TO AVOID QUOTA ISSUES
		}

		public void Run(SoccerDataContext dbContext)
		{
			var dbFixture = dbContext.Fixtures.Single(x => x.ApiFootballId == this.ApiFootballFixtureId);
			var isFixtureFinal = string.Equals("Match Finished", dbFixture.Status, StringComparison.CurrentCultureIgnoreCase);

			if (!dbFixture.AwayTeamSeasonId.HasValue
				|| !dbFixture.AwayTeamSeasonId.HasValue
				|| !isFixtureFinal)
			{
				return;
			}

			var url = Feeds.FixtureTeamStatsFeed.GetFeedUrlByFixtureId(this.ApiFootballFixtureId);
			var rawJson = JsonUtility.GetRawJsonFromUrl(url);

			if (rawJson.Contains("\"results\":0,"))
			{
				if (!dbFixture.HasTeamBoxscores.HasValue)
				{
					dbFixture.HasTeamBoxscores = false;
					dbContext.SaveChanges();
				}
				return;
			}

			var feed = Feeds.FixtureTeamStatsFeed.FromJson(rawJson);

			bool hasUpdate = false;

			int dbFixtureId = dbFixture.FixtureId;
			int dbHomeTeamSeasonId = dbFixture.HomeTeamSeasonId.Value;
			int dbAwayTeamSeasonId = dbFixture.AwayTeamSeasonId.Value;

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

			Dictionary<string, Feeds.FixtureTeamStatsFeed.Statistic> apiStatsDict = feed.ResultWrapper.Statistics;

			bool homeUpdated = false;
			bool awayUpdated = false;

			homeUpdated = PopulateTeamBoxscore(apiStatsDict, x => x.Home, ref dbHomeBoxscore);
			awayUpdated = PopulateTeamBoxscore(apiStatsDict, x => x.Away, ref dbAwayBoxscore);
			hasUpdate = hasUpdate || homeUpdated || awayUpdated;

			if (hasUpdate)
			{
				dbFixture.HasTeamBoxscores = true;
				dbContext.SaveChanges();
			}
		}

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
		private bool PopulateTeamBoxscore(Dictionary<string, Feeds.FixtureTeamStatsFeed.Statistic> apiStatsDict,
			Func<Feeds.FixtureTeamStatsFeed.Statistic, string> statGetFunc,
			ref TeamBoxscore dbTeamBoxscore)
		{
			bool hasUpdate = false;

			int? statVal = GetStatValueByKey(Feeds.FixtureTeamStatsFeed.StatKeys.ShotsOnGoal, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.ShotsOnGoal)
			{
				dbTeamBoxscore.ShotsOnGoal = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureTeamStatsFeed.StatKeys.ShotsOffGoal, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.ShotsOffGoal)
			{
				dbTeamBoxscore.ShotsOffGoal = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureTeamStatsFeed.StatKeys.TotalShots, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.ShotsTotal)
			{
				dbTeamBoxscore.ShotsTotal = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureTeamStatsFeed.StatKeys.BlockedShots, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.ShotsBlocked)
			{
				dbTeamBoxscore.ShotsBlocked = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureTeamStatsFeed.StatKeys.ShotsInsideBox, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.ShotsInsideBox)
			{
				dbTeamBoxscore.ShotsInsideBox = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureTeamStatsFeed.StatKeys.ShotsOutsideBox, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.ShotsOutsideBox)
			{
				dbTeamBoxscore.ShotsOutsideBox = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureTeamStatsFeed.StatKeys.FoulsCommitted, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.FoulsCommitted)
			{
				dbTeamBoxscore.FoulsCommitted = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureTeamStatsFeed.StatKeys.CornerKicks, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.CornerKicks)
			{
				dbTeamBoxscore.CornerKicks = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureTeamStatsFeed.StatKeys.Offsides, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.OffsidesCommitted)
			{
				dbTeamBoxscore.OffsidesCommitted = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureTeamStatsFeed.StatKeys.BallPossession, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.PossessionPct)
			{
				dbTeamBoxscore.PossessionPct = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureTeamStatsFeed.StatKeys.YellowCards, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.YellowCards)
			{
				dbTeamBoxscore.YellowCards = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureTeamStatsFeed.StatKeys.RedCards, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.RedCards)
			{
				dbTeamBoxscore.RedCards = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureTeamStatsFeed.StatKeys.GoalkeeperSaves, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.GoalieSaves)
			{
				dbTeamBoxscore.GoalieSaves = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureTeamStatsFeed.StatKeys.TotalPasses, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.PassesTotal)
			{
				dbTeamBoxscore.PassesTotal = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureTeamStatsFeed.StatKeys.AccuratePasses, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.PassesAccurate)
			{
				dbTeamBoxscore.PassesAccurate = statVal.Value;
				hasUpdate = true;
			}

			statVal = GetStatValueByKey(Feeds.FixtureTeamStatsFeed.StatKeys.PassCompPct, apiStatsDict, statGetFunc);
			if (statVal.HasValue && statVal.Value != dbTeamBoxscore.PassAccuracyPct)
			{
				dbTeamBoxscore.PassAccuracyPct = statVal.Value;
				hasUpdate = true;
			}

			return hasUpdate;
		}

		private int? GetStatValueByKey(string key,
			Dictionary<string, Feeds.FixtureTeamStatsFeed.Statistic> apiStatsDict,
			Func<Feeds.FixtureTeamStatsFeed.Statistic, string> statGetFunc)
		{
			if (!apiStatsDict.TryGetValue(key, out Feeds.FixtureTeamStatsFeed.Statistic apiTeamStat))
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
	}
}
