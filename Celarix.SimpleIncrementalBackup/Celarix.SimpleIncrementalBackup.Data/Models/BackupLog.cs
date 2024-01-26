using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.SimpleIncrementalBackup.Data.Models
{
    public sealed class BackupLog
    {
        public int BackupLogId { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public int NewFileCount { get; set; }
        public int DeletedFileCount { get; set; }
        public long SizeDelta { get; set; }
    }
}
