using SoccerData.Processors.ApiFootball.Processors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoccerData.Model.Initializer
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			using (var context = new SoccerDataContext())
			{
				var init = new SoccerDataContextInitializer();
				init.InitializeDatabase(context);

				var countriesProcessor = new CountriesProcessor();
				countriesProcessor.Run(context);
				context.SaveChanges();

				var leaguesProcessor = new LeaguesProcessor();
				leaguesProcessor.Run(context);
				context.SaveChanges();

				var competitionSeasonIds = context.CompetitionSeasons
													.Select(x => x.CompetitionSeasonId)
													.Distinct()
													.OrderBy(x => x)
													.ToList();
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
				competitionSeasonIds = competitionSeasonIds.Where(x => desiredLeagueIds.Contains(x)).ToList();
				foreach (var competitionSeasonId in competitionSeasonIds)
				{
					var teamsProcessor = new TeamsProcessor(competitionSeasonId);
					teamsProcessor.Run(context);
					context.SaveChanges();
				}
				var a = 1;
			}
		}
	}
}
