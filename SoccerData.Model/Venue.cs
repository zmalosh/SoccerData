using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Model
{
	public class Venue
	{
		public int VenueId { get; set; }
		public string VenueName { get; set; }
		public int? Capacity { get; set; }
		public string SurfaceType { get; set; }
		public string VenueCity { get; set; }
		public string VenueAddress { get; set; }
		public string VenueNation { get; set; }

		public virtual ICollection<VenueSeason> VenueSeasons { get; set; }
	}
}
