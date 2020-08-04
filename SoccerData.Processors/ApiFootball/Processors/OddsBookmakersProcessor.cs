using SoccerData.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoccerData.Processors.ApiFootball.Processors
{
	public class OddsBookmakersProcessor : IProcessor
	{
		private readonly JsonUtility JsonUtility;

		public OddsBookmakersProcessor(int? cacheLengthSec = 120 * 24 * 60 * 60)
		{
			this.JsonUtility = new JsonUtility(cacheLengthSec, sourceType: JsonUtility.JsonSourceType.ApiFootball);
		}

		public void Run(SoccerDataContext dbContext)
		{
			var url = Feeds.OddsBookmakersFeed.GetFeedUrl();
			var rawJson = JsonUtility.GetRawJsonFromUrl(url);
			var feed = Feeds.OddsBookmakersFeed.FromJson(rawJson);

			var dbBookmakersDict = dbContext.Bookmakers.ToDictionary(x => x.ApiFootballId, y => y);
			var apiBookmakers = feed.Result?.Bookmakers;
			bool hasChange = false;
			if (apiBookmakers != null && apiBookmakers.Count > 0)
			{
				foreach (var apiBookmaker in apiBookmakers)
				{
					if (!dbBookmakersDict.TryGetValue(apiBookmaker.Id, out Bookmaker dbBookmaker))
					{
						dbBookmaker = new Bookmaker
						{
							ApiFootballId = apiBookmaker.Id,
							ApiFootballName = apiBookmaker.Name,
							BookmakerName = apiBookmaker.Name
						};
						dbBookmakersDict.Add(apiBookmaker.Id, dbBookmaker);
						dbContext.Bookmakers.Add(dbBookmaker);
						hasChange = true;
					}
					else
					{
						if (dbBookmaker.ApiFootballName != apiBookmaker.Name)
						{
							dbBookmaker.ApiFootballName = apiBookmaker.Name;
							hasChange = true;
						}
					}
				}
			}

			if (hasChange)
			{
				dbContext.SaveChanges();
			}
		}
	}
}
