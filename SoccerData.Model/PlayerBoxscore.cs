using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Model
{
	public class PlayerBoxscore : IEntity
	{
		public int FixtureId { get; set; }
		public int PlayerSeasonId { get; set; }
		public int TeamSeasonId { get; set; }
		public bool? Played { get; set; }
		public bool? IsStarter { get; set; }
		public bool? IsBench { get; set; }
		public string Position { get; set; }
		public int? JerseyNumber { get; set; }
		public bool? IsCaptain { get; set; }
		public int? MinutesPlayed { get; set; }
		public decimal? Rating { get; set; }
		public int? Goals { get; set; }
		public int? Assists { get; set; }
		public int? YellowCards { get; set; }
		public int? RedCards { get; set; }
		public int? FoulsCommitted { get; set; }
		public int? FoulsSuffered { get; set; }
		public int? ShotsTaken { get; set; }
		public int? ShotsOnGoal { get; set; }
		public int? Offsides { get; set; }
		public int? PassAttempts { get; set; }
		public int? PassAccuracy { get; set; }
		public int? DribblesAttempted { get; set; }
		public int? DribblesSuccessful { get; set; }
		public int? Tackles { get; set; }
		public int? Blocks { get; set; }
		public int? Interceptions { get; set; }
		public int? DuelsTotal { get; set; }
		public int? DuelsWon { get; set; }
		public int? KeyPasses { get; set; }
		public int? DribblesPastDef { get; set; }
		public int? PenaltiesMissed { get; set; }
		public int? PenaltiesScored { get; set; }
		public int? PenaltiesCommitted { get; set; }
		public int? PenaltiesWon { get; set; }
		public int? GoalsConceded { get; set; }
		public int? PenaltiesSaved { get; set; }
		public int? ApiFootballLastUpdate { get; set; }
		public DateTime DateLastModifiedUtc { get; set; }
		public DateTime DateCreatedUtc { get; set; }

		public virtual Fixture Fixture { get; set; }
		public virtual PlayerSeason PlayerSeason { get; set; }
		public virtual TeamSeason TeamSeason { get; set; }
	}
}
