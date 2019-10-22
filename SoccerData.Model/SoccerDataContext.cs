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
		public DbSet<CompetitionSeason> CompetitionSeasons { get; set; }
		public DbSet<Venue> Venues { get; set; }
		public DbSet<Venue> VenueSeasons { get; set; }
		public DbSet<Team> Teams { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Country>().HasKey(c => c.CountryId);
			modelBuilder.Entity<Country>().Property(c => c.CountryAbbr).HasMaxLength(2);

			modelBuilder.Entity<Competition>().HasKey(c => c.CompetitionId);
			modelBuilder.Entity<Competition>().Property(c => c.CompetitionId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			modelBuilder.Entity<Competition>().HasRequired(c => c.Country).WithMany(c => c.Competitions).HasForeignKey(c => c.CountryId);

			modelBuilder.Entity<CompetitionSeason>().HasKey(cs => cs.CompetitionSeasonId);
			modelBuilder.Entity<CompetitionSeason>().Property(cs => cs.CompetitionSeasonId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			modelBuilder.Entity<CompetitionSeason>().HasRequired(cs => cs.Competition).WithMany(c => c.CompetitionSeasons).HasForeignKey(cs => cs.CompetitionId);

			modelBuilder.Entity<Venue>().HasKey(v => v.VenueId);
			modelBuilder.Entity<Venue>().Property(v => v.VenueId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			modelBuilder.Entity<Venue>().HasRequired(v => v.Country).WithMany(c => c.Venues).HasForeignKey(v => v.CountryId);

			modelBuilder.Entity<VenueSeason>().HasKey(vs => vs.VenueSeasonId);
			modelBuilder.Entity<VenueSeason>().Property(vs => vs.VenueSeasonId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			modelBuilder.Entity<VenueSeason>().HasRequired(vs => vs.Venue).WithMany(v => v.VenueSeasons).HasForeignKey(vs => vs.VenueId);

			modelBuilder.Entity<Team>().HasKey(t => t.TeamId);
			modelBuilder.Entity<Team>().Property(t => t.TeamId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			modelBuilder.Entity<Team>().HasRequired(t => t.Country).WithMany(c => c.Teams).HasForeignKey(t => t.CountryId);
		}
	}
}
