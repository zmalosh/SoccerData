using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Model
{
	public class Country
	{
		public int CountryId { get; set; }
		public string CountryName { get; set; }
		public string CountryAbbr { get; set; }
		public string FlagUrl { get; set; }

		public ICollection<Competition> Competitions { get; set; }
		public ICollection<Team> Teams { get; set; }
	}
}
