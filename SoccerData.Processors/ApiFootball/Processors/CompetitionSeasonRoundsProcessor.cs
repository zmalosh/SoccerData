using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using SoccerData.Model;

namespace SoccerData.Processors.ApiFootball.Processors
{
	public class CompetitionSeasonRoundsProcessor : IProcessor
	{
		private int CompetitionSeasonId { get; set; }

		public CompetitionSeasonRoundsProcessor(int competitionSeasonId)
		{
			this.CompetitionSeasonId = competitionSeasonId;
		}

		public void Run(SoccerDataContext dbContext)
		{
			var dbCompetitionSeason = dbContext.CompetitionSeasons.Include(x => x.CompetitionSeasonRounds).SingleOrDefault(x => x.CompetitionSeasonId == this.CompetitionSeasonId);
			if (dbCompetitionSeason == null)
			{
				return;
			}

			Console.WriteLine($"CSR-{dbCompetitionSeason.CompetitionSeasonId}-{dbCompetitionSeason.Season}-{dbCompetitionSeason.Competition.CompetitionName}");

			var url = Feeds.CompetitionSeasonRoundsFeed.GetFeedUrlByLeagueId(dbCompetitionSeason.ApiFootballId);
			var rawJson = JsonUtility.GetRawJsonFromUrl(url);
			var feed = Feeds.CompetitionSeasonRoundsFeed.FromJson(rawJson);

			var dbRounds = dbCompetitionSeason.CompetitionSeasonRounds.ToList();
			var feedRounds = feed.Result.Rounds.Select(x => x.Replace('_', ' ')).ToList();
			foreach (var feedRound in feedRounds)
			{
				var dbRound = dbRounds.SingleOrDefault(x => string.Equals(x.ApiFootballKey, feedRound, StringComparison.InvariantCultureIgnoreCase));
				if (dbRound == null)
				{
					dbRound = new CompetitionSeasonRound
					{
						CompetitionSeasonId = dbCompetitionSeason.CompetitionSeasonId,
						RoundName = feedRound,
						ApiFootballKey = feedRound
					};
					dbRounds.Add(dbRound);
					dbContext.CompetitionSeasonRounds.Add(dbRound);
				}
			}
		}
	}
}
