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
		private static readonly WebClient WebClient = CreateWebClient();
		private const string AzureContainerName = "cache";

		private readonly ICacheUtility CacheUtility;
		private readonly int? CacheTimeSeconds;

		public JsonUtility(int? cacheTimeSeconds = null)
		{
			this.CacheTimeSeconds = cacheTimeSeconds;
			if (!cacheTimeSeconds.HasValue || cacheTimeSeconds.Value == 0)
			{
				this.CacheUtility = new NoCacheUtility();
			}
			else
			{
				this.CacheUtility = new AzureUtility(AzureContainerName);
			}
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

		private static WebClient CreateWebClient()
		{
			var client = new WebClient();
			return client;
		}
	}
}