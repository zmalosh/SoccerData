using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Model
{
	public class CompetitionSeasonRound
	{
		public int CompetitionSeasonRoundId { get; set; }
		public int CompetitionSeasonId { get; set; }
		public string RoundName { get; set; }
		public string ApiFootballKey { get; set; }

		public virtual CompetitionSeason CompetitionSeason { get; set; }
		public virtual ICollection<Fixture> Fixtures { get; set; }
	}
}
