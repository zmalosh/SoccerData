using Microsoft.EntityFrameworkCore;
using SoccerData.Model;
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
			DateTime startTime = DateTime.Now;

			try
			{
				var config = GetConfig();

				context = new SoccerDataContext(config);

				//context.Database.EnsureDeleted();
				//context.Database.EnsureCreated();
				//context.SaveChanges();

				//var countriesProcessor = new CountriesProcessor();
				//Console.WriteLine("START COUNTRIES");
				//countriesProcessor.Run(context);
				//Console.WriteLine("SAVE COUNTRIES");
				//context.SaveChanges();
				//Console.WriteLine("END COUNTRIES");

				//var leaguesProcessor = new LeaguesProcessor();
				//Console.WriteLine("START LEAGUES");
				//leaguesProcessor.Run(context);
				//Console.WriteLine("SAVE LEAGUES");
				//context.SaveChanges();
				//Console.WriteLine("END LEAGUES");

				List<int> desiredLeagueIds = null;
				desiredLeagueIds = new List<int>
				{
					751, 752, 51, 403,							// EURO CHAMPS
					1, 749, 750,								// WORLD CUP
					52, 31, 132, 530, 53, 32, 137, 514,			// UEFA CL/EL
					735, 734, 733, 732, 731, 730, 64, 30, 87,	// ESP - LA LIGA
					65, 33, 88, 776,							// ESP - SEGUNDA
					808, 973,									// ESP - COPA DEL REY
					499, 498, 444, 968, 502, 501, 445, 966,		// AND - 1st AND 2nd
					201, 200, 199, 294,							// USA - MLS
					521, 520, 519, 518, 522, 523				// USA - USL
				};
				//desiredLeagueIds = new List<int>
				//{
				//	1, 749, 750,								// WORLD CUP
				//	64, 30, 87,									// ESP - LA LIGA
				//	808, 973,									// ESP - COPA DEL REY
				//	200, 199, 294,								// USA - MLS
				//};
				desiredLeagueIds = new List<int> { 357, 82, 775 }; // 2019 BRAZIL SERIE A, 2018 CHINA SUPER LEAGUE, 2019-2020 ESP LALIGA

				var competitionSeasons = context.CompetitionSeasons
													.Include(x => x.Competition)
													//.Where(x => desiredLeagueIds == null || desiredLeagueIds.Contains(x.ApiFootballId))
													.Where(x => x.StartDate.HasValue && /*x.StartDate.Value.Date >= new DateTime(2014, 01, 01) &&*/ x.StartDate.Value.Date < new DateTime(2016, 01, 01))
													//.Where(x => (x.IsCurrent && x.Competition.CompetitionType.ToUpper() == "LEAGUE" && x.EndDate.HasValue && x.EndDate.Value.Date >= DateTime.Now.Date) && (new List<string> { "ES", "CN", "BR", "AM", "CH", "CZ", "CR", "DE", "DK", "EE", "FO", "GB", "IT", "KR", "PT", "TW" }).Contains(x.Competition.Country.CountryAbbr)) // CURRENT || (PAST FROM COUNTRIES NOT CURRENTLY CANCELLED DUE TO COVID-19)
													//.Where(x => (x.IsCurrent && x.EndDate.Date >= DateTime.Now.Date) || (new List<string> { "MX", "RU", "CL", "DZ", "AR", "TR", "UA", "AU", "IN", "BY", "BR", "CR", "AO", "NI", "HK", "SG" }).Contains(x.Competition.Country.CountryAbbr)) // CURRENT || (PAST FROM COUNTRIES NOT CURRENTLY CANCELLED DUE TO COVID-19)
													//.Where(x =>x.Season >= 2016 && (new List<string> { "MX", "RU", "TR", "AU", "BY", "BR", "AO", "NI", "HK", "SG", "DK", "PL", "PY", "CN" }).Contains(x.Competition.Country.CountryAbbr)) // (PAST FROM COUNTRIES NOT CURRENTLY CANCELLED DUE TO COVID-19)
													//.Where(x => x.Season >= 2016 && (new List<string> { "BY", "NI", "ES" }).Contains(x.Competition.Country.CountryAbbr)) // (PAST FROM COUNTRIES NOT CURRENTLY CANCELLED DUE TO COVID-19)
													//.Where(x => x.IsCurrent && x.Competition.CompetitionType.ToUpper() == "LEAGUE" && x.StartDate.HasValue && x.StartDate.Value.Date >= new DateTime(2019, 01, 01)) // (PAST FROM COUNTRIES NOT CURRENTLY CANCELLED DUE TO COVID-19)
													.OrderBy(x => x.CompetitionSeasonId)
													.ToList();

				int i = 710; // REDO API LEAGUE ID 2432
				List<Team> dbTeams;
				for (; i < competitionSeasons.Count; i++)
				{
					Console.WriteLine($"START LEAGUE {i + 1} OF {competitionSeasons.Count}");

					var competitionSeason = competitionSeasons[i];
					int competitionSeasonId = competitionSeason.CompetitionSeasonId;

					// NULL StartDate AND/OR EndDate INDICATES NO GAMES AVAILABLE... NOTHING MORE TO PROCESS
					if (competitionSeason.StartDate.HasValue && competitionSeason.EndDate.HasValue)
					{
						var teamsProcessor = new TeamsProcessor(competitionSeasonId);
						teamsProcessor.Run(context);

						var roundsProcessor = new CompetitionSeasonRoundsProcessor(competitionSeasonId);
						roundsProcessor.Run(context);

						var leagueFixturesProcessor = new LeagueFixturesProcessor(competitionSeasonId);
						leagueFixturesProcessor.Run(context);

						context.Dispose();
						context = new SoccerDataContext(config);

						dbTeams = context.Teams.Where(x => x.TeamSeasons.Any(y => y.CompetitionSeasonId == competitionSeasonId)).ToList();
						for (int j = 0; j < dbTeams.Count; j++)
						{
							Console.WriteLine($"LEAGUE {i + 1} OF {competitionSeasons.Count} - TEAM {j + 1} OF {dbTeams.Count}");
							var dbTeam = dbTeams[j];
							var teamSquadProcessor = new TeamSquadProcessor(dbTeam.ApiFootballId, competitionSeason);
							teamSquadProcessor.Run(context);

							if (j % 5 == 4)
							{
								Console.WriteLine("NEW CONTEXT");
								context.Dispose();
								context = new SoccerDataContext(config);
							}
						}

						context.Dispose();
						context = new SoccerDataContext(config);

						var competitionSeasonFixtures = context.Fixtures.Where(x => x.CompetitionSeasonId == competitionSeasonId).ToList();
						for (int j = 0; j < competitionSeasonFixtures.Count; j++)
						{
							Console.WriteLine($"LEAGUE {i + 1} OF {competitionSeasons.Count} - FIXTURE {j + 1} OF {competitionSeasonFixtures.Count} - {competitionSeason.Competition.CompetitionName} ({competitionSeason.Competition.CountryId})");

							var dbFixture = competitionSeasonFixtures[j];

							if (string.Equals("Match Finished", dbFixture.Status, StringComparison.CurrentCultureIgnoreCase))
							{
								// TODO: PROCESS FIXTURE DATA (INCLUDE SETTING HasTeamBoxscores VALUE ON FIXTURE)
								var fixtureProcessor = new FixtureProcessor(dbFixture.ApiFootballId);
								fixtureProcessor.Run(context);

								var apiFootballPredictionsProcessor = new ApiFootballPredictionsProcessor(dbFixture.ApiFootballId);
								apiFootballPredictionsProcessor.Run(context);
							}

							if (j % 5 == 4)
							{
								Console.WriteLine("NEW CONTEXT");
								context.Dispose();
								context = new SoccerDataContext(config);
							}
						}

						context.SaveChanges();
					}
				}
				context.SaveChanges();

				var apiTeamIds = context.Teams.Select(x => x.ApiFootballId).Distinct().ToList();
				i = 0;
				for (; i < apiTeamIds.Count; i++)
				{
					Console.WriteLine($"TRANSFERS - TEAM {i + 1} OF {apiTeamIds.Count}");
					var apiTeamId = apiTeamIds[i];
					var teamTransfersProcessor = new TransfersProcessor(apiTeamId);
					teamTransfersProcessor.Run(context);

					if (i % 5 == 4)
					{
						Console.WriteLine("NEW CONTEXT");
						context.Dispose();
						context = new SoccerDataContext(config);
					}
				}
			}
			finally
			{
				if (context != null)
				{
					context.Dispose();
				}
			}

			DateTime endTime = DateTime.Now;
			TimeSpan runLength = endTime - startTime;
			Console.WriteLine($"{ (runLength).TotalSeconds } SEC ({ runLength.Hours }:{ runLength.Minutes }:{ runLength.Seconds })");
			Console.ReadKey();
		}
	}
}
