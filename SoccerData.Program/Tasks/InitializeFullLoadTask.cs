using SoccerData.Model;
using System;
using System.Collections.Generic;
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
