using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Text;

namespace SoccerData.Model
{
	public class SoccerDataContext : DbContext
	{
		public SoccerDataContext() : base("SoccerDataContext") { }

		public DbSet<Country> Countries { get; set; }
		public DbSet<Competition> Competitions { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Country>().HasKey(c => c.CountryId);
			modelBuilder.Entity<Country>().Property(c => c.CountryAbbr).HasMaxLength(2);

			modelBuilder.Entity<Competition>().HasKey(c => c.CompetitionId);
			modelBuilder.Entity<Competition>().Property(c => c.CompetitionId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			modelBuilder.Entity<Competition>().HasRequired(c => c.Country).WithMany(c => c.Competitions).HasForeignKey(c => c.CountryId);
		}
	}
}
