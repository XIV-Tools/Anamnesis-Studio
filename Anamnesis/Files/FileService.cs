// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Anamnesis;
using Anamnesis.Services;
using Anamnesis.Utils;
using Anamnesis.Windows;
using Microsoft.Win32;

public class FileService : ServiceBase<FileService>
{
	private static readonly Dictionary<Type, string> TypeNameLookup = new Dictionary<Type, string>();
	private static readonly Dictionary<Type, FileFilter> FileTypeFilterLookup = new Dictionary<Type, FileFilter>();

	public static DirectoryInfo StoreDirectory => new DirectoryInfo(ParseToFilePath("%AppData%/AnamnesisStudio/"));
	public static DirectoryInfo CacheDirectory => new DirectoryInfo(ParseToFilePath("%AppData%/AnamnesisStudio/RemoteCache/"));
	public static DirectoryInfo Desktop => new DirectoryInfo(ParseToFilePath("%Desktop%"));
	public static DirectoryInfo DefaultPoseDirectory => new DirectoryInfo(ParseToFilePath(SettingsService.Current.DefaultPoseDirectory));
	public static DirectoryInfo DefaultCharacterDirectory => new DirectoryInfo(ParseToFilePath(SettingsService.Current.DefaultCharacterDirectory));
	public static DirectoryInfo DefaultCameraDirectory => new DirectoryInfo(ParseToFilePath(SettingsService.Current.DefaultCameraShotDirectory));
	public static DirectoryInfo DefaultSceneDirectory => new DirectoryInfo(ParseToFilePath(SettingsService.Current.DefaultSceneDirectory));
	public static DirectoryInfo CMToolPoseSaveDir => new DirectoryInfo(ParseToFilePath("%MyDocuments%/CMTool/Matrix Saves/"));
	public static DirectoryInfo CMToolAppearanceSaveDir => new DirectoryInfo(ParseToFilePath("%MyDocuments%/CMTool/"));
	public static DirectoryInfo FFxivDatCharacterDirectory => new DirectoryInfo(ParseToFilePath("%MyDocuments%/My Games/FINAL FANTASY XIV - A Realm Reborn/"));

	public static async Task Import()
	{
		#pragma warning disable SA1118
		OpenResult result = await Open(
			null,
			new[]
			{
				Desktop,
				DefaultPoseDirectory,
				DefaultCharacterDirectory,
				DefaultCameraDirectory,
				CMToolPoseSaveDir,
				CMToolAppearanceSaveDir,
				FFxivDatCharacterDirectory,
			},
			new[]
			{
				typeof(CameraShotFile),
				typeof(CharacterFile),
				typeof(CmToolAppearanceFile),
				typeof(CmToolAppearanceJsonFile),
				typeof(CmToolGearsetFile),
				typeof(CmToolLegacyAppearanceFile),
				typeof(CmToolPoseFile),
				typeof(DatCharacterFile),
				typeof(PoseFile),
			});

#pragma warning restore SA1118

		if (result.Path != null)
		{
			await Services.Navigation.Navigate(new("ImportFile", result.Path.FullName));
		}
	}

	/*public static async Task Import(string filePath)
	{
		if (file is IUpgradeCharacterFile upgradeFile)
			file = upgradeFile.Upgrade();

		if (file is CharacterFile characterFile)
		{
			await Services.Navigation.Navigate(new("ImportFile", file));
			return;
		}
		else if (file is PoseFile poseFile)
		{
			await Services.Navigation.Navigate(new("ImportPose", file));
			return;
		}
	}*/

	public static Task Export()
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Replaces special folders (%ApplicationData%) with the actual path.
	/// </summary>
	public static string ParseToFilePath(string path)
	{
		foreach (Environment.SpecialFolder? specialFolder in Enum.GetValues(typeof(Environment.SpecialFolder)))
		{
			if (specialFolder == null)
				continue;

			path = path.Replace($"%{specialFolder}%", Environment.GetFolderPath((Environment.SpecialFolder)specialFolder));
		}

		// Special case for "AppData" instead of "ApplicationData"
		path = path.Replace("%AppData%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
		path = path.Replace("%InstallDir%", Directory.GetCurrentDirectory());
		path = path.Replace('/', '\\');
		return path;
	}

	/// <summary>
	/// Replaces special fodler paths with special fodler notation (%appdata%).
	/// </summary>
	public static string ParseFromFilePath(string path)
	{
		// Special case for "AppData" instead of "ApplicationData"
		path = path.Replace(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"%AppData%");

		foreach (Environment.SpecialFolder? specialFolder in Enum.GetValues(typeof(Environment.SpecialFolder)))
		{
			if (specialFolder == null)
				continue;

			string specialPath = Environment.GetFolderPath((Environment.SpecialFolder)specialFolder);

			if (string.IsNullOrEmpty(specialPath))
				continue;

			path = path.Replace(specialPath, $"%{specialFolder}%");
		}

		path = path.Replace('\\', '/');
		return path;
	}

	public static void OpenDirectory(string path)
	{
		string dir = FileService.ParseToFilePath(path);
		Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", $"\"{dir}\"");
	}

	public static void OpenDirectory(DirectoryInfo directory)
	{
		Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", $"\"{directory.FullName}\"");
	}

	public static Task<OpenResult> Open(DirectoryInfo? defaultDirectory, DirectoryInfo shortcut, Type fileType)
	{
		return Open(defaultDirectory, new[] { shortcut }, new[] { fileType });
	}

	public static async Task<OpenResult> Open(DirectoryInfo? defaultDirectory, DirectoryInfo[] shortcuts, Type[] fileTypes)
	{
		OpenResult result = default;

		try
		{
			result.Path = await App.Current.Dispatcher.InvokeAsync<FileInfo?>(() =>
			{
				OpenFileDialog dlg = new OpenFileDialog();

				if (defaultDirectory == null)
				{
					DirectoryInfo? defaultShortcut = null;
					foreach (DirectoryInfo shortcut in shortcuts)
					{
						if (defaultDirectory == null && shortcut.Exists && defaultShortcut == null)
						{
							defaultDirectory = shortcut;
						}
					}
				}

				if (defaultDirectory != null)
					dlg.InitialDirectory = defaultDirectory.FullName;

				if (!Directory.Exists(dlg.InitialDirectory))
				{
					Log.Warning($"Initial directory of open dialog is invalid: {dlg.InitialDirectory}");
					dlg.InitialDirectory = null;
				}

				foreach (DirectoryInfo? shortcut in shortcuts)
				{
					dlg.CustomPlaces.Add(new FileDialogCustomPlace(shortcut.FullName));
				}

				dlg.Filter = ToAnyFilter(fileTypes);

				bool? result = dlg.ShowDialog();

				if (result != true)
					return null;

				return new FileInfo(dlg.FileName);
			});

			if (result.Path == null)
				return result;

			result.File = Load(result.Path, fileTypes);
			return result;
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to open file");
		}

		return result;
	}

	public static FileBase Load(FileInfo info, Type fileType)
	{
		string extension = Path.GetExtension(info.FullName);

		try
		{
			FileBase? file = Activator.CreateInstance(fileType) as FileBase;

			if (file == null)
				throw new Exception($"Failed to create instance of file type: {fileType}");

			using FileStream stream = new FileStream(info.FullName, FileMode.Open);
			return file.Deserialize(stream);
		}
		catch (Exception ex)
		{
			Log.Verbose(ex, $"Attempted to deserialize file: {info} as type: {fileType} failed.");
			throw;
		}
	}

	public static FileBase Load(FileInfo info, Type[] fileTypes)
	{
		string extension = Path.GetExtension(info.FullName);

		Exception? lastException = null;
		foreach (Type fileType in fileTypes)
		{
			FileFilter filter = GetFileTypeFilter(fileType);

			if (filter.Extension == extension)
			{
				try
				{
					FileBase? file = Activator.CreateInstance(fileType) as FileBase;

					if (file == null)
						throw new Exception($"Failed to create instance of file type: {fileType}");

					using FileStream stream = new FileStream(info.FullName, FileMode.Open);
					return file.Deserialize(stream);
				}
				catch (Exception ex)
				{
					Log.Verbose(ex, $"Attempted to deserialize file: {info} as type: {fileType} failed.");
					lastException = ex;
				}
			}
		}

		if (lastException != null)
			throw lastException;

		throw new Exception($"Unrecognised file: {info}");
	}

	public static async Task<SaveResult> Save<T>(DirectoryInfo? defaultDirectory, params DirectoryInfo[] directories)
		where T : FileBase
	{
		SaveResult result = default;
		////result.Path = defaultPath;

		string ext = GetFileTypeFilter(typeof(T)).Extension;

		try
		{
			string typeName = GetFileTypeName(typeof(T));

			result.Path = await App.Current.Dispatcher.InvokeAsync<FileInfo?>(() =>
			{
				SaveFileDialog dlg = new SaveFileDialog();
				dlg.Filter = ToFilter(typeof(T));
				dlg.InitialDirectory = defaultDirectory?.FullName ?? string.Empty;

				if (!Directory.Exists(dlg.InitialDirectory))
				{
					Log.Warning($"Initial directory of save dialog is invalid: {dlg.InitialDirectory}");
					dlg.InitialDirectory = null;
				}

				bool? dlgResult = dlg.ShowDialog();

				if (dlgResult != true)
					return null;

				return new FileInfo(dlg.FileName);
			});

			if (result.Path == null)
				return result;

			if (result.Path.Exists)
			{
				string fileName = Path.GetFileNameWithoutExtension(result.Path.FullName);
				bool? overwrite = await GenericDialog.ShowAsync($"[Library_ReplaceMessage] {fileName}", "[Library_ReplaceTitle]", MessageBoxButton.YesNo);
				if (overwrite != true)
				{
					return await Save<T>(defaultDirectory, directories);
				}
			}

			DirectoryInfo? dir = result.Path.Directory;
			if (dir == null)
				throw new Exception("No directory in save path");

			if (!dir.Exists)
			{
				dir.Create();
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to save file");
		}

		return result;
	}

	/// <summary>
	/// Caches remote file, returns path to file if successful or file exists or original url if unsuccessful. Allows to custom file names to avoid overwrite.
	/// </summary>
	public static async Task<string> CacheRemoteFile(string url, string? filePath)
	{
		if (!CacheDirectory.Exists)
			CacheDirectory.Create();

		Uri uri = new Uri(url);

		string localFile;

		if (filePath != null)
		{
			localFile = CacheDirectory.FullName + filePath;

			string? directoryName = Path.GetDirectoryName(localFile);
			if (directoryName != null && !Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
		}
		else
		{
			localFile = CacheDirectory.FullName + uri.Segments[uri.Segments.Length - 1];
		}

		if (!File.Exists(localFile))
		{
			try
			{
				using HttpClient client = new();
				HttpResponseMessage result = await client.GetAsync(url);
				Stream responseStream = await result.Content.ReadAsStreamAsync();

				using FileStream fileStream = new FileStream(localFile, FileMode.Create, FileAccess.Write);
				responseStream.CopyTo(fileStream);

				Log.Verbose($"Cached remote file: {url}. {fileStream.Position} bytes.");
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to cache data from url: {url}", ex);
			}
		}

		return localFile;
	}

	/// <summary>
	/// Caches remote image, returns path to image if successful or image exists or original url if unsuccessful.
	/// Generates name based on hash of original url to avoid overwriting of images with same name.
	/// </summary>
	public static async Task<string> CacheRemoteImage(string url)
	{
		Uri uri = new Uri(url);
		string imagePath = "ImageCache/" + HashUtility.GetHashString(url) + Path.GetExtension(uri.Segments[uri.Segments.Length - 1]);

		return await CacheRemoteFile(url, imagePath);
	}

	private static string ToAnyFilter(params Type[] types)
	{
		StringBuilder builder = new StringBuilder();
		builder.Append("Any|");

		foreach (Type type in types)
			builder.Append("*" + GetFileTypeFilter(type).Extension + ";");

		foreach (Type type in types)
		{
			builder.Append("|");
			builder.Append(GetFileTypeName(type));
			builder.Append("|");
			builder.Append("*" + GetFileTypeFilter(type).Extension);
		}

		return builder.ToString();
	}

	private static string ToFilter(Type type)
	{
		StringBuilder builder = new StringBuilder();
		builder.Append(GetFileTypeName(type));
		builder.Append("|");
		builder.Append("*" + GetFileTypeFilter(type).Extension);
		return builder.ToString();
	}

	private static List<FileFilter> ToFileFilters(params Type[] types)
	{
		List<FileFilter> results = new();

		foreach (Type type in types)
			results.Add(GetFileTypeFilter(type));

		return results;
	}

	private static string GetFileTypeName(Type fileType)
	{
		string? name;
		if (!TypeNameLookup.TryGetValue(fileType, out name))
		{
			FileBase? file = Activator.CreateInstance(fileType) as FileBase;

			if (file == null)
				throw new Exception($"Failed to create instance of file type: {fileType}");

			name = file.TypeName;
			TypeNameLookup.Add(fileType, name);
		}

		return name;
	}

	private static FileFilter GetFileTypeFilter(Type fileType)
	{
		FileFilter? filter;
		if (!FileTypeFilterLookup.TryGetValue(fileType, out filter))
		{
			FileBase? file = Activator.CreateInstance(fileType) as FileBase;

			if (file == null)
				throw new Exception($"Failed to create instance of file type: {fileType}");

			filter = file.GetFilter();
			FileTypeFilterLookup.Add(fileType, filter);
		}

		return filter;
	}
}

#pragma warning disable SA1201, SA1402
public struct OpenResult
{
	public FileBase? File;
	public FileInfo? Path;

	public DirectoryInfo? Directory => this.Path?.Directory;
}

public struct SaveResult
{
	public FileInfo? Path;

	public DirectoryInfo? Directory => this.Path?.Directory;
}
