using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Celarix.IO.FileAnalysis.Data.Models;

namespace Celarix.IO.FileAnalysis.Data
{
	public class AnalysisContext : DbContext
	{
		public string DbPath { get; }
		
		public DbSet<FileRecord> Files { get; set; }
		public DbSet<FolderRecord> Folders { get; set; }
		public DbSet<FileDistributionSet> FileDistributionSets { get; set; }
		
		public AnalysisContext()
		{
			var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			DbPath = Path.Join(path, "fileAnalysis.db");
		}
		
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite($"Data Source={DbPath}");
		}
		
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<FileRecord>()
				.HasOne(f => f.ParentFolder)
				.WithMany(f => f.Files)
				.HasForeignKey(f => f.ParentFolderId);
			
			modelBuilder.Entity<FolderRecord>()
				.HasOne(f => f.ParentFolder)
				.WithMany(f => f.Folders)
				.HasForeignKey(f => f.ParentFolderId);
			
			modelBuilder.Entity<FileDistributionSet>()
				.HasOne(d => d.File)
				.WithMany(f => f.DistributionSets)
				.HasForeignKey(d => d.FileId);
		}
	}
}
