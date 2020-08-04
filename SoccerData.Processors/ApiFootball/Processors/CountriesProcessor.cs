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

		public CountriesProcessor(int? cacheLengthSec = 120 * 24 * 60 * 60)
		{
			this.JsonUtility = new JsonUtility(cacheLengthSec, sourceType: JsonUtility.JsonSourceType.ApiFootball);
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

			var existingCountries = dbContext.Countries.ToDictionary(x => x.ApiFootballCountryName);
			foreach (var country in orderedCountries)
			{
				if (!existingCountries.ContainsKey(country.CountryName))
				{
					var dbCountry = new Country
					{
						CountryName = country.CountryName,
						CountryAbbr = country.Code,
						FlagUrl = country.Flag?.ToString(),
						ApiFootballCountryName = country.CountryName
					};
					existingCountries.Add(country.CountryName, dbCountry);
					dbContext.Countries.Add(dbCountry);
					dbContext.SaveChanges();
				}
			}

			if (dbContext.Countries.Count(x => string.Equals(x.ApiFootballCountryName.ToUpper(), "SCOTLAND")) == 0)
			{
				dbContext.Add(new Country { ApiFootballCountryName = "Scotland", CountryAbbr = "GB", CountryName = "Scotland" });
				dbContext.SaveChanges();
			}

			if (dbContext.Countries.Count(x => string.Equals(x.ApiFootballCountryName.ToUpper(), "NORTHERN-IRELAND")) == 0)
			{
				dbContext.Add(new Country { ApiFootballCountryName = "Northern-Ireland", CountryAbbr = "GB", CountryName = "Northern Ireland" });
				dbContext.SaveChanges();
			}

			if (dbContext.Countries.Count(x => string.Equals(x.ApiFootballCountryName.ToUpper(), "SWAZILAND")) == 0)
			{
				dbContext.Add(new Country { ApiFootballCountryName = "Swaziland", CountryAbbr = "SW", CountryName = "Eswatini" });
				dbContext.SaveChanges();
			}
		}
	}
}
