using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Model
{
	public class Coach : IEntity
	{
		public int CoachId { get; set; }
		public string CoachName { get; set; }
		public int ApiFootballId { get; set; }
		public DateTime DateLastModifiedUtc { get; set; }
		public DateTime DateCreatedUtc { get; set; }

		public virtual IList<TeamBoxscore> TeamBoxscores { get; set; }
	}
}
