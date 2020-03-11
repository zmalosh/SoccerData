using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Processors.ICacheUtilities
{
	public class LocalCacheUtility : ICacheUtility
	{
		private readonly string LocalDirPath;

		public LocalCacheUtility(string localDirPath)
		{
			if (!localDirPath.EndsWith('\\'))
			{
				this.LocalDirPath = $"{localDirPath}\\";
			}
			else
			{
				this.LocalDirPath = localDirPath;
			}
		}

		public bool ReadFile(string path, out string text, int? cacheTimeSeconds = null)
		{
			var fullFilePath = this.GetFullFilePath(path);
			if (System.IO.File.Exists(fullFilePath))
			{
				bool isExpired = !cacheTimeSeconds.HasValue;
				if (cacheTimeSeconds.HasValue)
				{
					DateTime fileLastWrite = System.IO.File.GetLastWriteTime(fullFilePath);
					var expirationTime = fileLastWrite.AddSeconds(cacheTimeSeconds.Value);
					isExpired = expirationTime < DateTime.Now;
				}
				if (!isExpired)
				{
					text = System.IO.File.ReadAllText(fullFilePath);
					return true;
				}
			}
			text = null;
			return false;
		}

		public void WriteFile(string path, string text)
		{
			var fullFilePath = this.GetFullFilePath(path);
			if (System.IO.File.Exists(fullFilePath))
			{
				System.IO.File.Delete(fullFilePath);
			}
			System.IO.File.WriteAllText(fullFilePath, text);
		}

		private string GetFullFilePath(string relFilePath)
		{
			return string.Concat(this.LocalDirPath, relFilePath, ".json");
		}
	}
}
