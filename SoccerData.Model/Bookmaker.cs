using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Model
{
	public class Bookmaker : IEntity
	{
		public int BookmakerId { get; set; }
		public string BookmakerName { get; set; }
		public int ApiFootballId { get; set; }
		public string ApiFootballName { get; set; }
		public DateTime DateLastModifiedUtc { get; set; }
		public DateTime DateCreatedUtc { get; set; }
	}
}
