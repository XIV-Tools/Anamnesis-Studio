// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using Anamnesis.Files;
using System;
using System.IO;

public class SceneService : ServiceBase<SceneService>
{
	public SceneOptionsValues SceneOptions { get; init; } = new();

	public string CurrentSceneName { get; set; } = "Untitled Scene";
	public SceneFile CurrentScene { get; set; } = new();
	public FileInfo? CurrentScenePath { get; set; } = null;
	public bool UnsavedChanges { get; set; } = false; // TODO

	public async void Save()
	{
		if (this.CurrentScenePath == null)
		{
			this.SaveAs();
			return;
		}

		await this.CurrentScene.WriteToFile();

		using FileStream stream = new FileStream(this.CurrentScenePath.FullName, FileMode.Create);
		this.CurrentScene.Serialize(stream);
	}

	public async void SaveAs()
	{
		try
		{
			SaveResult result = await FileService.Save<SceneFile>(null, FileService.DefaultSceneDirectory);

			if (result.Path == null)
				return;

			this.CurrentScenePath = result.Path;
			this.CurrentSceneName = result.Path.Name;
			this.Save();
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to save scene");
		}
	}

	public async void Open()
	{
		try
		{
			OpenResult result = await FileService.Open(null, FileService.DefaultSceneDirectory, typeof(SceneFile));

			if (result.File == null)
				return;

			this.CurrentScene = (SceneFile)result.File;
			this.CurrentScenePath = result.Path;
			this.CurrentSceneName = result.Path?.Name ?? "Untitled Scene";

			await this.CurrentScene.Apply(this.SceneOptions.GetMode());
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to load scene");
		}
	}

	public class SceneOptionsValues
	{
		public bool RelativePositions { get; set; } = true;
		public bool WorldPositions { get; set; } = true;
		public bool Poses { get; set; } = true;
		public bool Camera { get; set; } = true;
		public bool Weather { get; set; } = true;
		public bool Time { get; set; } = true;

		public SceneFile.Mode GetMode()
		{
			SceneFile.Mode mode = 0;

			if (this.RelativePositions)
				mode |= SceneFile.Mode.RelativePosition;

			if (this.WorldPositions)
				mode |= SceneFile.Mode.WorldPosition;

			if (this.Poses)
				mode |= SceneFile.Mode.Pose;

			if (this.Camera)
				mode |= SceneFile.Mode.Camera;

			if (this.Weather)
				mode |= SceneFile.Mode.Weather;

			if (this.Time)
				mode |= SceneFile.Mode.Time;

			return mode;
		}
	}
}
