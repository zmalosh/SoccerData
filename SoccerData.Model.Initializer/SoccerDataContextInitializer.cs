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
			context.Countries.Add(new Country { CountryName = "USA" });
			context.Countries.Add(new Country { CountryName = "Spain" });
			context.SaveChanges();
		}
	}
}
