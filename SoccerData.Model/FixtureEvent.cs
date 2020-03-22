using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Model
{
	public class FixtureEvent : IEntity
	{
		public int FixtureEventId { get; set; }
		public int FixtureId { get; set; }
		public int TeamSeasonId { get; set; }
		public int? PlayerSeasonId { get; set; }
		public int? SecondaryPlayerSeasonId { get; set; }
		public string EventType { get; set; }
		public string EventDetail { get; set; }
		public int EventTime { get; set; }
		public int? EventTimePlus { get; set; }
		public string EventComment { get; set; }
		public DateTime DateLastModifiedUtc { get; set; }
		public DateTime DateCreatedUtc { get; set; }

		public virtual Fixture Fixture { get; set; }
		public virtual TeamSeason TeamSeason { get; set; }
		public virtual PlayerSeason PlayerSeason { get; set; }
		public virtual PlayerSeason SecondaryPlayerSeason { get; set; }
	}
}
