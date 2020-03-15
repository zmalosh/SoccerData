using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Model
{
	public class Player : IEntity
	{
		public int PlayerId { get; set; }
		public string PlayerName { get; set; }
		public string LastName { get; set; }
		public string FirstName { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public string Nationality { get; set; }
		public string BirthCountry { get; set; }
		public string BirthCity { get; set; }
		public int ApiFootballId { get; set; }
		public DateTime DateLastModifiedUtc { get; set; }
		public DateTime DateCreatedUtc { get; set; }

		public virtual IList<PlayerSeason> PlayerSeasons { get; set; }
	}
}
