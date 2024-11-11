using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.Data.Models
{
	public class FolderRecord
	{
		[Key]
		public long FolderId { get; set; }
		public long ParentFolderId { get; set; }
		public string Name { get; set; }
		public DateTimeOffset Created { get; set; }
		public DateTimeOffset Modified { get; set; }
		
		public virtual FolderRecord ParentFolder { get; set; }
		public virtual ICollection<FileRecord> Files { get; set; }
		public virtual ICollection<FolderRecord> Folders { get; set; }
	}
}
