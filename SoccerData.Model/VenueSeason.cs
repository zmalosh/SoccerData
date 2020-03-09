using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Model
{
	public class VenueSeason : IEntity
	{
		public int VenueSeasonId { get; set; }
		public int VenueId { get; set; }
		public int Season { get; set; }
		public string VenueName { get; set; }
		public int? Capacity { get; set; }
		public string SurfaceType { get; set; }
		public DateTime DateLastModifiedUtc { get; set; }
		public DateTime DateCreatedUtc { get; set; }

		public virtual Venue Venue { get; set; }
		public virtual ICollection<TeamSeason> TeamSeasons { get; set; }
		public virtual ICollection<Fixture> Fixtures { get; set; }
	}
}
