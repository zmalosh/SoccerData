using System;
using System.Collections.Generic;
using System.Linq;
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

			var orderedCountries = feed.Result.Countries
												.OrderBy(x => string.Equals(x.CountryName, "World", StringComparison.InvariantCultureIgnoreCase) ? 0 : 1)
												.ThenBy(x => x.Code)
												.ToList();

			foreach (var country in orderedCountries)
			{
				var dbCountry = new Country
				{
					CountryName = country.CountryName,
					CountryAbbr = country.Code,
					FlagUrl = country.Flag?.ToString()
				};
				dbContext.Countries.Add(dbCountry);
			}
		}
	}
}
