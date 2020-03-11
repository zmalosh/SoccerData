using SoccerData.Processors.ICacheUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace SoccerData.Processors
{
	public class JsonUtility
	{
		private static readonly string ApiFootballApiKey = File.ReadAllText("ApiFootball.key");
		private const string AzureContainerName = "apifootballcache";
		private const string LocalCachePath = "C:\\FileCache\\ApiFootball";

		private readonly ICacheUtility CacheUtility;
		private readonly int? CacheTimeSeconds;
		private readonly WebClient WebClient;

		public enum JsonSourceType
		{
			Unauthenticated = 0,
			ApiFootball = 1
		}

		public JsonUtility(int? cacheTimeSeconds = null, JsonSourceType sourceType = JsonSourceType.Unauthenticated)
		{
			this.CacheTimeSeconds = cacheTimeSeconds;
			if (!cacheTimeSeconds.HasValue || cacheTimeSeconds.Value == 0)
			{
				this.CacheUtility = new NoCacheUtility();
			}
			else
			{
				//this.CacheUtility = new AzureUtility(AzureContainerName);
				this.CacheUtility = new LocalCacheUtility(LocalCachePath);
			}

			this.WebClient = this.CreateWebClient(sourceType);
		}

		public string GetRawJsonFromUrl(string url)
		{
			string cachePath = GetCachePathFromUrl(url);

			if (!this.CacheTimeSeconds.HasValue || !CacheUtility.ReadFile(cachePath, out string rawJson, this.CacheTimeSeconds))
			{
				try
				{
					rawJson = WebClient.DownloadString(url);
				}
				catch (Exception ex)
				{
					return null;
				}
				if (!this.CacheTimeSeconds.HasValue || (this.CacheTimeSeconds.HasValue && this.CacheTimeSeconds.Value > 0))
				{
					CacheUtility.WriteFile(cachePath, rawJson);
				}
			}

			return rawJson;
		}

		private static string GetCachePathFromUrl(string url)
		{
			var rawPath = url.Split(".com/")[1];
			var path = rawPath.Replace("/", "_").Replace("?", "_");
			return path;
		}

		private WebClient CreateWebClient(JsonSourceType sourceType)
		{
			var client = new WebClient();
			if (sourceType == JsonSourceType.ApiFootball)
			{
				client.Headers.Add("x-rapidapi-host", "api-football-v1.p.rapidapi.com");
				client.Headers.Add("x-rapidapi-key", ApiFootballApiKey);
			}
			return client;
		}
	}
}