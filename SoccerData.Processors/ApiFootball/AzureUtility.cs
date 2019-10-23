using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.File;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SoccerData.Processors.ApiFootball
{
	public class AzureUtility
	{
		private static CloudStorageAccount AzureStorage;
		private static CloudFileClient AzureFileClient;
		private static CloudFileDirectory AzureRootDirectory;

		static AzureUtility()
		{
			AzureUtility.SetupAzureUtility();
		}

		public bool ReadFileFromAzure(string path, out string text)
		{
			CloudFile file = AzureRootDirectory.GetFileReference(path);
			if (file.Exists())
			{
				text = file.DownloadText();
				return true;
			}
			text = null;
			return false;
		}

		public void WriteFileToAzure(string path, string text)
		{
			var file = AzureRootDirectory.GetFileReference(path);
			if (!file.Exists())
			{
				file.UploadText(text);
			}
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
