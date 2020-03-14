using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Model
{
	public class TeamBoxscore : IEntity
	{
		public int FixtureId { get; set; }
		public int TeamSeasonId { get; set; }
		public int OppTeamSeasonId { get; set; }
		public bool IsHome { get; set; }
		public int? CoachId { get; set; }
		public string Formation { get; set; }
		public int? ShotsOnGoal { get; set; }
		public int? ShotsOffGoal { get; set; }
		public int? ShotsBlocked { get; set; }
		public int? ShotsTotal { get; set; }
		public int? ShotsInsideBox { get; set; }
		public int? ShotsOutsideBox { get; set; }
		public int? FoulsCommitted { get; set; }
		public int? CornerKicks { get; set; }
		public int? OffsidesCommitted { get; set; }
		public int? PossessionPct { get; set; }
		public int? YellowCards { get; set; }
		public int? RedCards { get; set; }
		public int? GoalieSaves { get; set; }
		public int? PassesTotal { get; set; }
		public int? PassesAccurate { get; set; }
		public int? PassAccuracyPct { get; set; }
		public DateTime DateLastModifiedUtc { get; set; }
		public DateTime DateCreatedUtc { get; set; }

		public virtual Fixture Fixture { get; set; }
		public virtual TeamSeason TeamSeason { get; set; }
		public virtual TeamSeason OppTeamSeason { get; set; }
		public virtual Coach Coach { get; set; }
	}
}
