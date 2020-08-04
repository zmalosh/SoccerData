using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Model
{
	public class OddsLabel : IEntity
	{
		public int OddsLabelId { get; set; }
		public string OddsLabelName { get; set; }
		public int ApiFootballId { get; set; }
		public string ApiFootballName { get; set; }
		public DateTime DateLastModifiedUtc { get; set; }
		public DateTime DateCreatedUtc { get; set; }
	}
}
