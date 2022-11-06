// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Sources;
using Anamnesis.Files;
using Anamnesis.Libraries.Items;
using Anamnesis.Services;
using Anamnesis.Tags;
using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using XivToolsWpf;

internal class FileSource : LibrarySourceBase
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

	public override IconChar Icon => IconChar.FolderTree;
	public override string Name => LocalizationService.GetString("Library_FileSource");

	public DirectoryInfo[] Directories { get; init; }
	public bool IsUpdateAvailable { get; set; } = false;

	public static bool IsSupportedFileType(FileInfo file)
	{
		return SupportedFileExtensions.Contains(file.Extension);
	}

	protected override async Task Load(bool refresh)
	{
		// Dont hit the file system on the main thread.
		await Dispatch.NonUiThread();

		foreach (DirectoryInfo info in this.Directories)
		{
			if (!info.Exists)
				continue;

			Pack pack = new(info.FullName, this);
			pack.Name = info.Name;
			pack.Description = info.FullName;
			await this.AddPack(pack);
			this.Populate(pack, info, new());
		}
	}

	protected DirectoryEntry AddDirectory(DirectoryEntry directory, DirectoryInfo info, TagCollection tags)
	{
		DirectoryEntry entry = new();
		entry.Parent = directory;
		entry.Name = info.Name;
		entry.Description = info.FullName;
		directory.AddEntry(entry);

		tags.Add(info.Name);

		this.Populate(entry, info, tags);

		foreach (EntryBase entryBase in entry.Entries)
		{
			entry.Tags.AddRange(entryBase.Tags);
		}

		return entry;
	}

	protected void Populate(DirectoryEntry entry, DirectoryInfo info, TagCollection tags)
	{
		foreach (DirectoryInfo directoryInfo in info.GetDirectories())
		{
			this.AddDirectory(entry, directoryInfo, new TagCollection(tags));
		}

		foreach (FileInfo file in info.GetFiles("*.*", SearchOption.TopDirectoryOnly))
		{
			if (!FileSource.IsSupportedFileType(file))
				continue;

			try
			{
				entry.AddEntry(new FileItem(file, tags));
			}
			catch (Exception ex)
			{
				this.Log.Warning(ex, $"Failed to load pack file: {file.FullName}");
				entry.AddEntry(new BrokenFileItem(file));
			}
		}
	}

	public class FileItem : ItemEntry
	{
		private readonly bool hasThumbnail;
		private readonly IconChar icon;

		public FileItem(FileInfo info, TagCollection tags)
		{
			this.Info = info;

			// load the file to get info from it, but don't keep it
			// loaded.
			FileBase fileBase = FileService.Load(info, SupportedFiles);

			this.Name = Path.GetFileNameWithoutExtension(info.Name);
			this.Author = fileBase.Author;
			this.Description = fileBase.Description;
			this.Version = fileBase.Version;
			this.Type = fileBase.GetType();
			this.hasThumbnail = !string.IsNullOrEmpty(fileBase.Base64Image);

			this.Tags.AddRange(tags);

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

		public FileInfo Info { get; init; }
		public Type Type { get; init; }
		public override IconChar Icon => this.icon;
		public override IconChar IconBack => IconChar.File;
		public override bool CanOpen => true;

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

		public override Task Open()
		{
			FileBase fileBase = FileService.Load(this.Info, SupportedFiles);
			return FileService.Import(fileBase);
		}
	}

	public class BrokenFileItem : ItemEntry
	{
		public BrokenFileItem(FileInfo info, params string[] tags)
		{
			this.Description = $"Failed to load file: {info.FullName}";
			this.Name = Path.GetFileNameWithoutExtension(info.Name);

			foreach (string tag in tags)
			{
				this.Tags.Add(tag);
			}
		}

		public override IconChar Icon => IconChar.Warning;
		public override IconChar IconBack => IconChar.File;
		public override bool CanOpen => false;

		public override bool IsType(LibraryFilter.Types type)
		{
			return true;
		}

		public override Task Open()
		{
			throw new NotSupportedException();
		}
	}
}
