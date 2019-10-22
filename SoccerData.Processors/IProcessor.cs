using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Processors
{
	public interface IProcessor
	{
		void Run(Model.SoccerDataContext dbContext);
	}
}
