using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Model
{
	public class VenueSeason
	{
		public int VenueSeasonId { get; set; }
		public int VenueId { get; set; }
		public int Season { get; set; }
		public string VenueName { get; set; }
		public int? Capacity { get; set; }
		public string SurfaceType { get; set; }

		public virtual Venue Venue { get; set; }
	}
}
