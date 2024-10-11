using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Celarix.SimpleIncrementalBackup.Data.Models
{
	[Index(nameof(Path))]
    public sealed class FileEntry
    {
        public int FileEntryId { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTimeOffset FSDateCreated { get; set; }
        public DateTimeOffset FSDateModified { get; set; }
        public DateTimeOffset BackupLastUpdated { get; set; }
        public bool FileDeletedOnLastBackup { get; set; }
    }
}
