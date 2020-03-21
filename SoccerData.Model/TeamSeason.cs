using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Model
{
	public class TeamSeason : IEntity
	{
		public int TeamSeasonId { get; set; }
		public int TeamId { get; set; }
		public int CompetitionSeasonId { get; set; }
		public int Season { get; set; }
		public string TeamName { get; set; }
		public int? VenueSeasonId { get; set; }
		public string LogoUrl { get; set; }
		public DateTime DateLastModifiedUtc { get; set; }
		public DateTime DateCreatedUtc { get; set; }

		public virtual Team Team { get; set; }
		public virtual CompetitionSeason CompetitionSeason { get; set; }
		public virtual VenueSeason VenueSeason { get; set; }
		public virtual IList<Fixture> HomeFixtures { get; set; }
		public virtual IList<Fixture> AwayFixtures { get; set; }
		public virtual IList<TeamBoxscore> TeamBoxscores { get; set; }
		public virtual IList<TeamBoxscore> OppTeamBoxscores { get; set; }
		public virtual IList<FixtureEvent> FixtureEvents { get; set; }
	}
}
