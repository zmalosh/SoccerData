using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Model
{
	public class Transfer : IEntity
	{
		public int TransferId { get; set; }
		public int PlayerId { get; set; }
		public int? SourceTeamId { get; set; }
		public int? DestTeamId { get; set; }
		public DateTime TransferDate { get; set; }
		public string TransferType { get; set; }
		public DateTime DateLastModifiedUtc { get; set; }
		public DateTime DateCreatedUtc { get; set; }

		public virtual Player Player { get; set; }
		public virtual Team SourceTeam { get; set; }
		public virtual Team DestTeam { get; set; }
	}
}
