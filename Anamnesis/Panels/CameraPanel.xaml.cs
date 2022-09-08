// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using Anamnesis.Files;
using Anamnesis.Memory;
using System;
using System.IO;
using System.Windows;

public partial class CameraPanel : PanelBase
{
	private static DirectoryInfo? lastLoadDir;
	private static DirectoryInfo? lastSaveDir;

	private async void OnImportCamera(object sender, RoutedEventArgs e)
	{
		ActorBasicMemory? targetActor = this.Services.Target.PlayerTarget;
		if (targetActor == null || !targetActor.IsValid)
			return;
		ActorMemory actorMemory = new ActorMemory();
		actorMemory.SetAddress(targetActor.Address);

		try
		{
			Shortcut[]? shortcuts = new[]
			{
				FileService.DefaultCameraDirectory,
			};

			Type[] types = new[]
			{
				typeof(CameraShotFile),
			};

			OpenResult result = await FileService.Open(lastLoadDir, shortcuts, types);

			if (result.File == null)
				return;

			lastLoadDir = result.Directory;

			if (result.File is CameraShotFile camFile)
			{
				camFile.Apply(CameraService.Instance, actorMemory);
			}
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Failed to load camera");
		}
	}

	private async void OnExportCamera(object sender, RoutedEventArgs e)
	{
		ActorBasicMemory? targetActor = this.Services.Target.PlayerTarget;
		if (targetActor == null || !targetActor.IsValid)
			return;
		ActorMemory actorMemory = new ActorMemory();
		actorMemory.SetAddress(targetActor.Address);

		SaveResult result = await FileService.Save<CameraShotFile>(lastSaveDir, FileService.DefaultCameraDirectory);

		if (result.Path == null)
			return;

		lastSaveDir = result.Directory;

		CameraShotFile file = new CameraShotFile();
		file.WriteToFile(CameraService.Instance, actorMemory);

		using FileStream stream = new FileStream(result.Path.FullName, FileMode.Create);
		file.Serialize(stream);
	}
}
