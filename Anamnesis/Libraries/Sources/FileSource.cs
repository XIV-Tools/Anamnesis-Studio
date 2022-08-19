// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Sources;

using Anamnesis.Files;
using Anamnesis.Libraries.Items;
using Anamnesis.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using XivToolsWpf;

public class FileSource : LibrarySourceBase
{
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

	public DirectoryInfo[] Directories { get; init; }

	protected override async Task Load()
	{
		await Dispatch.NonUiThread();

		foreach (DirectoryInfo directoryInfo in this.Directories)
		{
			if (!directoryInfo.Exists)
				continue;

			FileInfo[] fileInfos = directoryInfo.GetFiles("pack.json", SearchOption.AllDirectories);
			foreach (FileInfo fileInfo in fileInfos)
			{
				try
				{
					PackDefinitionFile definition = SerializerService.DeserializeFile<PackDefinitionFile>(fileInfo.FullName);
					this.AddPack(new Pack(definition));
				}
				catch(Exception ex)
				{
					this.Log.Error(ex, "Failed to load pack definition");
				}
			}
		}
	}
}