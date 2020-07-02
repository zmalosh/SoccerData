using CommandLine;
using System;

namespace SoccerData.Program
{
	class Program
	{
		static void Main(string[] args)
		{
			BaseTask task = null;

			Parser.Default.ParseArguments<CommandLineOptions>(args)
				.WithParsed(o =>
				{
					if (o.InitializeFullLoadTask)
					{
						task = new Tasks.InitializeFullLoadTask();
					}

					if (o.DailyFixtureUpdateTask)
					{
						task = new Tasks.DailyFixtureUpdateTask();
					}
				});

			if (task != null)
			{
				task.Run();
			}
		}
	}
}
