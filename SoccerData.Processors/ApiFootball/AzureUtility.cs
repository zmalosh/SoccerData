using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.File;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SoccerData.Processors.ApiFootball
{
	public class AzureUtility : ICacheUtility
	{
		private static CloudStorageAccount AzureStorage;
		private static CloudFileClient AzureFileClient;
		private static CloudFileDirectory AzureRootDirectory;

		static AzureUtility()
		{
			AzureUtility.SetupAzureUtility();
		}

		public bool ReadFile(string path, out string text, int? cacheTimeSeconds = null)
		{
			CloudFile file = AzureRootDirectory.GetFileReference(path);
			if (file.Exists())
			{
				bool isExpired = !cacheTimeSeconds.HasValue;
				if (cacheTimeSeconds.HasValue)
				{
					file.FetchAttributes();
					var expirationTime = file.Properties.LastModified.Value.LocalDateTime.AddSeconds(cacheTimeSeconds.Value);
					isExpired = expirationTime < DateTime.Now;
				}
				if (!isExpired)
				{
					text = file.DownloadText();
					return true;
				}
			}
			text = null;
			return false;
		}

		public void WriteFile(string path, string text)
		{
			var file = AzureRootDirectory.GetFileReference(path);
			if (file.Exists())
			{
				file.Delete();
			}
			file.UploadText(text);
		}

		private static void SetupAzureUtility()
		{
			string rawConnStr = File.ReadAllText("AzureStorageConnectionString.key");
			AzureUtility.AzureStorage = CloudStorageAccount.Parse(rawConnStr);
			AzureUtility.AzureFileClient = AzureStorage.CreateCloudFileClient();

			CloudFileShare share = AzureFileClient.GetShareReference("apifootball");
			share.CreateIfNotExists();

			AzureUtility.AzureRootDirectory = share.GetRootDirectoryReference();
			AzureUtility.AzureRootDirectory.CreateIfNotExists();
		}
	}
}
