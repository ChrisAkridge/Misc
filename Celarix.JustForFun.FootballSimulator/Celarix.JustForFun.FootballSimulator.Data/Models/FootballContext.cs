using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Celarix.JustForFun.FootballSimulator.Data.Models
{
    public class FootballContext : DbContext
    {
        public string DbPath { get; }
        
        public DbSet<Team> Teams { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<PlayerRosterPosition> PlayerRosterPositions { get; set; }
        public DbSet<Stadium> Stadiums { get; set; }
        public DbSet<SeasonRecord> SeasonRecords { get; set; }
        public DbSet<GameRecord> GameRecords { get; set; }
        public DbSet<TeamGameRecord> TeamGameRecords { get; set; }
        public DbSet<QuarterBoxScore> QuarterBoxScores { get; set; }
        public DbSet<TeamDriveRecord> TeamDriveRecords { get; set; }
        public DbSet<SimulatorSettings> SimulatorSettings { get; set; }
        public DbSet<PhysicsParam> PhysicsParams { get; set; }
        public DbSet<Summary> Summaries { get; set; }

        public FootballContext()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            DbPath = Path.Join(path, "footballSimulator.db");
        }

        /// <summary>
        ///     <para>
        ///         Override this method to configure the database (and other options) to be used for this context.
        ///         This method is called for each instance of the context that is created.
        ///         The base implementation does nothing.
        ///     </para>
        ///     <para>
        ///         In situations where an instance of <see cref="T:Microsoft.EntityFrameworkCore.DbContextOptions" /> may or may not have been passed
        ///         to the constructor, you can use <see cref="P:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.IsConfigured" /> to determine if
        ///         the options have already been set, and skip some or all of the logic in
        ///         <see cref="M:Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder)" />.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-dbcontext">DbContext lifetime, configuration, and initialization</see>
        ///     for more information.
        /// </remarks>
        /// <param name="optionsBuilder">
        ///     A builder used to create or modify options for this context. Databases (and other extensions)
        ///     typically define extension methods on this object that allow you to configure the context.
        /// </param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={DbPath}");
        }

        /// <summary>
        ///     Override this method to further configure the model that was discovered by convention from the entity types
        ///     exposed in <see cref="T:Microsoft.EntityFrameworkCore.DbSet`1" /> properties on your derived context. The resulting model may be cached
        ///     and re-used for subsequent instances of your derived context.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If a model is explicitly set on the options for this context (via <see cref="M:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel)" />)
        ///         then this method will not be run. However, it will still run when creating a compiled model.
        ///     </para>
        ///     <para>
        ///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
        ///         examples.
        ///     </para>
        /// </remarks>
        /// <param name="modelBuilder">
        ///     The builder being used to construct the model for this context. Databases (and other extensions) typically
        ///     define extension methods on this object that allow you to configure aspects of the model that are specific
        ///     to a given database.
        /// </param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Team>()
                .Property(t => t.Conference)
                .HasConversion<int>();

            modelBuilder.Entity<Team>()
                .Property(t => t.Division)
                .HasConversion<int>();

            modelBuilder.Entity<Team>()
                .Property(t => t.Disposition)
                .HasConversion<int>();

            modelBuilder.Entity<GameRecord>()
                .Property(r => r.GameType)
                .HasConversion<int>();

            modelBuilder.Entity<GameRecord>()
                .Property(r => r.WeatherAtKickoff)
                .HasConversion<int>();

            modelBuilder.Entity<QuarterBoxScore>()
                .Property(s => s.Team)
                .HasConversion<int>();

            modelBuilder.Entity<TeamDriveRecord>()
                .Property(r => r.Team)
                .HasConversion<int>();

            modelBuilder.Entity<TeamDriveRecord>()
                .Property(r => r.Result)
                .HasConversion<int>();

            modelBuilder.Entity<TeamGameRecord>()
                .Property(r => r.Team)
                .HasConversion<int>();
        }
    }
}
