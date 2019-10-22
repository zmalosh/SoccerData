using System;
using System.Collections.Generic;
using System.Text;
using SoccerData.Model;

namespace SoccerData.Processors.ApiFootball.Processors
{
	public class CountriesProcessor : IProcessor
	{
		public void Run(SoccerDataContext dbContext)
		{
			var url = Feeds.CountriesFeed.GetFeedUrl();
			var rawJson = JsonUtility.GetRawJsonFromUrl(url);
			var feed = Feeds.CountriesFeed.FromJson(rawJson);

			Console.WriteLine($"{feed.Result.Count}");
		}
	}
}
