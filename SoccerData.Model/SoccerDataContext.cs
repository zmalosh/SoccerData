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
		public DbSet<CompetitionSeasonRound> CompetitionSeasonRounds { get; set; }
		public DbSet<Venue> Venues { get; set; }
		public DbSet<VenueSeason> VenueSeasons { get; set; }
		public DbSet<Team> Teams { get; set; }
		public DbSet<TeamSeason> TeamSeasons { get; set; }
		public DbSet<Fixture> Fixtures { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Country>().HasKey(c => c.CountryId);
			modelBuilder.Entity<Country>().Property(c => c.CountryAbbr).HasMaxLength(2);

			modelBuilder.Entity<Competition>().HasKey(c => c.CompetitionId);
			modelBuilder.Entity<Competition>().Property(c => c.CompetitionId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			modelBuilder.Entity<Competition>().HasRequired(c => c.Country).WithMany(c => c.Competitions).HasForeignKey(c => c.CountryId).WillCascadeOnDelete(false);

			modelBuilder.Entity<CompetitionSeason>().HasKey(cs => cs.CompetitionSeasonId);
			modelBuilder.Entity<CompetitionSeason>().Property(cs => cs.CompetitionSeasonId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			modelBuilder.Entity<CompetitionSeason>().HasRequired(cs => cs.Competition).WithMany(c => c.CompetitionSeasons).HasForeignKey(cs => cs.CompetitionId).WillCascadeOnDelete(false);

			modelBuilder.Entity<CompetitionSeasonRound>().HasKey(csr => csr.CompetitionSeasonRoundId);
			modelBuilder.Entity<CompetitionSeasonRound>().Property(csr => csr.CompetitionSeasonRoundId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			modelBuilder.Entity<CompetitionSeasonRound>().HasRequired(csr => csr.CompetitionSeason).WithMany(cs => cs.CompetitionSeasonRounds).HasForeignKey(csr => csr.CompetitionSeasonId).WillCascadeOnDelete(false);

			modelBuilder.Entity<Venue>().HasKey(v => v.VenueId);
			modelBuilder.Entity<Venue>().Property(v => v.VenueId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

			modelBuilder.Entity<VenueSeason>().HasKey(vs => vs.VenueSeasonId);
			modelBuilder.Entity<VenueSeason>().Property(vs => vs.VenueSeasonId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			modelBuilder.Entity<VenueSeason>().HasRequired(vs => vs.Venue).WithMany(v => v.VenueSeasons).HasForeignKey(vs => vs.VenueId).WillCascadeOnDelete(false);

			modelBuilder.Entity<Team>().HasKey(t => t.TeamId);
			modelBuilder.Entity<Team>().Property(t => t.TeamId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			modelBuilder.Entity<Team>().HasRequired(t => t.Country).WithMany(c => c.Teams).HasForeignKey(t => t.CountryId).WillCascadeOnDelete(false);

			modelBuilder.Entity<TeamSeason>().HasKey(ts => ts.TeamSeasonId);
			modelBuilder.Entity<TeamSeason>().Property(ts => ts.TeamSeasonId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			modelBuilder.Entity<TeamSeason>().HasRequired(ts => ts.Team).WithMany(t => t.TeamSeasons).HasForeignKey(ts => ts.TeamId).WillCascadeOnDelete(false);
			modelBuilder.Entity<TeamSeason>().HasRequired(ts => ts.CompetitionSeason).WithMany(cs => cs.TeamSeasons).HasForeignKey(ts => ts.CompetitionSeasonId).WillCascadeOnDelete(false);
			modelBuilder.Entity<TeamSeason>().HasOptional(ts => ts.VenueSeason).WithMany(vs => vs.TeamSeasons).HasForeignKey(ts => ts.VenueSeasonId).WillCascadeOnDelete(false);

			modelBuilder.Entity<Fixture>().HasKey(f => f.FixtureId);
			modelBuilder.Entity<Fixture>().Property(f => f.FixtureId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			modelBuilder.Entity<Fixture>().HasRequired(f => f.CompetitionSeason).WithMany(cs => cs.Fixtures).HasForeignKey(f => f.CompetitionSeasonId).WillCascadeOnDelete(false);
			modelBuilder.Entity<Fixture>().HasRequired(f => f.CompetitionSeasonRound).WithMany(csr => csr.Fixtures).HasForeignKey(f => f.CompetitionSeasonRoundId).WillCascadeOnDelete(false);
			modelBuilder.Entity<TeamSeason>().HasMany(ts => ts.HomeFixtures).WithOptional(f => f.HomeTeamSeason).HasForeignKey(f => f.HomeTeamSeasonId).WillCascadeOnDelete(false);
			modelBuilder.Entity<TeamSeason>().HasMany(ts => ts.AwayFixtures).WithOptional(f => f.AwayTeamSeason).HasForeignKey(f => f.AwayTeamSeasonId).WillCascadeOnDelete(false);
			modelBuilder.Entity<Fixture>().HasOptional(f => f.VenueSeason).WithMany(vs => vs.Fixtures).HasForeignKey(f => f.VenueSeasonId).WillCascadeOnDelete(false);
		}
	}
}
