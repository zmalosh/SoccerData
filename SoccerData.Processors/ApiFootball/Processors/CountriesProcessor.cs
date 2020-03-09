using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SoccerData.Model;

namespace SoccerData.Processors.ApiFootball.Processors
{
	public class CountriesProcessor : IProcessor
	{
		private readonly JsonUtility JsonUtility;

		public CountriesProcessor()
		{
			this.JsonUtility = new JsonUtility(7 * 24 * 60 * 60);
		}

		public void Run(SoccerDataContext dbContext)
		{
			var url = Feeds.CountriesFeed.GetFeedUrl();
			var rawJson = JsonUtility.GetRawJsonFromUrl(url);
			var feed = Feeds.CountriesFeed.FromJson(rawJson);

			var orderedCountries = feed.Result.Countries
												.OrderBy(x => string.Equals(x.CountryName, "World", StringComparison.InvariantCultureIgnoreCase) ? 0 : 1)
												.ThenBy(x => x.Code)
												.ToList();

			var existingCountries = dbContext.Countries.ToDictionary(x => x.CountryAbbr ?? "(null)");

			foreach (var country in orderedCountries)
			{
				if (!existingCountries.ContainsKey(country.Code ?? "(null)"))
				{
					var dbCountry = new Country
					{
						CountryName = country.CountryName,
						CountryAbbr = country.Code,
						FlagUrl = country.Flag?.ToString(),
						ApiFootballCountryName = country.CountryName
					};
					existingCountries.Add(country.Code ?? "(null)", dbCountry);
					dbContext.Countries.Add(dbCountry);
				}
			}
		}
	}
}
