using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Program
{
	public class CommandLineOptions
	{
		[Option('i', "initializeFullLoad", Required = false, HelpText = "Initialize database in full.")]
		public bool InitializeFullLoadTask { get; set; }

		[Option('d', "dailyFixtureTask", Required = false, HelpText = "Daily Fixture Data Quality Check")]
		public bool DailyFixtureUpdateTask { get; set; }

		[Option('t', "testTask", Required = false, HelpText = "Debug Test Task")]
		public bool TestTask { get; set; }
	}
}
