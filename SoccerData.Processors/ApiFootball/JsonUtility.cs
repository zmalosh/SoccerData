using Microsoft.Azure;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.File;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;

namespace SoccerData.Processors.ApiFootball
{
	public static class JsonUtility
	{
		private static readonly string ApiKey = File.ReadAllText("ApiFootball.key");
		private static readonly WebClient WebClient = CreateWebClient();
		private static readonly ICacheUtility CacheUtility = new AzureUtility();

		public static string GetRawJsonFromUrl(string url, int? cacheTimeSeconds = null)
		{
			string cachePath = GetCachePathFromUrl(url);

			if (!CacheUtility.ReadFile(cachePath, out string rawJson, cacheTimeSeconds))
			{
				rawJson = WebClient.DownloadString(url);
				if (cacheTimeSeconds.HasValue && cacheTimeSeconds.Value > 0)
				{
					CacheUtility.WriteFile(cachePath, rawJson);
				}
			}

			return rawJson;
		}

		private static string GetCachePathFromUrl(string url)
		{
			var rawPath = url.Split("/v2/")[1];
			var path = rawPath.Replace("/", "_");
			return path;
		}

		private static WebClient CreateWebClient()
		{
			var client = new WebClient();
			client.Headers.Add("x-rapidapi-host", "api-football-v1.p.rapidapi.com");
			client.Headers.Add("x-rapidapi-key", ApiKey);
			return client;
		}
	}
}
