using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Model
{
	public class ApiFootballPrediction : IEntity
	{
		public int FixtureId { get; set; }
		public string Advice { get; set; }
		public int? WinPctHome { get; set; }
		public int? WinPctDraw { get; set; }
		public int? WinPctAway { get; set; }
		public string MatchWinner { get; set; }
		public decimal? GoalsHomeSpread { get; set; }
		public decimal? GoalsAwaySpread { get; set; }
		public decimal? GameTotal { get; set; }
		public int? Pred_Home_Attack { get; set; }
		public int? Pred_Away_Attack { get; set; }
		public int? Pred_Home_Defense { get; set; }
		public int? Pred_Away_Defense { get; set; }
		public int? Pred_Home_FishLaw { get; set; }
		public int? Pred_Away_FishLaw { get; set; }
		public int? Pred_Home_Forme { get; set; }
		public int? Pred_Away_Forme { get; set; }
		public int? Pred_Home_GoalsH2H { get; set; }
		public int? Pred_Away_GoalsH2H { get; set; }
		public int? Pred_Home_H2H { get; set; }
		public int? Pred_Away_H2H { get; set; }
		public int? RawLast5_Home_Attack { get; set; }
		public int? RawLast5_Home_Defense { get; set; }
		public int? RawLast5_Home_Forme { get; set; }
		public int? RawLast5_Home_Goals { get; set; }
		public int? RawLast5_Home_AllowedGoals { get; set; }
		public int? RawLast5_Away_Attack { get; set; }
		public int? RawLast5_Away_Defense { get; set; }
		public int? RawLast5_Away_Forme { get; set; }
		public int? RawLast5_Away_Goals { get; set; }
		public int? RawLast5_Away_AllowedGoals { get; set; }
		public int? H2H_Home_TotalWins { get; set; }
		public int? H2H_Home_HomeWins { get; set; }
		public int? H2H_Home_AwayWins { get; set; }
		public int? H2H_Home_TotalLosses { get; set; }
		public int? H2H_Home_HomeLosses { get; set; }
		public int? H2H_Home_AwayLosses { get; set; }
		public int? H2H_Home_TotalDraws { get; set; }
		public int? H2H_Home_HomeDraws { get; set; }
		public int? H2H_Home_AwayDraws { get; set; }
		public int? Recent_Home_TotalGoals { get; set; }
		public int? Recent_Home_HomeGoals { get; set; }
		public int? Recent_Home_AwayGoals { get; set; }
		public int? Recent_Home_TotalAllowedGoals { get; set; }
		public int? Recent_Home_HomeAllowedGoals { get; set; }
		public int? Recent_Home_AwayAllowedGoals { get; set; }
		public int? Recent_Home_TotalWins { get; set; }
		public int? Recent_Home_HomeWins { get; set; }
		public int? Recent_Home_AwayWins { get; set; }
		public int? Recent_Home_TotalDraws { get; set; }
		public int? Recent_Home_HomeDraws { get; set; }
		public int? Recent_Home_AwayDraws { get; set; }
		public int? Recent_Home_TotalLosses { get; set; }
		public int? Recent_Home_HomeLosses { get; set; }
		public int? Recent_Home_AwayLosses { get; set; }
		public int? Recent_Away_TotalGoals { get; set; }
		public int? Recent_Away_HomeGoals { get; set; }
		public int? Recent_Away_AwayGoals { get; set; }
		public int? Recent_Away_TotalAllowedGoals { get; set; }
		public int? Recent_Away_HomeAllowedGoals { get; set; }
		public int? Recent_Away_AwayAllowedGoals { get; set; }
		public int? Recent_Away_TotalWins { get; set; }
		public int? Recent_Away_HomeWins { get; set; }
		public int? Recent_Away_AwayWins { get; set; }
		public int? Recent_Away_TotalDraws { get; set; }
		public int? Recent_Away_HomeDraws { get; set; }
		public int? Recent_Away_AwayDraws { get; set; }
		public int? Recent_Away_TotalLosses { get; set; }
		public int? Recent_Away_HomeLosses { get; set; }
		public int? Recent_Away_AwayLosses { get; set; }

		public DateTime DateLastModifiedUtc { get; set; }
		public DateTime DateCreatedUtc { get; set; }

		public virtual Fixture Fixture { get; set; }
	}
}
