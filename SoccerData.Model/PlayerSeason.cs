using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Model
{
	public class PlayerSeason : IEntity
	{
		public int PlayerSeasonId { get; set; }
		public int PlayerId { get; set; }
		public int CompetitionSeasonId { get; set; }
		public string Position { get; set; }
		public int? JerseyNumber { get; set; }
		public int? Height { get; set; }
		public int? Weight { get; set; }
		public DateTime DateLastModifiedUtc { get; set; }
		public DateTime DateCreatedUtc { get; set; }

		public virtual Player Player { get; set; }
		public virtual CompetitionSeason CompetitionSeason { get; set; }
	}
}
