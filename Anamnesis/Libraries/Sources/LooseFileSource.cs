// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Sources;
using Anamnesis.Files;
using Anamnesis.Libraries.Items;
using Anamnesis.Serialization;
using Anamnesis.Services;
using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using XivToolsWpf;
using static Anamnesis.Libraries.Sources.FileSource;

/// <summary>
/// Unlike the FileSource, the LooseFileSource keeps the hierarchy of files, and operates
/// more like a file browser.
/// </summary>
internal class LooseFileSource : LibrarySourceBase
{
	public LooseFileSource(params DirectoryInfo[] directories)
	{
		this.Directories = directories;
	}

	public LooseFileSource(params string[] paths)
	{
		List<DirectoryInfo> dirs = new();
		foreach (string path in paths)
		{
			dirs.Add(new(FileService.ParseToFilePath(path)));
		}

		this.Directories = dirs.ToArray();
	}

	protected LooseFileSource()
	{
		this.Directories = new DirectoryInfo[0];
	}

	public override IconChar Icon => IconChar.FolderTree;
	public override string Name => LocalizationService.GetString("Library_FileSource");

	public DirectoryInfo[] Directories { get; init; }
	public bool IsUpdateAvailable { get; set; } = false;

	protected override async Task Load()
	{
		Pack pack = new Pack("looseFiles", this);
		pack.Name = LocalizationService.GetString("Library_LooseFiles");
		await this.AddPack(pack);
		await this.Load(pack);
	}

	protected override Task Update(Pack pack)
	{
		throw new NotSupportedException();
	}

	protected override async Task Load(Pack pack)
	{
		// Dont hit the file system on the main thread.
		await Dispatch.NonUiThread();

		foreach (DirectoryInfo directoryInfo in this.Directories)
		{
			if (!directoryInfo.Exists)
				continue;

			this.AddDirectory(pack, directoryInfo);
		}
	}

	private DirectoryEntry AddDirectory(DirectoryEntry directory, DirectoryInfo info)
	{
		DirectoryEntry entry = new();
		entry.Parent = directory;
		entry.Name = info.Name;
		directory.AddEntry(entry);

		foreach (DirectoryInfo directoryInfo in info.GetDirectories())
		{
			this.AddDirectory(entry, directoryInfo);
		}

		foreach (FileInfo file in info.GetFiles("*.*", SearchOption.TopDirectoryOnly))
		{
			if (!FileSource.IsSupportedFileType(file))
				continue;

			try
			{
				entry.AddEntry(new FileItem(file));
			}
			catch (Exception ex)
			{
				this.Log.Warning(ex, $"Failed to load pack file: {file.FullName}");
				directory.AddEntry(new BrokenFileItem(file));
			}
		}

		return entry;
	}
}
