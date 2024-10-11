using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.WeatherForecastHistory.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace Celarix.JustForFun.WeatherForecastHistory.Data
{
	public sealed class HistoryContext(string databasePath) : DbContext
	{
		public required DbSet<Forecast> Forecasts { get; set; }
		public required DbSet<ForecastDay> ForecastDays { get; set; }
		public required DbSet<ForecastSource> ForecastSources { get; set; }
		public required DbSet<Observation> Observations { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite($"Data Source={databasePath}");
		}
		
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Forecast>()
				.HasKey(f => f.ForecastId);
			modelBuilder.Entity<Forecast>()
				.HasOne(f => f.ForecastSource)
				.WithMany(fs => fs.Forecasts)
				.HasForeignKey(f => f.ForecastSourceId);
			
			modelBuilder.Entity<ForecastDay>()
				.HasKey(fd => fd.ForecastDayId);
			modelBuilder.Entity<ForecastDay>()
				.HasOne(fd => fd.Forecast)
				.WithMany(f => f.ForecastDays)
				.HasForeignKey(fd => fd.ForecastId);
			
			modelBuilder.Entity<ForecastSource>()
				.HasKey(fs => fs.ForecastSourceId);
			
			modelBuilder.Entity<Observation>()
				.HasKey(o => o.ObservationId);
		}
	}
}
