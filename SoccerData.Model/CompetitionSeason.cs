using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Model
{
	public class CompetitionSeason
	{
		public int CompetitionSeasonId { get; set; }
		public int CompetitionId { get; set; }
		public int Season { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public bool IsCurrent { get; set; }
		public bool HasFixtures { get; set; }
		public bool HasLineups { get; set; }
		public bool HasPlayerStats { get; set; }
		public bool HasTeamStats { get; set; }
		public bool HasOdds { get; set; }
		public bool HasPlayers { get; set; }
		public bool HasPredictions { get; set; }
		public bool HasStandings { get; set; }
		public bool HasTopScorers { get; set; }

		public virtual Competition Competition { get; set; }
	}
}
