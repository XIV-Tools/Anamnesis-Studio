// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using Anamnesis.Files;
using Anamnesis.Memory;
using System.Windows;
using System;
using Anamnesis.Actor;
using XivToolsWpf.Extensions;
using System.Threading.Tasks;
using Anamnesis.Windows;

public partial class ImportPosePanel : PanelBase
{
	public enum Destinations
	{
		Expression,
		Body,
		Selection,
		ScalePack,
		All,
	}

	public PoseFile.Mode Mode { get; set; } = PoseFile.Mode.Scale | PoseFile.Mode.Rotation | PoseFile.Mode.WorldRotation | PoseFile.Mode.WorldScale;
	public Destinations Destination { get; set; } = Destinations.All;
	public PoseFile? File { get; set; }

	public ActorMemory? Actor { get; private set; }

	public override void SetContext(FloatingWindow host, object? context)
	{
		base.SetContext(host, context);

		if (context is not OpenResult openFile)
			return;

		if (openFile.File is not PoseFile file)
			throw new Exception("Import file was not a pose file");

		this.Title = openFile.Path?.Name;
		this.File = file;

		this.Initialize().Run();
	}

	private Task Initialize()
	{
		if (this.Actor == null)
			return Task.CompletedTask;

		return Task.CompletedTask;
	}

	private void OnImportClicked(object sender, RoutedEventArgs e)
	{
		if (this.Actor == null)
			return;

		this.File?.Apply(this.Actor, null, this.Mode).Run();
		this.Close();
	}

	private void OnActorSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
	{
		this.Initialize().Run();
	}

	/*
	private async void OnImportBodyClicked(object sender, RoutedEventArgs e)
	{
		if (this.Skeleton == null)
			return;

		this.Skeleton.SelectHead();
		this.Skeleton.InvertSelection();

		await this.ImportPose(true, PoseFile.Mode.Rotation);
		this.Skeleton.ClearSelection();
	}

	private async void OnImportExpressionClicked(object sender, RoutedEventArgs e)
	{
		if (this.Skeleton == null)
			return;

		if (this.Services.Pose.FreezePositions)
		{
			bool? result = await GenericDialog.ShowLocalizedAsync("Pose_WarningExpresionPositions", "Common_Confirm", MessageBoxButton.OKCancel);

			if (result != true)
			{
				return;
			}
		}

		this.Skeleton.SelectHead();
		await this.ImportPose(true, PoseFile.Mode.Rotation | PoseFile.Mode.Scale);
		this.Skeleton.ClearSelection();
	}

	private async Task ImportPose(bool selectionOnly, PoseFile.Mode mode)
	{
		try
		{
			if (this.Actor == null || this.Skeleton == null)
				return;

			PoseService.Instance.SetEnabled(true);
			PoseService.Instance.FreezeScale |= mode.HasFlag(PoseFile.Mode.Scale);
			PoseService.Instance.FreezeRotation |= mode.HasFlag(PoseFile.Mode.Rotation);
			PoseService.Instance.FreezePositions |= mode.HasFlag(PoseFile.Mode.Position);

			Type[] types = new[]
			{
				typeof(PoseFile),
				typeof(CmToolPoseFile),
			};

			Shortcut[] shortcuts = new[]
			{
				FileService.DefaultPoseDirectory,
				FileService.StandardPoseDirectory,
				FileService.CMToolPoseSaveDir,
			};

			OpenResult result = await FileService.Open(lastLoadDir, shortcuts, types);

			if (result.File == null)
				return;

			lastLoadDir = result.Directory;

			if (result.File is CmToolPoseFile legacyFile)
				result.File = legacyFile.Upgrade(this.Actor.Customize?.Race ?? ActorCustomizeMemory.Races.Hyur);

			if (result.File is PoseFile poseFile)
			{
				HashSet<string>? bones = null;
				if (selectionOnly)
				{
					bones = new HashSet<string>();

					foreach ((string name, BoneVisual3d visual) in this.Skeleton.Bones)
					{
						if (this.Skeleton.GetIsBoneSelected(visual))
						{
							bones.Add(name);
						}
					}
				}

				await poseFile.Apply(this.Actor, this.Skeleton, bones, mode);
			}
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Failed to load pose file");
		}
	}*/
}
