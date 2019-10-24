using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Processors
{
	public interface ICacheUtility
	{
		bool ReadFile(string path, out string text, int? cacheTimeSeconds = null);
		void WriteFile(string path, string text);
	}
}
