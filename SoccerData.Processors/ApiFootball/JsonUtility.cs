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

		public static string GetRawJsonFromUrl(string url)
		{
			string rawJson;
			using (var client = new WebClient())
			{
				client.Headers.Add("x-rapidapi-host", "api-football-v1.p.rapidapi.com");
				client.Headers.Add("x-rapidapi-key", ApiKey);
				rawJson = client.DownloadString(url);
			}
			return rawJson;
		}
	}
}
