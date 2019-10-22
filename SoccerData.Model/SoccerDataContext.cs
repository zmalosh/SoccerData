using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Text;

namespace SoccerData.Model
{
	public class SoccerDataContext : DbContext
	{
		public SoccerDataContext() : base("SoccerDataContext") { }

		public DbSet<Country> Countries { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Country>().HasKey(c => c.CountryId);
			modelBuilder.Entity<Country>().Property(c => c.CountryAbbr).HasMaxLength(2);
		}
	}
}
