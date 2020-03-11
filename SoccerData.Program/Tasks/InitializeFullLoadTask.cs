﻿using SoccerData.Model;
using SoccerData.Processors.ApiFootball.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoccerData.Program.Tasks
{
	public class InitializeFullLoadTask : BaseTask
	{
		public override void Run()
		{
			Console.WriteLine("Hello World!");
			SoccerDataContext context = null;

			try
			{
				var config = GetConfig();

				context = new SoccerDataContext(config);

				//context.Database.EnsureDeleted();
				//context.Database.EnsureCreated();
				//context.SaveChanges();

				var countriesProcessor = new CountriesProcessor();
				Console.WriteLine("START COUNTRIES");
				countriesProcessor.Run(context);
				Console.WriteLine("SAVE COUNTRIES");
				context.SaveChanges();
				Console.WriteLine("END COUNTRIES");

				var leaguesProcessor = new LeaguesProcessor();
				Console.WriteLine("START LEAGUES");
				leaguesProcessor.Run(context);
				Console.WriteLine("SAVE LEAGUES");
				context.SaveChanges();
				Console.WriteLine("END LEAGUES");

				List<int> desiredLeagueIds = null;
				//desiredLeagueIds = new List<int>
				//{
				//	751, 752, 51, 403,							// EURO CHAMPS
				//	1, 749, 750,								// WORLD CUP
				//	52, 31, 132, 530, 53, 32, 137, 514,			// UEFA CL/EL
				//	735, 734, 733, 732, 731, 730, 64, 30, 87,	// ESP - LA LIGA
				//	65, 33, 88, 776,							// ESP - SEGUNDA
				//	808, 973,									// ESP - COPA DEL REY
				//	499, 498, 444, 968, 502, 501, 445, 966,		// AND - 1st AND 2nd
				//	201, 200, 199, 294,							// USA - MLS
				//	521, 520, 519, 518, 522, 523				// USA - USL
				//};
				//desiredLeagueIds = new List<int>
				//{
				//	1, 749, 750,								// WORLD CUP
				//	64, 30, 87,									// ESP - LA LIGA
				//	808, 973,									// ESP - COPA DEL REY
				//	200, 199, 294,								// USA - MLS
				//};

				var competitionSeasons = context.CompetitionSeasons
													//.Where(x => desiredLeagueIds == null || desiredLeagueIds.Contains(x.ApiFootballId))
													.Where(x => x.StartDate.Date >= new DateTime(2019, 08, 01))
													.Where(x => x.CompetitionSeasonId != 991) // SOME PROBLEM WITH MISSING TEAM.... CIRCLE BACK LATER
													.OrderBy(x => x.CompetitionSeasonId)
													.ToList();

				for (int i = 0; i < competitionSeasons.Count; i++)
				{
					Console.WriteLine($"START LEAGUE {i + 1} OF {competitionSeasons.Count}");
					var competitionSeason = competitionSeasons[i];
					int competitionSeasonId = competitionSeason.CompetitionSeasonId;

					if (i % 10 == 9)
					{
						Console.WriteLine("NEW CONTEXT");
						context.Dispose();
						context = new SoccerDataContext(config);
					}

					var teamsProcessor = new TeamsProcessor(competitionSeasonId);
					teamsProcessor.Run(context);

					var roundsProcessor = new CompetitionSeasonRoundsProcessor(competitionSeasonId);
					roundsProcessor.Run(context);

					var leagueFixturesProcessor = new LeagueFixturesProcessor(competitionSeasonId);
					leagueFixturesProcessor.Run(context);

					var competitionSeasonFixtures = context.Fixtures.Where(x => x.CompetitionSeasonId == competitionSeasonId).ToList();
					for (int j = 0; j < competitionSeasonFixtures.Count; j++)
					{
						Console.WriteLine($"LEAGUE {i + 1} OF {competitionSeasons.Count} - FIXTURE {j + 1} OF {competitionSeasonFixtures.Count}");

						var dbFixture = competitionSeasonFixtures[j];

						if (competitionSeason.HasTeamStats)
						{
							if (string.Equals("Match Finished", dbFixture.Status, StringComparison.CurrentCultureIgnoreCase))
							{
								var teamBoxscoreProcessor = new FixtureTeamStatsProcessor(dbFixture.ApiFootballId);
								teamBoxscoreProcessor.Run(context);
							}

							if (j % 30 == 29)
							{
								Console.WriteLine("NEW CONTEXT");
								context.Dispose();
								context = new SoccerDataContext(config);
							}
						}
						else
						{
							dbFixture.HasTeamBoxscores = false;
						}
					}
					context.SaveChanges();
				}
			}
			finally
			{
				if (context != null)
				{
					context.Dispose();
				}
			}
		}
	}
}