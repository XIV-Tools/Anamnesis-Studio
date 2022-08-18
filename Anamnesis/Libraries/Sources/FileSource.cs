// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Sources;

using Anamnesis.Files;
using Anamnesis.Libraries.Items;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using XivToolsWpf;

public class FileSource<T> : LibrarySourceBase
	where T : FileBase
{
	public FileSource(string name, DirectoryInfo directory, bool recursive = false)
	{
		this.Name = name;
		this.Directory = directory;
		this.Recursive = recursive;

		this.FileFormat = Activator.CreateInstance<T>();
	}

	public string Name { get; init; }
	public DirectoryInfo Directory { get; init; }
	public bool Recursive { get; init; }
	public FileBase FileFormat { get; init; }
	public string Filter => "*" + this.FileFormat.FileExtension;

	public override async Task<List<LibraryPack>> Load()
	{
		await Dispatch.NonUiThread();

		List<LibraryPack> packs = new();

		if (!this.Directory.Exists)
			return packs;

		LibraryPack pack = new();
		pack.Name = this.Name;
		packs.Add(pack);

		await this.GetFiles(pack, this.Directory);

		return packs;
	}

	private async Task GetFiles(ILibraryItemCollection collection, DirectoryInfo directoryInfo)
	{
		FileInfo[] fileInfos = directoryInfo.GetFiles(this.Filter);
		foreach (FileInfo fileInfo in fileInfos)
		{
			collection.Items.Add(this.GetItem(fileInfo));
		}

		if (this.Recursive)
		{
			DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();
			foreach (DirectoryInfo childDirectoryInfo in directoryInfos)
			{
				GroupItem group = new(childDirectoryInfo.Name);
				await this.GetFiles(group, childDirectoryInfo);

				if (group.Items.Count > 0)
				{
					collection.Items.Add(group);
				}
			}
		}
	}

	private ItemBase GetItem(FileInfo fileInfo)
	{
		try
		{
			using FileStream stream = new FileStream(fileInfo.FullName, FileMode.Open);
			FileBase file = this.FileFormat.Deserialize(stream);
			return new FileItem(file, fileInfo.Name);
		}
		catch (Exception ex)
		{
			this.Log.Warning(ex, $"Failed to load file: \"{fileInfo.FullName}\"");
			return new BrokenFileItem(fileInfo);
		}
	}

	public class FileItem : ItemBase
	{
		public FileItem(FileBase file, string name)
		{
			this.File = file;
			this.Name = name;

			this.Desription = file.Description;
			this.Author = file.Author;
			this.Version = file.Version;
		}

		public FileBase File { get; init; }
		public override bool CanLoad => true;
	}

	public class BrokenFileItem : ItemBase
	{
		public BrokenFileItem(FileInfo info)
		{
			this.Path = info;
			this.Name = info.Name;
			this.Desription = "This file could not be loaded";
		}

		public FileInfo Path { get; init; }
		public override bool CanLoad => false;
	}
}