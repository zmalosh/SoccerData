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

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Country>(e =>
			{
				e.HasKey(c => c.CountryId);
				e.Property(c => c.DateCreatedUtc).HasColumnType("datetime");
				e.Property(c => c.DateLastModifiedUtc).HasColumnType("datetime");
				e.Property(c => c.CountryAbbr).HasMaxLength(2);
			});

			modelBuilder.Entity<Competition>(e =>
			{
				e.HasKey(c => c.CompetitionId);
				e.Property(c => c.DateCreatedUtc).HasColumnType("datetime");
				e.Property(c => c.DateLastModifiedUtc).HasColumnType("datetime");
				e.HasOne(c => c.Country).WithMany(c => c.Competitions).HasForeignKey(c => c.CountryId).OnDelete(DeleteBehavior.ClientNoAction);
			});

			modelBuilder.Entity<CompetitionSeason>(e =>
			{
				e.HasKey(cs => cs.CompetitionSeasonId);
				e.Property(cs => cs.DateCreatedUtc).HasColumnType("datetime");
				e.Property(cs => cs.DateLastModifiedUtc).HasColumnType("datetime");
				e.HasOne(cs => cs.Competition).WithMany(c => c.CompetitionSeasons).HasForeignKey(cs => cs.CompetitionId).OnDelete(DeleteBehavior.ClientNoAction);
			});

			modelBuilder.Entity<CompetitionSeasonRound>(e =>
			{
				e.HasKey(csr => csr.CompetitionSeasonRoundId);
				e.Property(csr => csr.DateCreatedUtc).HasColumnType("datetime");
				e.Property(csr => csr.DateLastModifiedUtc).HasColumnType("datetime");
				e.HasOne(csr => csr.CompetitionSeason).WithMany(cs => cs.CompetitionSeasonRounds).HasForeignKey(csr => csr.CompetitionSeasonId).OnDelete(DeleteBehavior.ClientNoAction);
			});

			modelBuilder.Entity<Venue>(e =>
			{
				e.HasKey(v => v.VenueId);
				e.Property(v => v.DateCreatedUtc).HasColumnType("datetime");
				e.Property(v => v.DateLastModifiedUtc).HasColumnType("datetime");
			});

			modelBuilder.Entity<VenueSeason>(e =>
			{
				e.HasKey(vs => vs.VenueSeasonId);
				e.Property(vs => vs.DateCreatedUtc).HasColumnType("datetime");
				e.Property(vs => vs.DateLastModifiedUtc).HasColumnType("datetime");
				e.HasOne(vs => vs.Venue).WithMany(v => v.VenueSeasons).HasForeignKey(vs => vs.VenueId).OnDelete(DeleteBehavior.ClientNoAction);
			});

			modelBuilder.Entity<Team>(e =>
			{
				e.HasKey(t => t.TeamId);
				e.Property(t => t.DateCreatedUtc).HasColumnType("datetime");
				e.Property(t => t.DateLastModifiedUtc).HasColumnType("datetime");
				e.HasOne(t => t.Country).WithMany(c => c.Teams).HasForeignKey(t => t.CountryId).OnDelete(DeleteBehavior.ClientNoAction);
			});

			modelBuilder.Entity<Fixture>(e =>
			{
				e.HasKey(f => f.FixtureId);
				e.Property(f => f.GameTimeUtc).HasColumnType("datetime");
				e.Property(f => f.DateCreatedUtc).HasColumnType("datetime");
				e.Property(f => f.DateLastModifiedUtc).HasColumnType("datetime");
				e.HasOne(f => f.CompetitionSeason).WithMany(cs => cs.Fixtures).HasForeignKey(f => f.CompetitionSeasonId).OnDelete(DeleteBehavior.ClientNoAction);
				e.HasOne(f => f.CompetitionSeasonRound).WithMany(csr => csr.Fixtures).HasForeignKey(f => f.CompetitionSeasonRoundId).OnDelete(DeleteBehavior.ClientNoAction);
				e.HasOne(f => f.VenueSeason).WithMany(vs => vs.Fixtures).HasForeignKey(f => f.VenueSeasonId).OnDelete(DeleteBehavior.ClientNoAction);
			});

			modelBuilder.Entity<TeamSeason>(e =>
			{
				e.HasKey(ts => ts.TeamSeasonId);
				e.Property(ts => ts.DateCreatedUtc).HasColumnType("datetime");
				e.Property(ts => ts.DateLastModifiedUtc).HasColumnType("datetime");
				e.HasOne(ts => ts.Team).WithMany(t => t.TeamSeasons).HasForeignKey(ts => ts.TeamId).OnDelete(DeleteBehavior.ClientNoAction);
				e.HasOne(ts => ts.CompetitionSeason).WithMany(cs => cs.TeamSeasons).HasForeignKey(ts => ts.CompetitionSeasonId).OnDelete(DeleteBehavior.ClientNoAction);
				e.HasOne(ts => ts.VenueSeason).WithMany(vs => vs.TeamSeasons).HasForeignKey(ts => ts.VenueSeasonId).OnDelete(DeleteBehavior.ClientNoAction);
				e.HasMany(ts => ts.HomeFixtures).WithOne(f => f.HomeTeamSeason).HasForeignKey(f => f.HomeTeamSeasonId).OnDelete(DeleteBehavior.ClientNoAction);
				e.HasMany(ts => ts.AwayFixtures).WithOne(f => f.AwayTeamSeason).HasForeignKey(f => f.AwayTeamSeasonId).OnDelete(DeleteBehavior.ClientNoAction);
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
