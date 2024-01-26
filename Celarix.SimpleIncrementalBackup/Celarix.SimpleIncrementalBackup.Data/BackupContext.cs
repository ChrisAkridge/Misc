using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.SimpleIncrementalBackup.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Celarix.SimpleIncrementalBackup.Data
{
    public sealed class BackupContext(string databasePath) : DbContext
    {
        private readonly string databasePath = databasePath;

        public DbSet<FileEntry> FileEntries { get; set; }
        public DbSet<BackupLog> BackupLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={databasePath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FileEntry>()
                .HasKey(f => f.FileEntryId);

            modelBuilder.Entity<BackupLog>()
                .HasKey(f => f.BackupLogId);
        }
    }
}
