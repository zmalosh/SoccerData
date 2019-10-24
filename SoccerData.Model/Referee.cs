using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Model
{
	public class Referee
	{
		public int RefereeId { get; set; }
		public string ApiFootballKey { get; set; }

		public virtual ICollection<Fixture> Fixtures { get; set; }
	}
}
