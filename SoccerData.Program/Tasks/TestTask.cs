using SoccerData.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Program.Tasks
{
	public class TestTask : BaseTask
	{
		public override void Run()
		{
			SoccerDataContext context = null;
			DateTime startTime = DateTime.Now;

			int? cacheLength = 1;

			var config = GetConfig();

			context = new SoccerDataContext(config);

			//var bookmakersProcessor = new Processors.ApiFootball.Processors.OddsBookmakersProcessor(cacheLength);
			//bookmakersProcessor.Run(context);

			var oddsLabelsProcessor = new Processors.ApiFootball.Processors.OddsLabelsProcessor(cacheLength);
			oddsLabelsProcessor.Run(context);
		}
	}
}
