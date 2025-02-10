using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.Data.Models
{
	public class FileDistributionSet
	{
		[Key]
		public long FileDistributionSetId { get; set; }
		public long FileId { get; set; }
		public long FileDistributionDatabaseOffset { get; set; }
		public bool FileSmallerThanDistributions { get; set; }
		
		public virtual FileRecord File { get; set; }
	}
}
