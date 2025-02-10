using Celarix.IO.FileUtility;
using Celarix.IO.FileUtility.Commands;
using Celarix.IO.FileUtility.Logic;
using CommandLine;

var runOption = ParseArguments(args, out var parseResult);

switch (runOption)
{
	case RunOption.SequentialRename:
		RunSequentialRename(parseResult);
		break;
	case RunOption.MultifolderSequentialRename:
		RunMultifolderSequentialRename(parseResult);
		break;
	case RunOption.MoveFilesUpOneLevel:
		RunMoveFilesUpOneLevel(parseResult);
		break;
	case RunOption.SortFilesIntoSeries:
		RunSortFilesIntoSeries(parseResult);
		break;
	case RunOption.ProcessLinkList:
		RunProcessLinkList(parseResult);
		break;
	case RunOption.FixExtensions:
		RunFixExtensions(parseResult);
		break;
	case RunOption.SortIntoFoldersByExtension:
		RunSortFilesIntoFoldersByExtension(parseResult);
		break;
	case RunOption.Invalid:
		Console.WriteLine("Invalid command line arguments:");

		foreach (var error in (IEnumerable<Error>)parseResult!)
		{
			Console.WriteLine("\t" + error.Tag);
		}

		return;
	default:
		throw new ArgumentOutOfRangeException(nameof(runOption));
}

static RunOption ParseArguments(IEnumerable<string> args, out object? parseResult)
{
	var result = RunOption.Invalid;
	object? parsed = null;

	Parser.Default.ParseArguments<SequentialRenameCommand,
			MultifolderSequentialRenameCommand,
			MoveFilesUpOneLevelCommand,
			SortFilesIntoSeriesCommand,
			ProcessLinkListCommand,
			FixExtensionsCommand,
			SortIntoFoldersByExtensionCommand>(args)
		.WithParsed<SequentialRenameCommand>(opts =>
		{
			result = RunOption.SequentialRename;
			parsed = opts;
		})
		.WithParsed<MultifolderSequentialRenameCommand>(opts =>
		{
			result = RunOption.MultifolderSequentialRename;
			parsed = opts;
		})
		.WithParsed<MoveFilesUpOneLevelCommand>(opts =>
		{
			result = RunOption.MoveFilesUpOneLevel;
			parsed = opts;
		})
		.WithParsed<SortFilesIntoSeriesCommand>(opts =>
		{
			result = RunOption.SortFilesIntoSeries;
			parsed = opts;
		})
		.WithParsed<ProcessLinkListCommand>(opts =>
		{
			result = RunOption.ProcessLinkList;
			parsed = opts;
		})
		.WithParsed<FixExtensionsCommand>(opts =>
		{
			result = RunOption.FixExtensions;
			parsed = opts;
		})
		.WithParsed<SortIntoFoldersByExtensionCommand>(opts =>
		{
			result = RunOption.SortIntoFoldersByExtension;
			parsed = opts;
		})
		.WithNotParsed(errs =>
		{
			result = RunOption.Invalid;
			parsed = errs;
		});
	
	parseResult = parsed;
	return result;
}

static void RunSequentialRename(object? parseResult)
{
	var options = (SequentialRenameCommand)parseResult!;

	if (!options.Validate()) { return; }
	
	SequentialRenamer.SequentialRename(options);
}

static void RunMultifolderSequentialRename(object? parseResult)
{
	var options = (MultifolderSequentialRenameCommand)parseResult!;

	if (!options.Validate()) { return; }

	MultifolderSequentialRenamer.SequentiallyRename(options);
}

static void RunProcessLinkList(object? parseResult)
{
	var options = (ProcessLinkListCommand)parseResult!;

	if (!options.Validate()) { return; }

	LinkListProcessor.ProcessLinkList(options);
}

static void RunSortFilesIntoSeries(object? parseResult)
{
	var options = (SortFilesIntoSeriesCommand)parseResult!;

	if (!options.Validate()) { return; }

	FilesIntoSeriesSorter.SortFilesIntoSeries(options);
}

static void RunMoveFilesUpOneLevel(object? parseResult)
{
	var options = (MoveFilesUpOneLevelCommand)parseResult!;

	if (!options.Validate()) { return; }

	OneLevelUpFileMover.MoveFilesAndFoldersOneLevelUp(options);
}

static void RunFixExtensions(object? parseResult)
{
	var options = (FixExtensionsCommand)parseResult!;

	if (!options.Validate()) { return; }

	ExtensionFixer.FixExtensions(options);
}

static void RunSortFilesIntoFoldersByExtension(object? parseResult)
{
	var options = (SortIntoFoldersByExtensionCommand)parseResult!;

	if (!options.Validate()) { return; }

	FileSorterByExtension.SortFilesIntoFoldersByExtension(options);
}