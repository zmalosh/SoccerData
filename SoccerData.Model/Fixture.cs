using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Model
{
	public class Fixture : IEntity
	{
		public int FixtureId { get; set; }
		public int CompetitionSeasonId { get; set; }
		public int? HomeTeamSeasonId { get; set; }
		public int? AwayTeamSeasonId { get; set; }
		public DateTime? GameTimeUtc { get; set; }
		public int? HomeScore { get; set; }
		public int? AwayScore { get; set; }
		public string Status { get; set; }
		public string StatusShort { get; set; }
		public int? TimeElapsed { get; set; }
		public int? HomeCoachId { get; set; }
		public int? AwayCoachId { get; set; }
		public string HomeFormation { get; set; }
		public string AwayFormation { get; set; }
		public string Referee { get; set; }
		public string VenueName { get; set; }
		public int? VenueSeasonId { get; set; }
		public int? HomeHalfTimeScore { get; set; }
		public int? AwayHalfTimeScore { get; set; }
		public int? HomeFullTimeScore { get; set; }
		public int? AwayFullTimeScore { get; set; }
		public int? HomeExtraTimeScore { get; set; }
		public int? AwayExtraTimeScore { get; set; }
		public int? HomePenaltiesScore { get; set; }
		public int? AwayPenaltiesScore { get; set; }
		public int CompetitionSeasonRoundId { get; set; }
		public DateTime? FirstHalfStartUtc { get; set; }
		public DateTime? SecondHalfStartUtc { get; set; }
		public int ApiFootballId { get; set; }
		public bool? HasTeamBoxscores { get; set; }
		public DateTime DateLastModifiedUtc { get; set; }
		public DateTime DateCreatedUtc { get; set; }

		public virtual CompetitionSeason CompetitionSeason { get; set; }
		public virtual CompetitionSeasonRound CompetitionSeasonRound { get; set; }
		public virtual TeamSeason HomeTeamSeason { get; set; }
		public virtual TeamSeason AwayTeamSeason { get; set; }
		public virtual Coach HomeCoach { get; set; }
		public virtual Coach AwayCoach { get; set; }
		public virtual VenueSeason VenueSeason { get; set; }
		public virtual ApiFootballPrediction ApiFootballPrediction { get; set; }
		public virtual IList<TeamBoxscore> TeamBoxscores { get; set; }
		public virtual IList<PlayerBoxscore> PlayerBoxscores { get; set; }
		public virtual IList<FixtureEvent> FixtureEvents { get; set; }
	}
}
