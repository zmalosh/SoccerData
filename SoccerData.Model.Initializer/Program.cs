using Microsoft.Extensions.Configuration;
using SoccerData.Processors.ApiFootball.Processors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SoccerData.Model.Initializer
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			var config = GetConfig();

			var context = new SoccerDataContext(config);

			//var init = new SoccerDataContextInitializer();
			//init.InitializeDatabase(context);

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

			var desiredLeagueIds = new List<int>
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
			desiredLeagueIds = new List<int>
				{
					1, 749, 750,								// WORLD CUP
					64, 30, 87,									// ESP - LA LIGA
					808, 973,									// ESP - COPA DEL REY
					200, 199, 294,								// USA - MLS
				};

			var competitionSeasonIds = context.CompetitionSeasons
												.Where(x => desiredLeagueIds.Contains(x.ApiFootballId))
												.Select(x => x.CompetitionSeasonId)
												.Distinct()
												.OrderBy(x => x)
												.ToList();

			for (int i = 0; i < competitionSeasonIds.Count; i++)
			{
				Console.WriteLine($"START LEAGUE {i+1} OF {competitionSeasonIds.Count}");
				int competitionSeasonId = competitionSeasonIds[i];

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

				context.SaveChanges();

				var leagueFixturesProcessor = new LeagueFixturesProcessor(competitionSeasonId);
				leagueFixturesProcessor.Run(context);

				context.SaveChanges();
			}
			var a = 1;
		}

		static protected IConfiguration GetConfig()
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
				.AddJsonFile("appsettings.json");

			var config = builder.Build();
			return config;
		}
	}
}
