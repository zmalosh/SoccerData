using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Model
{
	public class TeamSeason
	{
		public int TeamSeasonId { get; set; }
		public int TeamId { get; set; }
		public int CompetitionSeasonId { get; set; }
		public int Season { get; set; }
		public string TeamName { get; set; }
		public int? VenueSeasonId { get; set; }
		public string LogoUrl { get; set; }

		public virtual Team Team { get; set; }
		public virtual CompetitionSeason CompetitionSeason { get; set; }
		public virtual VenueSeason VenueSeason { get; set; }
		public virtual ICollection<Fixture> HomeFixtures { get; set; }
		public virtual ICollection<Fixture> AwayFixtures { get; set; }
	}
}
