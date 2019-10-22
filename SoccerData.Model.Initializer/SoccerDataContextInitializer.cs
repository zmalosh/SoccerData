using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Text;

namespace SoccerData.Model.Initializer
{
	public class SoccerDataContextInitializer : DropCreateDatabaseAlways<SoccerDataContext>
	{
		protected override void Seed(SoccerDataContext context)
		{
			base.Seed(context);
		}
	}
}
