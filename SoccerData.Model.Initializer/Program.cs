using System;

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
			}
		}
	}
}
