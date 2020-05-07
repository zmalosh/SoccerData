using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Model
{
	public class Competition : IEntity
	{
		public int CompetitionId { get; set; }
		public int CountryId { get; set; }
		public string CompetitionName { get; set; }
		public string CompetitionType { get; set; }
		public int? CurrentTier { get; set; }
		public string LogoUrl { get; set; }
		public DateTime DateLastModifiedUtc { get; set; }
		public DateTime DateCreatedUtc { get; set; }

		public virtual Country Country { get; set; }
		public virtual ICollection<CompetitionSeason> CompetitionSeasons { get; set; }
	}
}
