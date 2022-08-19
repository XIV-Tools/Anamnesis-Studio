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
using System.Text;
using System.Threading.Tasks;
using XivToolsWpf;
using static System.Net.WebRequestMethods;

public class FileSource : LibrarySourceBase
{
	protected static readonly Type[] SupportedFiles = new Type[]
	{
		typeof(PoseFile),
		typeof(CharacterFile),
		typeof(CameraShotFile),
		typeof(SceneFile),
	};

	protected static readonly HashSet<string> SupportedFileExtensions = new()
	{
		new PoseFile().FileExtension,
		new CharacterFile().FileExtension,
		new CameraShotFile().FileExtension,
		new SceneFile().FileExtension,
	};

	public FileSource(params DirectoryInfo[] directories)
	{
		this.Directories = directories;
	}

	public FileSource(params string[] paths)
	{
		List<DirectoryInfo> dirs = new();
		foreach (string path in paths)
		{
			dirs.Add(new(FileService.ParseToFilePath(path)));
		}

		this.Directories = dirs.ToArray();
	}

	protected FileSource()
	{
		this.Directories = new DirectoryInfo[0];
	}

	public override IconChar Icon => IconChar.Folder;
	public override string Name => LocalizationService.GetString("Library_FileSource");

	public DirectoryInfo[] Directories { get; init; }

	protected override async Task Load(bool force)
	{
		// Dont hit the file system on the main thread.
		await Dispatch.NonUiThread();

		foreach (DirectoryInfo directoryInfo in this.Directories)
		{
			if (!directoryInfo.Exists)
				continue;

			FileInfo[] fileInfos = directoryInfo.GetFiles("pack.json", SearchOption.AllDirectories);
			foreach (FileInfo fileInfo in fileInfos)
			{
				if (fileInfo.Directory == null)
					continue;

				try
				{
					PackDefinitionFile definition = SerializerService.DeserializeFile<PackDefinitionFile>(fileInfo.FullName);
					Pack pack = new Pack(definition, this);
					this.AddPack(pack);
					await this.GetFiles(pack, definition, fileInfo.Directory);
				}
				catch (Exception ex)
				{
					this.Log.Error(ex, "Failed to load pack definition");
				}
			}
		}
	}

	protected virtual Task GetFiles(Pack pack, PackDefinitionFile definition, DirectoryInfo rootDirectory)
	{
		DirectoryInfo packContentsDirectory = rootDirectory;
		if (definition.Directory != null)
		{
			packContentsDirectory = new(packContentsDirectory.FullName + '/' + definition.Directory.Replace('\\', '/').Trim('/'));

			if (!packContentsDirectory.Exists)
			{
				throw new Exception($"Failed to get pack directory: {packContentsDirectory}");
			}
		}

		return this.GetFiles(pack, packContentsDirectory);
	}

	protected virtual Task GetFiles(Pack pack, DirectoryInfo directory)
	{
		FileInfo[] files = directory.GetFiles("*.*", SearchOption.AllDirectories);

		HashSet<string> allTags = new();

		foreach (FileInfo fileInfo in files)
		{
			if (fileInfo.DirectoryName == null)
				continue;

			string[] folders = fileInfo.DirectoryName.Replace(directory.FullName, string.Empty).Split('\\', StringSplitOptions.RemoveEmptyEntries);
			foreach (string folder in folders)
			{
				allTags.Add(folder);
			}

			try
			{
				pack.AddItem(new FileItem(fileInfo, folders));
			}
			catch (Exception ex)
			{
				this.Log.Warning(ex, $"Failed to load pack file: {fileInfo.FullName}");
				pack.AddItem(new BrokenFileItem(fileInfo));
			}
		}

		foreach (string tag in allTags)
		{
			pack.AvailableTags.Add(tag);
		}

		return Task.CompletedTask;
	}

	public class FileItem : ItemBase
	{
		public FileItem(FileInfo info, string[] tags)
		{
			this.Info = info;

			// load the file to get info from it, but don't keep it
			// loaded.
			FileBase fileBase = FileService.Load(info, SupportedFiles);

			// TODO: Image!
			this.Name = info.Name;
			this.Author = fileBase.Author;
			this.Desription = fileBase.Description;
			this.Version = fileBase.Version;
			this.Type = fileBase.GetType();
		}

		public override bool CanLoad => true;
		public FileInfo Info { get; init; }
		public Type Type { get; init; }
	}

	public class BrokenFileItem : ItemBase
	{
		public BrokenFileItem(FileInfo info)
		{
			this.Desription = $"Failed to load file: {info.FullName}";
			this.Name = info.Name;
		}

		public override bool CanLoad => false;
	}
}