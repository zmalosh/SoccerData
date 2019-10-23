using SoccerData.Processors.ApiFootball.Processors;
using System;
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

				var competitionSeasonIds = context.CompetitionSeasons.Select(x => x.CompetitionSeasonId).Distinct().ToList();
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
