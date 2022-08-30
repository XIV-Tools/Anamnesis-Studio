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
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using XivToolsWpf;

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
	public bool IsUpdateAvailable { get; set; } = false;

	public static bool IsSupportedFileType(FileInfo file)
	{
		return SupportedFileExtensions.Contains(file.Extension);
	}

	protected override Task Load()
	{
		return this.Load(null);
	}

	protected override async Task Load(Pack? pack)
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

					if (pack == null)
					{
						Pack newPack = new Pack(fileInfo.FullName, definition, this);
						await this.AddPack(newPack);
						await this.GetFiles(newPack, definition, fileInfo.Directory);
					}
					else if (pack.Id == fileInfo.FullName)
					{
						await this.GetFiles(pack, definition, fileInfo.Directory);
						return;
					}
				}
				catch (Exception ex)
				{
					this.Log.Error(ex, "Failed to load pack definition");
				}
			}
		}
	}

	protected override Task Update(Pack pack)
	{
		// File soruces are always live
		return Task.CompletedTask;
	}

	protected DirectoryInfo GetPackDirectory(PackDefinitionFile definition, DirectoryInfo rootDirectory)
	{
		DirectoryInfo packContentsDirectory = rootDirectory;
		if (definition.Directory != null)
		{
			return new(packContentsDirectory.FullName + '/' + definition.Directory.Replace('\\', '/').Trim('/'));
		}

		return rootDirectory;
	}

	protected virtual async Task GetFiles(Pack pack, PackDefinitionFile definition, DirectoryInfo rootDirectory)
	{
		await Dispatch.NonUiThread();

		DirectoryInfo packContentsDirectory = this.GetPackDirectory(definition, rootDirectory);
		if (!packContentsDirectory.Exists)
			throw new Exception($"Failed to get pack directory: {packContentsDirectory}");

		await this.GetFiles(pack, packContentsDirectory);
	}

	protected virtual async Task GetFiles(Pack pack, DirectoryInfo directory)
	{
		await Dispatch.NonUiThread();

		FileInfo[] files = directory.GetFiles("*.*", SearchOption.AllDirectories);

		HashSet<string> allTags = new();

		foreach (FileInfo fileInfo in files)
		{
			if (fileInfo.DirectoryName == null)
				continue;

			if (!IsSupportedFileType(fileInfo))
				continue;

			string[] folderTags = fileInfo.DirectoryName.Replace(directory.FullName, string.Empty).Split('\\', StringSplitOptions.RemoveEmptyEntries);
			foreach (string folderTag in folderTags)
			{
				allTags.Add(folderTag);
			}

			try
			{
				pack.AddEntry(new FileItem(fileInfo, folderTags));
			}
			catch (Exception ex)
			{
				this.Log.Warning(ex, $"Failed to load pack file: {fileInfo.FullName}");
				pack.AddEntry(new BrokenFileItem(fileInfo, folderTags));
			}
		}

		foreach (string tag in allTags)
		{
			pack.AvailableTags.Add(tag);
		}
	}

	public class FileItem : ItemEntry
	{
		private readonly bool hasThumbnail;
		private readonly IconChar icon;

		public FileItem(FileInfo info, params string[] tags)
		{
			this.Info = info;

			// load the file to get info from it, but don't keep it
			// loaded.
			FileBase fileBase = FileService.Load(info, SupportedFiles);

			this.Name = this.Info.Name;
			this.Author = fileBase.Author;
			this.Description = fileBase.Description;
			this.Version = fileBase.Version;
			this.Type = fileBase.GetType();
			this.hasThumbnail = !string.IsNullOrEmpty(fileBase.Base64Image);

			foreach (string tag in tags)
			{
				this.Tags.Add(tag);
			}

			if (this.Type == typeof(CharacterFile))
			{
				this.icon = IconChar.User;
			}
			else if (this.Type == typeof(PoseFile))
			{
				this.icon = IconChar.Running;
			}
			else if (this.Type == typeof(CameraShotFile))
			{
				this.icon = IconChar.Camera;
			}
			else if (this.Type == typeof(SceneFile))
			{
				this.icon = IconChar.Users;
			}
			else
			{
				this.icon = IconChar.Question;
			}
		}

		public override bool CanLoad => true;
		public FileInfo Info { get; init; }
		public Type Type { get; init; }
		public override IconChar Icon => this.icon;

		public override ImageSource? Thumbnail
		{
			get
			{
				if (!this.hasThumbnail)
					return null;

				FileBase fileBase = FileService.Load(this.Info, SupportedFiles);
				return fileBase.ImageSource;
			}
		}

		public override bool IsType(LibraryFilter.Types type)
		{
			if (type == LibraryFilter.Types.Characters && this.Type == typeof(CharacterFile))
			{
				return true;
			}
			else if (type == LibraryFilter.Types.Poses && this.Type == typeof(PoseFile))
			{
				return true;
			}
			else if (type == LibraryFilter.Types.Scenes && this.Type == typeof(SceneFile))
			{
				return true;
			}

			return false;
		}
	}

	public class BrokenFileItem : ItemEntry
	{
		public BrokenFileItem(FileInfo info, params string[] tags)
		{
			this.Description = $"Failed to load file: {info.FullName}";
			this.Name = info.Name;

			foreach (string tag in tags)
			{
				this.Tags.Add(tag);
			}
		}

		public override bool CanLoad => false;
		public override IconChar Icon => IconChar.Warning;

		public override bool IsType(LibraryFilter.Types type)
		{
			return false;
		}
	}
}