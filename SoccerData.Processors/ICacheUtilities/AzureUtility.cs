using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.File;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SoccerData.Processors.ICacheUtilities
{
	public class AzureUtility : ICacheUtility
	{
		private CloudStorageAccount AzureStorage;
		private CloudFileClient AzureFileClient;
		private CloudFileDirectory AzureRootDirectory;

		public AzureUtility(string shareReference)
		{
			string rawConnStr = File.ReadAllText("AzureStorageConnectionString.key");
			this.AzureStorage = CloudStorageAccount.Parse(rawConnStr);
			this.AzureFileClient = AzureStorage.CreateCloudFileClient();

			//CloudFileShare share = AzureFileClient.GetShareReference("apibasketball");
			CloudFileShare share = AzureFileClient.GetShareReference(shareReference);
			share.CreateIfNotExists();

			this.AzureRootDirectory = share.GetRootDirectoryReference();
			this.AzureRootDirectory.CreateIfNotExists();
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
	}
}
