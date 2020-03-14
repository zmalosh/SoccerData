using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Processors.ApiFootball.Feeds
{
	public class FixtureFeed
	{
		public static string GetFeedUrlByFixtureId(int apiFootballFixtureId)
		{
			return $"https://api-football-v1.p.rapidapi.com/v2/fixtures/id/{apiFootballFixtureId}";
		}
	}
}
