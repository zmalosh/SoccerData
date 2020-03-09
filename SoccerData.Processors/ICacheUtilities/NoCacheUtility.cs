using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Processors.ICacheUtilities
{
	public class NoCacheUtility : ICacheUtility
	{
		public bool ReadFile(string path, out string text, int? cacheTimeSeconds = null)
		{
			text = null;
			return false;
		}

		public void WriteFile(string path, string text)
		{
			return;
		}
	}
}
