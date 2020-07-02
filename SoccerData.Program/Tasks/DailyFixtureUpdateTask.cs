using Microsoft.EntityFrameworkCore;
using SoccerData.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoccerData.Program.Tasks
{
	public class DailyFixtureUpdateTask : BaseTask
	{
		public override void Run()
		{
			SoccerDataContext context = null;
			DateTime startTime = DateTime.Now;

			int? cacheLength = 1;

			var config = GetConfig();

			context = new SoccerDataContext(config);

			var countriesProcessor = new Processors.ApiFootball.Processors.CountriesProcessor();
			countriesProcessor.Run(context);

			var leaguesProcessor = new Processors.ApiFootball.Processors.LeaguesProcessor();
			leaguesProcessor.Run(context);

			var apiTransferTeamIds = new List<int>();

			// ALWAYS VERIFY GAMES IN PAST 5 DAYS AND IN UPCOMING 5 DAYS
			var apiUpdatedFixtureIds = context.Fixtures
												.Where(x => x.GameTimeUtc.HasValue
															&& x.GameTimeUtc.Value.AddDays(5).Date > DateTime.UtcNow.Date
															&& x.GameTimeUtc.Value.AddDays(-5).Date <= DateTime.UtcNow.Date)
												.Select(x => x.ApiFootballId)
												.ToList();

			// SEASON IS CURRENT IF IsCurrent AND SEASON ENDS IN FUTURE OR IN PAST 14 DAYS
			var dbCompetitionSeasons = context.CompetitionSeasons
												.Include(x => x.TeamSeasons)
												.ThenInclude(y => y.Team)
												.Where(x => x.IsCurrent && x.EndDate.HasValue && x.EndDate.Value.AddDays(14).Date >= DateTime.UtcNow.Date)
												.ToList();

			for (int i = 0; i < dbCompetitionSeasons.Count; i++)
			{
				var dbCompetitionSeason = dbCompetitionSeasons[i];
				Console.WriteLine($"DAILY COMPETITION SEASON UPDATES - {i + 1} OF {dbCompetitionSeasons.Count}");

				context = new SoccerDataContext(config);

				//var teamsProcessor = new Processors.ApiFootball.Processors.TeamsProcessor(dbCompetitionSeason.CompetitionSeasonId, cacheLengthSec: cacheLength);
				//teamsProcessor.Run(context);

				//var apiTeamIds = dbCompetitionSeason.TeamSeasons.Select(x => x.Team.ApiFootballId).ToList();
				//for (int j = 0; j < apiTeamIds.Count; j++)
				//{
				//	Console.WriteLine($"DAILY COMPETITION SEASON UPDATES - {i + 1} OF {dbCompetitionSeasons.Count} - TEAM SQUAD {j + 1} OF {apiTeamIds.Count}");

				//	var apiTeamId = apiTeamIds[j];
				//	apiTransferTeamIds.Add(apiTeamId);

				//	var teamSquadProcessor = new Processors.ApiFootball.Processors.TeamSquadProcessor(apiTeamId, dbCompetitionSeason, cacheLengthSec: cacheLength);
				//	teamSquadProcessor.Run(context);
				//}

				//context = new SoccerDataContext(config);

				var fixtureLastUpdatedDict_Start = context.Fixtures
													.Where(x => x.CompetitionSeasonId == dbCompetitionSeason.CompetitionSeasonId)
													.ToDictionary(x => x.ApiFootballId, y => y.DateLastModifiedUtc);

				var fixturesProcessor = new Processors.ApiFootball.Processors.LeagueFixturesProcessor(dbCompetitionSeason.CompetitionSeasonId, cacheLengthSec: cacheLength);
				fixturesProcessor.Run(context);

				var fixtureLastUpdatedDict_End = context.Fixtures
													.Where(x => x.CompetitionSeasonId == dbCompetitionSeason.CompetitionSeasonId)
													.ToDictionary(x => x.ApiFootballId, y => y.DateLastModifiedUtc);

				foreach (var fixtureUpdateEntry_End in fixtureLastUpdatedDict_End)
				{
					if (!fixtureLastUpdatedDict_Start.TryGetValue(fixtureUpdateEntry_End.Key, out DateTime startLastUpdateUtc)
						|| startLastUpdateUtc != fixtureUpdateEntry_End.Value)
					{
						apiUpdatedFixtureIds.Add(fixtureUpdateEntry_End.Key);
					}
				}
			}

			apiUpdatedFixtureIds = apiUpdatedFixtureIds.Distinct().ToList();
			for (int i = 0; i < apiUpdatedFixtureIds.Count; i++)
			{
				if (i % 5 == 4)
				{
					context = new SoccerDataContext(config);
				}

				var apiFixtureId = apiUpdatedFixtureIds[i];
				Console.WriteLine($"DAILY FIXTURE UPDATES - {i + 1} OF {apiUpdatedFixtureIds.Count}");

				var fixtureProcessor = new Processors.ApiFootball.Processors.FixtureProcessor(apiFixtureId, cacheLengthSec: cacheLength);
				fixtureProcessor.Run(context);

				var teamFixtureStatsProcessor = new Processors.ApiFootball.Processors.FixtureTeamStatsProcessor(apiFixtureId, cacheLengthSec: cacheLength);
				teamFixtureStatsProcessor.Run(context);

				var apiFootballPredictionsProcessor = new Processors.ApiFootball.Processors.ApiFootballPredictionsProcessor(apiFixtureId, cacheLengthSec: cacheLength);
				apiFootballPredictionsProcessor.Run(context);
			}

			apiTransferTeamIds = apiTransferTeamIds.Distinct().ToList();
			for (int i = 0; i < apiTransferTeamIds.Count; i++)
			{
				if (i % 10 == 9)
				{
					context = new SoccerDataContext(config);
				}

				int apiTransferTeamId = apiTransferTeamIds[i];
				Console.WriteLine($"DAILY TEAM TRANSFERS - {i + 1} OF {apiTransferTeamIds.Count}");

				var transfersProcessor = new Processors.ApiFootball.Processors.TransfersProcessor(apiTransferTeamId);
				transfersProcessor.Run(context);
			}

			DateTime endTime = DateTime.Now;
			TimeSpan runLength = endTime - startTime;
			Console.WriteLine($"{ (runLength).TotalSeconds } SEC ({ runLength.Hours }:{ runLength.Minutes }:{ runLength.Seconds })");
			Console.ReadKey();
		}
	}
}
