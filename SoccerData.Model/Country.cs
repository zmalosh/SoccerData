using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Model
{
	public class Country : IEntity
	{
		public int CountryId { get; set; }
		public string CountryName { get; set; }
		public string CountryAbbr { get; set; }
		public string FlagUrl { get; set; }
		public string ApiFootballCountryName { get; set; }
		public DateTime DateLastModifiedUtc { get; set; }
		public DateTime DateCreatedUtc { get; set; }

		public ICollection<Competition> Competitions { get; set; }
		public ICollection<Team> Teams { get; set; }
		public ICollection<Player> NationalPlayers { get; set; }
		public ICollection<Player> BornPlayers { get; set; }
	}
}
