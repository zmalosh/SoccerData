using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Model
{
	public class Player : IEntity
	{
		public int PlayerId { get; set; }
		public string LastName { get; set; }
		public string FirstName { get; set; }
		public string FullName { get; set; }
		public DateTime DateOfBirth { get; set; }
		public int? NationalityId { get; set; }
		public int? BirthCountryId { get; set; }
		public string BirthCity { get; set; }
		public int ApiFootballId { get; set; }
		public DateTime DateLastModifiedUtc { get; set; }
		public DateTime DateCreatedUtc { get; set; }

		public virtual Country Nationality { get; set; }
		public virtual Country BirthCountry { get; set; }
	}
}
