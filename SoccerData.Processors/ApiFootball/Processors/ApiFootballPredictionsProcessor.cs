using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SoccerData.Model;

namespace SoccerData.Processors.ApiFootball.Processors
{
	public class ApiFootballPredictionsProcessor : IProcessor
	{
		private readonly int ApiFootballFixtureId;
		private readonly JsonUtility JsonUtility;

		public ApiFootballPredictionsProcessor(int apiFootballFixtureId)
		{
			this.ApiFootballFixtureId = apiFootballFixtureId;
			this.JsonUtility = new JsonUtility(120 * 24 * 60 * 60, sourceType: JsonUtility.JsonSourceType.ApiFootball); // 230K+ FIXTURES.... SAVE FINISHED GAMES FOR A LONG TIME (120 DAYS?) TO AVOID QUOTA ISSUES
		}

		public void Run(SoccerDataContext dbContext)
		{
			var dbFixture = dbContext.Fixtures.SingleOrDefault(x => x.ApiFootballId == this.ApiFootballFixtureId);

			if (dbFixture == null) { return; }

			var url = Feeds.PredictionsFeed.GetFeedUrlByFixtureId(this.ApiFootballFixtureId);
			var rawJson = JsonUtility.GetRawJsonFromUrl(url);
			if (rawJson == null)
			{
				rawJson = JsonUtility.GetRawJsonFromUrl(url);
			}
			var feed = Feeds.PredictionsFeed.FromJson(rawJson);

			var feedPred = feed?.Result?.Predictions?.SingleOrDefault();
			if (feedPred == null)
			{
				return;
			}

			bool hasUpdate = false;
			var dbApiFootballPrediction = dbContext.ApiFootballPredictions.SingleOrDefault(x => x.FixtureId == dbFixture.FixtureId);
			if (dbApiFootballPrediction == null)
			{
				hasUpdate = true;
				dbApiFootballPrediction = new ApiFootballPrediction
				{
					FixtureId = dbFixture.FixtureId
				};
				dbContext.ApiFootballPredictions.Add(dbApiFootballPrediction);
			}
			else if (
				dbApiFootballPrediction.Advice != feedPred.Advice
				|| dbApiFootballPrediction.GameTotal != feedPred.GameTotal
				|| dbApiFootballPrediction.GoalsAwaySpread != feedPred.GoalsAway
				|| dbApiFootballPrediction.GoalsHomeSpread != feedPred.GoalsHome
				|| dbApiFootballPrediction.H2H_Home_AwayDraws != feedPred.Teams?.Home?.LastH2H?.Draws?.Away
				|| dbApiFootballPrediction.H2H_Home_AwayLosses != feedPred.Teams?.Home?.LastH2H?.Losses?.Away
				|| dbApiFootballPrediction.H2H_Home_AwayWins != feedPred.Teams?.Home?.LastH2H?.Wins?.Away
				|| dbApiFootballPrediction.H2H_Home_HomeDraws != feedPred.Teams?.Home?.LastH2H?.Draws?.Home
				|| dbApiFootballPrediction.H2H_Home_HomeLosses != feedPred.Teams?.Home?.LastH2H?.Losses?.Home
				|| dbApiFootballPrediction.H2H_Home_HomeWins != feedPred.Teams?.Home?.LastH2H?.Wins?.Home
				|| dbApiFootballPrediction.H2H_Home_TotalDraws != feedPred.Teams?.Home?.LastH2H?.Draws?.Total
				|| dbApiFootballPrediction.H2H_Home_TotalLosses != feedPred.Teams?.Home?.LastH2H?.Losses?.Total
				|| dbApiFootballPrediction.H2H_Home_TotalWins != feedPred.Teams?.Home?.LastH2H?.Wins?.Total
				|| dbApiFootballPrediction.MatchWinner != feedPred.MatchWinner
				|| dbApiFootballPrediction.Pred_Away_Attack != feedPred.Comparison?.Att?.AwayWinPct
				|| dbApiFootballPrediction.Pred_Away_Defense != feedPred.Comparison?.Def?.AwayWinPct
				|| dbApiFootballPrediction.Pred_Away_FishLaw != feedPred.Comparison?.FishLaw?.AwayWinPct
				|| dbApiFootballPrediction.Pred_Away_Forme != feedPred.Comparison?.Forme?.AwayWinPct
				|| dbApiFootballPrediction.Pred_Away_GoalsH2H != feedPred.Comparison?.GoalsH2H?.AwayWinPct
				|| dbApiFootballPrediction.Pred_Away_H2H != feedPred.Comparison?.H2H?.AwayWinPct
				|| dbApiFootballPrediction.Pred_Home_Attack != feedPred.Comparison?.Att?.HomeWinPct
				|| dbApiFootballPrediction.Pred_Home_Defense != feedPred.Comparison?.Def?.HomeWinPct
				|| dbApiFootballPrediction.Pred_Home_FishLaw != feedPred.Comparison?.FishLaw?.HomeWinPct
				|| dbApiFootballPrediction.Pred_Home_Forme != feedPred.Comparison?.Forme?.HomeWinPct
				|| dbApiFootballPrediction.Pred_Home_GoalsH2H != feedPred.Comparison?.GoalsH2H?.HomeWinPct
				|| dbApiFootballPrediction.Pred_Home_H2H != feedPred.Comparison?.H2H?.HomeWinPct
				|| dbApiFootballPrediction.RawLast5_Away_AllowedGoals != feedPred.Teams?.Away?.Last5_Matches?.GoalsAgainst
				|| dbApiFootballPrediction.RawLast5_Away_Attack != feedPred.Teams?.Away?.Last5_Matches?.Attack
				|| dbApiFootballPrediction.RawLast5_Away_Defense != feedPred.Teams?.Away?.Last5_Matches?.Defense
				|| dbApiFootballPrediction.RawLast5_Away_Forme != feedPred.Teams?.Away?.Last5_Matches?.Forme
				|| dbApiFootballPrediction.RawLast5_Away_Goals != feedPred.Teams?.Away?.Last5_Matches?.Goals
				|| dbApiFootballPrediction.RawLast5_Home_AllowedGoals != feedPred.Teams?.Home?.Last5_Matches?.GoalsAgainst
				|| dbApiFootballPrediction.RawLast5_Home_Attack != feedPred.Teams?.Home?.Last5_Matches?.Attack
				|| dbApiFootballPrediction.RawLast5_Home_Defense != feedPred.Teams?.Home?.Last5_Matches?.Defense
				|| dbApiFootballPrediction.RawLast5_Home_Forme != feedPred.Teams?.Home?.Last5_Matches?.Forme
				|| dbApiFootballPrediction.RawLast5_Home_Goals != feedPred.Teams?.Home?.Last5_Matches?.Goals
				|| dbApiFootballPrediction.Recent_Away_AwayAllowedGoals != feedPred.Teams?.Away?.AllLastMatches?.Goals?.GoalsAgainst?.Away
				|| dbApiFootballPrediction.Recent_Away_AwayGoals != feedPred.Teams?.Away?.AllLastMatches?.Goals?.GoalsFor?.Away
				|| dbApiFootballPrediction.Recent_Away_HomeAllowedGoals != feedPred.Teams?.Away?.AllLastMatches?.Goals?.GoalsAgainst?.Home
				|| dbApiFootballPrediction.Recent_Away_HomeGoals != feedPred.Teams?.Away?.AllLastMatches?.Goals?.GoalsFor?.Home
				|| dbApiFootballPrediction.Recent_Away_TotalAllowedGoals != feedPred.Teams?.Away?.AllLastMatches?.Goals?.GoalsAgainst?.Total
				|| dbApiFootballPrediction.Recent_Away_TotalGoals != feedPred.Teams?.Away?.AllLastMatches?.Goals?.GoalsFor?.Total
				|| dbApiFootballPrediction.Recent_Home_AwayAllowedGoals != feedPred.Teams?.Home?.AllLastMatches?.Goals?.GoalsAgainst?.Away
				|| dbApiFootballPrediction.Recent_Home_AwayGoals != feedPred.Teams?.Home?.AllLastMatches?.Goals?.GoalsFor?.Away
				|| dbApiFootballPrediction.Recent_Home_HomeAllowedGoals != feedPred.Teams?.Home?.AllLastMatches?.Goals?.GoalsAgainst?.Home
				|| dbApiFootballPrediction.Recent_Home_HomeGoals != feedPred.Teams?.Home?.AllLastMatches?.Goals?.GoalsFor?.Home
				|| dbApiFootballPrediction.Recent_Home_TotalAllowedGoals != feedPred.Teams?.Home?.AllLastMatches?.Goals?.GoalsAgainst?.Total
				|| dbApiFootballPrediction.Recent_Home_TotalGoals != feedPred.Teams?.Home?.AllLastMatches?.Goals?.GoalsFor?.Total
				|| dbApiFootballPrediction.Recent_Away_AwayWins != feedPred.Teams?.Away?.AllLastMatches?.Matches?.Wins?.Away
				|| dbApiFootballPrediction.Recent_Away_AwayDraws != feedPred.Teams?.Away?.AllLastMatches?.Matches?.Draws?.Away
				|| dbApiFootballPrediction.Recent_Away_AwayLosses != feedPred.Teams?.Away?.AllLastMatches?.Matches?.Losses?.Away
				|| dbApiFootballPrediction.Recent_Away_HomeWins != feedPred.Teams?.Away?.AllLastMatches?.Matches?.Wins?.Home
				|| dbApiFootballPrediction.Recent_Away_HomeDraws != feedPred.Teams?.Away?.AllLastMatches?.Matches?.Draws?.Home
				|| dbApiFootballPrediction.Recent_Away_HomeLosses != feedPred.Teams?.Away?.AllLastMatches?.Matches?.Losses?.Home
				|| dbApiFootballPrediction.Recent_Away_TotalWins != feedPred.Teams?.Away?.AllLastMatches?.Matches?.Wins?.Total
				|| dbApiFootballPrediction.Recent_Away_TotalDraws != feedPred.Teams?.Away?.AllLastMatches?.Matches?.Draws?.Total
				|| dbApiFootballPrediction.Recent_Away_TotalLosses != feedPred.Teams?.Away?.AllLastMatches?.Matches?.Losses?.Total
				|| dbApiFootballPrediction.Recent_Home_AwayWins != feedPred.Teams?.Home?.AllLastMatches?.Matches?.Wins?.Away
				|| dbApiFootballPrediction.Recent_Home_AwayDraws != feedPred.Teams?.Home?.AllLastMatches?.Matches?.Draws?.Away
				|| dbApiFootballPrediction.Recent_Home_AwayLosses != feedPred.Teams?.Home?.AllLastMatches?.Matches?.Losses?.Away
				|| dbApiFootballPrediction.Recent_Home_HomeWins != feedPred.Teams?.Home?.AllLastMatches?.Matches?.Wins?.Home
				|| dbApiFootballPrediction.Recent_Home_HomeDraws != feedPred.Teams?.Home?.AllLastMatches?.Matches?.Draws?.Home
				|| dbApiFootballPrediction.Recent_Home_HomeLosses != feedPred.Teams?.Home?.AllLastMatches?.Matches?.Losses?.Home
				|| dbApiFootballPrediction.Recent_Home_TotalWins != feedPred.Teams?.Home?.AllLastMatches?.Matches?.Wins?.Total
				|| dbApiFootballPrediction.Recent_Home_TotalDraws != feedPred.Teams?.Home?.AllLastMatches?.Matches?.Draws?.Total
				|| dbApiFootballPrediction.Recent_Home_TotalLosses != feedPred.Teams?.Home?.AllLastMatches?.Matches?.Losses?.Total
				|| dbApiFootballPrediction.WinPctAway != feedPred.WinningPercent?.Away
				|| dbApiFootballPrediction.WinPctDraw != feedPred.WinningPercent?.Draw
				|| dbApiFootballPrediction.WinPctHome != feedPred.WinningPercent?.Home)
			{
				hasUpdate = true;
			}

			if (hasUpdate)
			{
				dbApiFootballPrediction.Advice = feedPred.Advice;
				dbApiFootballPrediction.GameTotal = feedPred.GameTotal;
				dbApiFootballPrediction.GoalsAwaySpread = feedPred.GoalsAway;
				dbApiFootballPrediction.GoalsHomeSpread = feedPred.GoalsHome;
				dbApiFootballPrediction.H2H_Home_AwayDraws = feedPred.Teams?.Home?.LastH2H?.Draws?.Away;
				dbApiFootballPrediction.H2H_Home_AwayLosses = feedPred.Teams?.Home?.LastH2H?.Losses?.Away;
				dbApiFootballPrediction.H2H_Home_AwayWins = feedPred.Teams?.Home?.LastH2H?.Wins?.Away;
				dbApiFootballPrediction.H2H_Home_HomeDraws = feedPred.Teams?.Home?.LastH2H?.Draws?.Home;
				dbApiFootballPrediction.H2H_Home_HomeLosses = feedPred.Teams?.Home?.LastH2H?.Losses?.Home;
				dbApiFootballPrediction.H2H_Home_HomeWins = feedPred.Teams?.Home?.LastH2H?.Wins?.Home;
				dbApiFootballPrediction.H2H_Home_TotalDraws = feedPred.Teams?.Home?.LastH2H?.Draws?.Total;
				dbApiFootballPrediction.H2H_Home_TotalLosses = feedPred.Teams?.Home?.LastH2H?.Losses?.Total;
				dbApiFootballPrediction.H2H_Home_TotalWins = feedPred.Teams?.Home?.LastH2H?.Wins?.Total;
				dbApiFootballPrediction.MatchWinner = feedPred.MatchWinner;
				dbApiFootballPrediction.Pred_Away_Attack = feedPred.Comparison?.Att?.AwayWinPct;
				dbApiFootballPrediction.Pred_Away_Defense = feedPred.Comparison?.Def?.AwayWinPct;
				dbApiFootballPrediction.Pred_Away_FishLaw = feedPred.Comparison?.FishLaw?.AwayWinPct;
				dbApiFootballPrediction.Pred_Away_Forme = feedPred.Comparison?.Forme?.AwayWinPct;
				dbApiFootballPrediction.Pred_Away_GoalsH2H = feedPred.Comparison?.GoalsH2H?.AwayWinPct;
				dbApiFootballPrediction.Pred_Away_H2H = feedPred.Comparison?.H2H?.AwayWinPct;
				dbApiFootballPrediction.Pred_Home_Attack = feedPred.Comparison?.Att?.HomeWinPct;
				dbApiFootballPrediction.Pred_Home_Defense = feedPred.Comparison?.Def?.HomeWinPct;
				dbApiFootballPrediction.Pred_Home_FishLaw = feedPred.Comparison?.FishLaw?.HomeWinPct;
				dbApiFootballPrediction.Pred_Home_Forme = feedPred.Comparison?.Forme?.HomeWinPct;
				dbApiFootballPrediction.Pred_Home_GoalsH2H = feedPred.Comparison?.GoalsH2H?.HomeWinPct;
				dbApiFootballPrediction.Pred_Home_H2H = feedPred.Comparison?.H2H?.HomeWinPct;
				dbApiFootballPrediction.RawLast5_Away_AllowedGoals = feedPred.Teams?.Away?.Last5_Matches?.GoalsAgainst;
				dbApiFootballPrediction.RawLast5_Away_Attack = feedPred.Teams?.Away?.Last5_Matches?.Attack;
				dbApiFootballPrediction.RawLast5_Away_Defense = feedPred.Teams?.Away?.Last5_Matches?.Defense;
				dbApiFootballPrediction.RawLast5_Away_Forme = feedPred.Teams?.Away?.Last5_Matches?.Forme;
				dbApiFootballPrediction.RawLast5_Away_Goals = feedPred.Teams?.Away?.Last5_Matches?.Goals;
				dbApiFootballPrediction.RawLast5_Home_AllowedGoals = feedPred.Teams?.Home?.Last5_Matches?.GoalsAgainst;
				dbApiFootballPrediction.RawLast5_Home_Attack = feedPred.Teams?.Home?.Last5_Matches?.Attack;
				dbApiFootballPrediction.RawLast5_Home_Defense = feedPred.Teams?.Home?.Last5_Matches?.Defense;
				dbApiFootballPrediction.RawLast5_Home_Forme = feedPred.Teams?.Home?.Last5_Matches?.Forme;
				dbApiFootballPrediction.RawLast5_Home_Goals = feedPred.Teams?.Home?.Last5_Matches?.Goals;
				dbApiFootballPrediction.Recent_Away_AwayAllowedGoals = feedPred.Teams?.Away?.AllLastMatches?.Goals?.GoalsAgainst?.Away;
				dbApiFootballPrediction.Recent_Away_AwayGoals = feedPred.Teams?.Away?.AllLastMatches?.Goals?.GoalsFor?.Away;
				dbApiFootballPrediction.Recent_Away_HomeAllowedGoals = feedPred.Teams?.Away?.AllLastMatches?.Goals?.GoalsAgainst?.Home;
				dbApiFootballPrediction.Recent_Away_HomeGoals = feedPred.Teams?.Away?.AllLastMatches?.Goals?.GoalsFor?.Home;
				dbApiFootballPrediction.Recent_Away_TotalAllowedGoals = feedPred.Teams?.Away?.AllLastMatches?.Goals?.GoalsAgainst?.Total;
				dbApiFootballPrediction.Recent_Away_TotalGoals = feedPred.Teams?.Away?.AllLastMatches?.Goals?.GoalsFor?.Total;
				dbApiFootballPrediction.Recent_Home_AwayAllowedGoals = feedPred.Teams?.Home?.AllLastMatches?.Goals?.GoalsAgainst?.Away;
				dbApiFootballPrediction.Recent_Home_AwayGoals = feedPred.Teams?.Home?.AllLastMatches?.Goals?.GoalsFor?.Away;
				dbApiFootballPrediction.Recent_Home_HomeAllowedGoals = feedPred.Teams?.Home?.AllLastMatches?.Goals?.GoalsAgainst?.Home;
				dbApiFootballPrediction.Recent_Home_HomeGoals = feedPred.Teams?.Home?.AllLastMatches?.Goals?.GoalsFor?.Home;
				dbApiFootballPrediction.Recent_Home_TotalAllowedGoals = feedPred.Teams?.Home?.AllLastMatches?.Goals?.GoalsAgainst?.Total;
				dbApiFootballPrediction.Recent_Home_TotalGoals = feedPred.Teams?.Home?.AllLastMatches?.Goals?.GoalsFor?.Total;
				dbApiFootballPrediction.Recent_Away_AwayWins = feedPred.Teams?.Away?.AllLastMatches?.Matches?.Wins?.Away;
				dbApiFootballPrediction.Recent_Away_AwayDraws = feedPred.Teams?.Away?.AllLastMatches?.Matches?.Draws?.Away;
				dbApiFootballPrediction.Recent_Away_AwayLosses = feedPred.Teams?.Away?.AllLastMatches?.Matches?.Losses?.Away;
				dbApiFootballPrediction.Recent_Away_HomeWins = feedPred.Teams?.Away?.AllLastMatches?.Matches?.Wins?.Home;
				dbApiFootballPrediction.Recent_Away_HomeDraws = feedPred.Teams?.Away?.AllLastMatches?.Matches?.Draws?.Home;
				dbApiFootballPrediction.Recent_Away_HomeLosses = feedPred.Teams?.Away?.AllLastMatches?.Matches?.Losses?.Home;
				dbApiFootballPrediction.Recent_Away_TotalWins = feedPred.Teams?.Away?.AllLastMatches?.Matches?.Wins?.Total;
				dbApiFootballPrediction.Recent_Away_TotalDraws = feedPred.Teams?.Away?.AllLastMatches?.Matches?.Draws?.Total;
				dbApiFootballPrediction.Recent_Away_TotalLosses = feedPred.Teams?.Away?.AllLastMatches?.Matches?.Losses?.Total;
				dbApiFootballPrediction.Recent_Home_AwayWins = feedPred.Teams?.Home?.AllLastMatches?.Matches?.Wins?.Away;
				dbApiFootballPrediction.Recent_Home_AwayDraws = feedPred.Teams?.Home?.AllLastMatches?.Matches?.Draws?.Away;
				dbApiFootballPrediction.Recent_Home_AwayLosses = feedPred.Teams?.Home?.AllLastMatches?.Matches?.Losses?.Away;
				dbApiFootballPrediction.Recent_Home_HomeWins = feedPred.Teams?.Home?.AllLastMatches?.Matches?.Wins?.Home;
				dbApiFootballPrediction.Recent_Home_HomeDraws = feedPred.Teams?.Home?.AllLastMatches?.Matches?.Draws?.Home;
				dbApiFootballPrediction.Recent_Home_HomeLosses = feedPred.Teams?.Home?.AllLastMatches?.Matches?.Losses?.Home;
				dbApiFootballPrediction.Recent_Home_TotalWins = feedPred.Teams?.Home?.AllLastMatches?.Matches?.Wins?.Total;
				dbApiFootballPrediction.Recent_Home_TotalDraws = feedPred.Teams?.Home?.AllLastMatches?.Matches?.Draws?.Total;
				dbApiFootballPrediction.Recent_Home_TotalLosses = feedPred.Teams?.Home?.AllLastMatches?.Matches?.Losses?.Total;
				dbApiFootballPrediction.WinPctAway = feedPred.WinningPercent?.Away;
				dbApiFootballPrediction.WinPctDraw = feedPred.WinningPercent?.Draw;
				dbApiFootballPrediction.WinPctHome = feedPred.WinningPercent?.Home;

				dbContext.SaveChanges();
			}
		}
	}
}
