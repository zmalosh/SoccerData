using SoccerData.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoccerData.Processors.ApiFootball.Processors
{
	public class OddsLabelsProcessor
	{
		private readonly JsonUtility JsonUtility;

		public OddsLabelsProcessor(int? cacheLengthSec = 120 * 24 * 60 * 60)
		{
			this.JsonUtility = new JsonUtility(cacheLengthSec, sourceType: JsonUtility.JsonSourceType.ApiFootball);
		}

		public void Run(SoccerDataContext dbContext)
		{
			var url = Feeds.OddsLabelsFeed.GetFeedUrl();
			var rawJson = JsonUtility.GetRawJsonFromUrl(url);
			var feed = Feeds.OddsLabelsFeed.FromJson(rawJson);

			var dbLabelsDict = dbContext.OddsLabels.ToDictionary(x => x.ApiFootballId, y => y);
			var apiOddsLabels = feed.Result?.OddsLabels;
			bool hasChange = false;
			if (apiOddsLabels != null && apiOddsLabels.Count > 0)
			{
				foreach (var apiOddsLabel in apiOddsLabels)
				{
					if (!dbLabelsDict.TryGetValue(apiOddsLabel.Id, out OddsLabel dbOddsLabel))
					{
						dbOddsLabel = new OddsLabel
						{
							ApiFootballId = apiOddsLabel.Id,
							ApiFootballName = apiOddsLabel.Name,
							OddsLabelName = apiOddsLabel.Name
						};
						dbLabelsDict.Add(apiOddsLabel.Id, dbOddsLabel);
						dbContext.OddsLabels.Add(dbOddsLabel);
						dbContext.SaveChanges();
					}
					else
					{
						if (dbOddsLabel.ApiFootballName != apiOddsLabel.Name)
						{
							dbOddsLabel.ApiFootballName = apiOddsLabel.Name;
							dbContext.SaveChanges();
						}
					}
				}
			}
		}
	}
}
