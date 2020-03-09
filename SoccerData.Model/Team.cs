using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Model
{
	public class Team : IEntity
	{
		public int TeamId { get; set; }
		public string TeamName { get; set; }
		public int CountryId { get; set; }
		public int? YearFounded { get; set; }
		public string LogoUrl { get; set; }
		public int ApiFootballId { get; set; }
		public DateTime DateLastModifiedUtc { get; set; }
		public DateTime DateCreatedUtc { get; set; }

		public virtual Country Country { get; set; }
		public virtual ICollection<TeamSeason> TeamSeasons { get; set; }
	}
}
