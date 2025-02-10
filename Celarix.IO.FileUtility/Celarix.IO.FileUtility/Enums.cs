using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileUtility;

internal enum RunOption
{
	Invalid,
	SequentialRename,
	MultifolderSequentialRename,
	MoveFilesUpOneLevel,
	SortFilesIntoSeries,
	ProcessLinkList,
	FixExtensions,
	SortIntoFoldersByExtension
}

internal enum SeriesType
{
	Invalid,
	Picture,
	Screenshot
}