using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace SoccerData.Program
{
	public abstract class BaseTask
	{
		public abstract void Run();

		protected IConfiguration GetConfig()
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
				.AddJsonFile("appsettings.json");

			var config = builder.Build();
			return config;
		}
	}
}
