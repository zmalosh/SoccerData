using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using Microsoft.EntityFrameworkCore;

namespace SoccerData.Model
{
	public class SoccerDataContext : DbContext
	{
		private readonly IConfiguration config;

		public SoccerDataContext(IConfiguration config) : base()
		{
			this.config = config;
			this.ChangeTracker.Tracked += OnEntityTracked;
			this.ChangeTracker.StateChanged += OnEntityStateChanged;
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			//IConfigurationRoot configuration = new ConfigurationBuilder()
			//	.SetBasePath(Directory.GetCurrentDirectory())
			//	.AddJsonFile("appsettings.json")
			//	.Build();
			var connectionString = this.config["SoccerDataContextConnectionString"];
			optionsBuilder.UseSqlServer(connectionString);
		}


		public DbSet<Country> Countries { get; set; }
		public DbSet<Competition> Competitions { get; set; }
		public DbSet<CompetitionSeason> CompetitionSeasons { get; set; }
		public DbSet<CompetitionSeasonRound> CompetitionSeasonRounds { get; set; }
		public DbSet<Venue> Venues { get; set; }
		public DbSet<VenueSeason> VenueSeasons { get; set; }
		public DbSet<Team> Teams { get; set; }
		public DbSet<TeamSeason> TeamSeasons { get; set; }
		public DbSet<Fixture> Fixtures { get; set; }
		public DbSet<Player> Players { get; set; }
		public DbSet<PlayerSeason> PlayerSeasons { get; set; }
		public DbSet<Coach> Coaches { get; set; }
		public DbSet<TeamBoxscore> TeamBoxscores { get; set; }
		public DbSet<PlayerBoxscore> PlayerBoxscores { get; set; }
		public DbSet<FixtureEvent> FixtureEvents { get; set; }
		public DbSet<Transfer> Transfers { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Country>(e =>
			{
				e.HasKey(c => c.CountryId);
				e.Property(c => c.CountryName).HasMaxLength(32);
				e.Property(c => c.FlagUrl).HasMaxLength(256);
				e.Property(c => c.ApiFootballCountryName).HasMaxLength(32);
				e.Property(c => c.DateCreatedUtc).HasColumnType("datetime");
				e.Property(c => c.DateLastModifiedUtc).HasColumnType("datetime");
				e.Property(c => c.CountryAbbr).HasMaxLength(2);
			});

			modelBuilder.Entity<Competition>(e =>
			{
				e.HasKey(c => c.CompetitionId);
				e.Property(c => c.CompetitionName).HasMaxLength(64);
				e.Property(c => c.CompetitionType).HasMaxLength(16);
				e.Property(c => c.LogoUrl).HasMaxLength(256);
				e.Property(c => c.DateCreatedUtc).HasColumnType("datetime");
				e.Property(c => c.DateLastModifiedUtc).HasColumnType("datetime");
				e.HasOne(c => c.Country).WithMany(c => c.Competitions).HasForeignKey(c => c.CountryId).OnDelete(DeleteBehavior.ClientSetNull);
			});

			modelBuilder.Entity<CompetitionSeason>(e =>
			{
				e.HasKey(cs => cs.CompetitionSeasonId);
				e.Property(cs => cs.StartDate).HasColumnType("datetime");
				e.Property(cs => cs.EndDate).HasColumnType("datetime");
				e.Property(cs => cs.DateCreatedUtc).HasColumnType("datetime");
				e.Property(cs => cs.DateLastModifiedUtc).HasColumnType("datetime");
				e.HasOne(cs => cs.Competition).WithMany(c => c.CompetitionSeasons).HasForeignKey(cs => cs.CompetitionId).OnDelete(DeleteBehavior.ClientSetNull);
			});

			modelBuilder.Entity<CompetitionSeasonRound>(e =>
			{
				e.HasKey(csr => csr.CompetitionSeasonRoundId);
				e.Property(csr => csr.RoundName).HasMaxLength(64);
				e.Property(csr => csr.ApiFootballKey).HasMaxLength(64);
				e.Property(csr => csr.DateCreatedUtc).HasColumnType("datetime");
				e.Property(csr => csr.DateLastModifiedUtc).HasColumnType("datetime");
				e.HasOne(csr => csr.CompetitionSeason).WithMany(cs => cs.CompetitionSeasonRounds).HasForeignKey(csr => csr.CompetitionSeasonId).OnDelete(DeleteBehavior.ClientSetNull);
			});

			modelBuilder.Entity<Venue>(e =>
			{
				e.HasKey(v => v.VenueId);
				e.Property(v => v.VenueName).HasMaxLength(128);
				e.Property(v => v.SurfaceType).HasMaxLength(32);
				e.Property(v => v.VenueCity).HasMaxLength(128);
				e.Property(v => v.VenueAddress).HasMaxLength(128);
				e.Property(v => v.VenueNation).HasMaxLength(32);
				e.Property(v => v.DateCreatedUtc).HasColumnType("datetime");
				e.Property(v => v.DateLastModifiedUtc).HasColumnType("datetime");
			});

			modelBuilder.Entity<VenueSeason>(e =>
			{
				e.HasKey(vs => vs.VenueSeasonId);
				e.Property(v => v.VenueName).HasMaxLength(128);
				e.Property(v => v.SurfaceType).HasMaxLength(32);
				e.Property(vs => vs.DateCreatedUtc).HasColumnType("datetime");
				e.Property(vs => vs.DateLastModifiedUtc).HasColumnType("datetime");
				e.HasOne(vs => vs.Venue).WithMany(v => v.VenueSeasons).HasForeignKey(vs => vs.VenueId).OnDelete(DeleteBehavior.ClientSetNull);
			});

			modelBuilder.Entity<Team>(e =>
			{
				e.HasKey(t => t.TeamId);
				e.Property(t => t.TeamName).HasMaxLength(64);
				e.Property(t => t.LogoUrl).HasMaxLength(256);
				e.Property(t => t.CountryName).HasMaxLength(128);
				e.Property(t => t.DateCreatedUtc).HasColumnType("datetime");
				e.Property(t => t.DateLastModifiedUtc).HasColumnType("datetime");
				e.HasMany(t => t.DestTransfers).WithOne(t => t.DestTeam).HasForeignKey(t => t.DestTeamId).OnDelete(DeleteBehavior.ClientSetNull);
				e.HasMany(t => t.SourceTransfers).WithOne(t => t.SourceTeam).HasForeignKey(t => t.SourceTeamId).OnDelete(DeleteBehavior.ClientSetNull);
			});

			modelBuilder.Entity<Player>(e =>
			{
				e.HasKey(p => p.PlayerId);
				e.Property(p => p.FirstName).HasMaxLength(64).IsRequired(false);
				e.Property(p => p.LastName).HasMaxLength(64).IsRequired(false);
				e.Property(p => p.PlayerName).HasMaxLength(128).IsRequired(false);
				e.Property(p => p.ApiFootballName).HasMaxLength(128).IsRequired(false);
				e.Property(p => p.Nationality).HasMaxLength(64).IsRequired(false);
				e.Property(p => p.BirthCity).HasMaxLength(128).IsRequired(false);
				e.Property(p => p.BirthCountry).HasMaxLength(64).IsRequired(false);
				e.Property(t => t.DateOfBirth).HasColumnType("date");
				e.Property(t => t.DateCreatedUtc).HasColumnType("datetime");
				e.Property(t => t.DateLastModifiedUtc).HasColumnType("datetime");
			});

			modelBuilder.Entity<Fixture>(e =>
			{
				e.HasKey(f => f.FixtureId);
				e.Property(f => f.GameTimeUtc).HasColumnType("datetime");
				e.Property(f => f.FirstHalfStartUtc).HasColumnType("datetime");
				e.Property(f => f.SecondHalfStartUtc).HasColumnType("datetime");
				e.Property(f => f.DateCreatedUtc).HasColumnType("datetime");
				e.Property(f => f.DateLastModifiedUtc).HasColumnType("datetime");
				e.Property(f => f.Status).HasMaxLength(32);
				e.Property(f => f.StatusShort).HasMaxLength(8);
				e.Property(f => f.HomeFormation).HasMaxLength(16);
				e.Property(f => f.AwayFormation).HasMaxLength(16);
				e.Property(f => f.Referee).HasMaxLength(128);
				e.Property(f => f.VenueName).HasMaxLength(128);
				e.HasOne(f => f.CompetitionSeason).WithMany(cs => cs.Fixtures).HasForeignKey(f => f.CompetitionSeasonId).OnDelete(DeleteBehavior.ClientSetNull);
				e.HasOne(f => f.CompetitionSeasonRound).WithMany(csr => csr.Fixtures).HasForeignKey(f => f.CompetitionSeasonRoundId).OnDelete(DeleteBehavior.ClientSetNull);
				e.HasOne(f => f.VenueSeason).WithMany(vs => vs.Fixtures).HasForeignKey(f => f.VenueSeasonId).OnDelete(DeleteBehavior.ClientSetNull);
			});

			modelBuilder.Entity<Coach>(e =>
			{
				e.HasKey(c => c.CoachId);
				e.Property(c => c.CoachName).IsRequired(false).HasMaxLength(128);
				e.Property(c => c.DateCreatedUtc).HasColumnType("datetime");
				e.Property(c => c.DateLastModifiedUtc).HasColumnType("datetime");
				e.HasMany(c => c.HomeFixtures).WithOne(f => f.HomeCoach).HasForeignKey(f => f.HomeCoachId).OnDelete(DeleteBehavior.ClientSetNull);
				e.HasMany(c => c.AwayFixtures).WithOne(f => f.AwayCoach).HasForeignKey(f => f.AwayCoachId).OnDelete(DeleteBehavior.ClientSetNull);
			});

			modelBuilder.Entity<TeamBoxscore>(e =>
			{
				e.HasKey(tb => new { tb.FixtureId, tb.TeamSeasonId });
				e.Property(tb => tb.DateCreatedUtc).HasColumnType("datetime");
				e.Property(tb => tb.DateLastModifiedUtc).HasColumnType("datetime");
				e.HasOne(tb => tb.Fixture).WithMany(f => f.TeamBoxscores).HasForeignKey(tb => tb.FixtureId).OnDelete(DeleteBehavior.ClientSetNull);
			});

			modelBuilder.Entity<PlayerBoxscore>(e =>
			{
				e.HasKey(pb => new { pb.FixtureId, pb.PlayerSeasonId });
				e.Property(pb => pb.Position).HasMaxLength(16).IsRequired(false);
				e.Property(pb => pb.Assists).HasColumnType("tinyint").IsRequired(false);
				e.Property(pb => pb.Blocks).HasColumnType("tinyint").IsRequired(false);
				e.Property(pb => pb.DribblesAttempted).HasColumnType("tinyint").IsRequired(false);
				e.Property(pb => pb.DribblesPastDef).HasColumnType("tinyint").IsRequired(false);
				e.Property(pb => pb.DribblesSuccessful).HasColumnType("tinyint").IsRequired(false);
				e.Property(pb => pb.DuelsTotal).HasColumnType("tinyint").IsRequired(false);
				e.Property(pb => pb.DuelsWon).HasColumnType("tinyint").IsRequired(false);
				e.Property(pb => pb.FoulsCommitted).HasColumnType("tinyint").IsRequired(false);
				e.Property(pb => pb.FoulsSuffered).HasColumnType("tinyint").IsRequired(false);
				e.Property(pb => pb.Goals).HasColumnType("tinyint").IsRequired(false);
				e.Property(pb => pb.GoalsConceded).HasColumnType("tinyint").IsRequired(false);
				e.Property(pb => pb.Interceptions).HasColumnType("tinyint").IsRequired(false);
				e.Property(pb => pb.JerseyNumber).HasColumnType("tinyint").IsRequired(false);
				e.Property(pb => pb.KeyPasses).HasColumnType("tinyint").IsRequired(false);
				e.Property(pb => pb.MinutesPlayed).HasColumnType("tinyint").IsRequired(false);
				e.Property(pb => pb.PassAccuracy).HasColumnType("tinyint").IsRequired(false);
				e.Property(pb => pb.PassAttempts).HasColumnType("tinyint").IsRequired(false);
				e.Property(pb => pb.PenaltiesCommitted).HasColumnType("tinyint").IsRequired(false);
				e.Property(pb => pb.PenaltiesMissed).HasColumnType("tinyint").IsRequired(false);
				e.Property(pb => pb.PenaltiesSaved).HasColumnType("tinyint").IsRequired(false);
				e.Property(pb => pb.PenaltiesScored).HasColumnType("tinyint").IsRequired(false);
				e.Property(pb => pb.PenaltiesWon).HasColumnType("tinyint").IsRequired(false);
				e.Property(pb => pb.RedCards).HasColumnType("tinyint").IsRequired(false);
				e.Property(pb => pb.ShotsOnGoal).HasColumnType("tinyint").IsRequired(false);
				e.Property(pb => pb.ShotsTaken).HasColumnType("tinyint").IsRequired(false);
				e.Property(pb => pb.Tackles).HasColumnType("tinyint").IsRequired(false);
				e.Property(pb => pb.YellowCards).HasColumnType("tinyint").IsRequired(false);
				e.Property(pb => pb.Rating).HasColumnType("decimal(3,1)").IsRequired(false);
				e.Property(pb => pb.DateCreatedUtc).HasColumnType("datetime");
				e.Property(pb => pb.DateLastModifiedUtc).HasColumnType("datetime");
				e.HasOne(pb => pb.Fixture).WithMany(ps => ps.PlayerBoxscores).HasForeignKey(pb => pb.FixtureId).OnDelete(DeleteBehavior.ClientSetNull);
				e.HasOne(pb => pb.PlayerSeason).WithMany(ps => ps.PlayerBoxscores).HasForeignKey(pb => pb.PlayerSeasonId).OnDelete(DeleteBehavior.ClientSetNull);
				e.HasOne(pb => pb.TeamSeason).WithMany(ps => ps.PlayerBoxscores).HasForeignKey(pb => pb.TeamSeasonId).OnDelete(DeleteBehavior.ClientSetNull);
			});

			modelBuilder.Entity<FixtureEvent>(e =>
			{
				e.HasKey(fe => fe.FixtureEventId);
				e.Property(fe => fe.EventComment).HasMaxLength(512).IsRequired(false);
				e.Property(fe => fe.EventDetail).HasMaxLength(64).IsRequired(false);
				e.Property(fe => fe.EventType).HasMaxLength(32);
				e.Property(fe => fe.DateCreatedUtc).HasColumnType("datetime");
				e.Property(fe => fe.DateLastModifiedUtc).HasColumnType("datetime");
				e.HasOne(fe => fe.Fixture).WithMany(f => f.FixtureEvents).HasForeignKey(fe => fe.FixtureId).OnDelete(DeleteBehavior.ClientSetNull);
				e.HasOne(fe => fe.TeamSeason).WithMany(f => f.FixtureEvents).HasForeignKey(fe => fe.TeamSeasonId).OnDelete(DeleteBehavior.ClientSetNull);
			});

			modelBuilder.Entity<TeamSeason>(e =>
			{
				e.HasKey(ts => ts.TeamSeasonId);
				e.Property(ts => ts.TeamName).HasMaxLength(64);
				e.Property(ts => ts.LogoUrl).HasMaxLength(256);
				e.Property(ts => ts.DateCreatedUtc).HasColumnType("datetime");
				e.Property(ts => ts.DateLastModifiedUtc).HasColumnType("datetime");
				e.HasOne(ts => ts.Team).WithMany(t => t.TeamSeasons).HasForeignKey(ts => ts.TeamId).OnDelete(DeleteBehavior.ClientSetNull);
				e.HasOne(ts => ts.CompetitionSeason).WithMany(cs => cs.TeamSeasons).HasForeignKey(ts => ts.CompetitionSeasonId).OnDelete(DeleteBehavior.ClientSetNull);
				e.HasOne(ts => ts.VenueSeason).WithMany(vs => vs.TeamSeasons).HasForeignKey(ts => ts.VenueSeasonId).OnDelete(DeleteBehavior.ClientSetNull);
				e.HasMany(ts => ts.HomeFixtures).WithOne(f => f.HomeTeamSeason).HasForeignKey(f => f.HomeTeamSeasonId).OnDelete(DeleteBehavior.ClientSetNull);
				e.HasMany(ts => ts.AwayFixtures).WithOne(f => f.AwayTeamSeason).HasForeignKey(f => f.AwayTeamSeasonId).OnDelete(DeleteBehavior.ClientSetNull);
				e.HasMany(ts => ts.TeamBoxscores).WithOne(f => f.TeamSeason).HasForeignKey(f => f.TeamSeasonId).OnDelete(DeleteBehavior.ClientSetNull);
				e.HasMany(ts => ts.OppTeamBoxscores).WithOne(f => f.OppTeamSeason).HasForeignKey(f => f.OppTeamSeasonId).OnDelete(DeleteBehavior.ClientSetNull);
			});

			modelBuilder.Entity<PlayerSeason>(e =>
			{
				e.HasKey(ps => ps.PlayerSeasonId);
				e.Property(ps => ps.Position).HasMaxLength(16).IsRequired(false);
				e.Property(ps => ps.DateCreatedUtc).HasColumnType("datetime");
				e.Property(ps => ps.DateLastModifiedUtc).HasColumnType("datetime");
				e.HasOne(ps => ps.Player).WithMany(p => p.PlayerSeasons).HasForeignKey(ps => ps.PlayerId).OnDelete(DeleteBehavior.ClientSetNull);
				e.HasOne(ps => ps.CompetitionSeason).WithMany(cs => cs.PlayerSeasons).HasForeignKey(ps => ps.CompetitionSeasonId).OnDelete(DeleteBehavior.ClientSetNull);
				e.HasMany(ps => ps.FixtureEvents).WithOne(fe => fe.PlayerSeason).HasForeignKey(fe => fe.PlayerSeasonId).OnDelete(DeleteBehavior.ClientSetNull);
				e.HasMany(ps => ps.SecondaryFixtureEvents).WithOne(fe => fe.SecondaryPlayerSeason).HasForeignKey(fe => fe.SecondaryPlayerSeasonId).OnDelete(DeleteBehavior.ClientSetNull);
			});

			modelBuilder.Entity<Transfer>(e =>
			{
				e.HasKey(t => t.TransferId);
				e.Property(t => t.TransferType).HasMaxLength(16).IsRequired(false);
				e.Property(t => t.TransferDate).HasColumnType("date");
				e.Property(t => t.DateCreatedUtc).HasColumnType("datetime");
				e.Property(t => t.DateLastModifiedUtc).HasColumnType("datetime");
				e.HasOne(t => t.Player).WithMany(p => p.Transfers).HasForeignKey(t => t.PlayerId);
			});
		}

		void OnEntityTracked(object sender, EntityTrackedEventArgs e)
		{
			if (!e.FromQuery && e.Entry.State == EntityState.Added && e.Entry.Entity is IEntity entity)
			{
				entity.DateCreatedUtc = DateTime.UtcNow;
				entity.DateLastModifiedUtc = DateTime.UtcNow;
			}
		}

		void OnEntityStateChanged(object sender, EntityStateChangedEventArgs e)
		{
			if (e.NewState == EntityState.Modified && e.Entry.Entity is IEntity entity)
			{
				entity.DateLastModifiedUtc = DateTime.UtcNow;
			}
		}
	}
}
