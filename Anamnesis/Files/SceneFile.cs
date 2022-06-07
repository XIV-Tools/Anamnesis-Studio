﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using Anamnesis.Actor;
using Anamnesis.GUI.Dialogs;
using Anamnesis.Memory;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

[Serializable]
public class SceneFile : JsonFileBase
{
	[Flags]
	public enum Mode
	{
		Pose = 1,
		RelativePosition = 2,
		WorldPosition = 4,
		Camera = 8,
		Weather = 16,
		Time = 32,

		All = Pose | RelativePosition | WorldPosition | Camera,
	}

	public override string FileExtension => ".scene";
	public override string TypeName => "Anamnesis Scene";

	public uint Territory { get; set; }
	public ushort Weather { get; set; }
	public byte DayOfMonth { get; set; }
	public long TimeOfDay { get; set; }

	public CameraShotFile? CameraShot { get; set; }

	public string RootActorName { get; set; } = string.Empty;
	public string TargetActorName { get; set; } = string.Empty;
	public Dictionary<string, ActorEntry> ActorEntries { get; set; } = new();

	public async Task Apply(Mode mode)
	{
		if (TargetService.Instance.PinnedActors.Where(i => !i.IsValid || i.Memory?.ModelObject == null).Any())
			throw new Exception("All pinned actors must be valid and have a model");

		// First up, ensure we have the actors mapped
		Dictionary<string, ActorMemory?> actors = new();
		foreach((string name, ActorEntry entry) in this.ActorEntries)
		{
			actors.Add(name, GetPinnedActor(name));
		}

		// TODO: present a dialog allowing the user to set the nicknames of actors to
		// the remainder in the usedNames array.
		int missingCount = 0;
		foreach ((string name, ActorMemory? memory) in actors)
		{
			if (memory == null)
			{
				missingCount++;
			}
		}

		if (missingCount > 0)
		{
			if (await GenericDialog.ShowAsync($"{missingCount} actors not accounted for", "Warning", System.Windows.MessageBoxButton.OKCancel) != true)
			{
				return;
			}
		}

		// Find the root actor
		ActorMemory? rootActor = GetPinnedActor(this.RootActorName);
		ActorEntry rootActorEntry = this.ActorEntries[this.RootActorName];

		if (rootActor == null)
			throw new Exception($"Failed to locate root actor: {this.RootActorName} in scene");

		// Find the target actor
		ActorMemory? targetActor = GetPinnedActor(this.TargetActorName);
		ActorEntry taretActorEntry = this.ActorEntries[this.TargetActorName];

		if (targetActor == null)
			throw new Exception($"Failed to locate target actor: {this.TargetActorName} in scene");

		TargetService.SetPlayerTarget(targetActor);

		if (mode.HasFlag(Mode.Weather))
			TerritoryService.Instance.CurrentWeatherId = this.Weather;

		if (mode.HasFlag(Mode.Time))
		{
			TimeService.Instance.Freeze = true;
			TimeService.Instance.DayOfMonth = this.DayOfMonth;
			TimeService.Instance.TimeOfDay = this.TimeOfDay;
		}

		if (mode.HasFlag(Mode.WorldPosition))
		{
			if (TerritoryService.Instance.CurrentTerritoryId != this.Territory)
				throw new Exception("Could not restore world positions as you are not in the correct territory");

			rootActor.ModelObject!.Transform!.Position = rootActorEntry.Position;
			rootActor.ModelObject!.Transform!.Rotation = rootActorEntry.Rotation;
		}

		Vector rootPosition = rootActor!.ModelObject!.Transform!.Position;
		Quaternion rootRotation = rootActor!.ModelObject!.Transform!.Rotation;
		Quaternion invertedRootRotation = rootRotation;
		invertedRootRotation.Invert();

		foreach ((string name, ActorMemory? actor) in actors)
		{
			if (actor == null)
				continue;

			ActorEntry entry = this.ActorEntries[name];

			if (name != this.RootActorName && mode.HasFlag(Mode.RelativePosition))
			{
				Vector rotatedRelativePosition = rootRotation * entry.Position;
				Quaternion rotatedRotation = rootRotation * entry.Rotation;

				actor.ModelObject!.Transform!.Position = rootPosition + rotatedRelativePosition;
				actor.ModelObject!.Transform!.Rotation = rotatedRotation;
			}

			if (mode.HasFlag(Mode.Pose))
			{
				SkeletonVisual3d skeleton = new();
				await skeleton.SetActor(actor);
				await entry.Pose!.Apply(actor, skeleton, null, PoseFile.Mode.Rotation);
			}
		}

		if (mode.HasFlag(Mode.Camera))
		{
			this.CameraShot!.Apply(CameraService.Instance, targetActor);
		}
	}

	public async Task WriteToFile()
	{
		this.Territory = TerritoryService.Instance.CurrentTerritoryId;
		this.Weather = TerritoryService.Instance.CurrentWeatherId;
		this.DayOfMonth = TimeService.Instance.DayOfMonth;
		this.TimeOfDay = TimeService.Instance.TimeOfDay;

		if(TargetService.Instance.PinnedActors.Where(i => !i.IsValid || i.Memory?.ModelObject == null).Any())
			throw new Exception("All pinned actors must be valid and have a model");

		List<ActorMemory> actors = TargetService.Instance.PinnedActors.Select(i => i.Memory!).ToList();

		ActorMemory rootActor = actors[0];
		this.RootActorName = rootActor.DisplayName;

		Vector rootPosition = rootActor!.ModelObject!.Transform!.Position;
		Quaternion rootRotation = rootActor!.ModelObject!.Transform!.Rotation;

		PinnedActor? targetActor = TargetService.GetPlayerTarget();
		if (targetActor == null || !targetActor.IsValid || targetActor.Memory == null)
			throw new Exception("Targeted actor must be pinned");

		this.TargetActorName = TargetService.Instance.PlayerTarget.DisplayName;

		this.CameraShot = new();
		this.CameraShot.WriteToFile(CameraService.Instance, targetActor.Memory);

		this.ActorEntries = new();

		Quaternion invertedRootRotation = rootRotation;
		invertedRootRotation.Invert();

		foreach (ActorMemory actor in actors)
		{
			Vector actorPosition = actor.ModelObject!.Transform!.Position;
			Vector actorScale = actor.ModelObject!.Transform!.Scale;
			Quaternion actorRotation = actor.ModelObject!.Transform!.Rotation;

			Vector relativePosition = actorPosition - rootPosition;

			Vector rotatedRelativePosition = invertedRootRotation * relativePosition;
			Quaternion rotatedRelativeRotation = actorRotation * invertedRootRotation;

			CharacterFile characterFile = new();
			characterFile.WriteToFile(actor, CharacterFile.SaveModes.All);

			PoseFile poseFile = new();
			SkeletonVisual3d skeleton = new();
			await skeleton.SetActor(actor);
			poseFile.WriteToFile(actor, skeleton, null);

			ActorEntry entry = new();

			if (actor == rootActor)
			{
				entry.Position = rootPosition;
				entry.Rotation = rootRotation;
			}
			else
			{
				entry.Position = rotatedRelativePosition;
				entry.Rotation = rotatedRelativeRotation;
			}

			entry.Scale = actorScale;
			entry.Appearance = characterFile;
			entry.Pose = poseFile;

			if (this.ActorEntries.ContainsKey(actor.DisplayName))
				throw new Exception($"Duplicate actor name: {actor.DisplayName} in scene.");

			this.ActorEntries.Add(actor.DisplayName, entry);
		}
	}

	private static ActorMemory? GetPinnedActor(string name)
	{
		foreach (PinnedActor pinnedActor in TargetService.Instance.PinnedActors)
		{
			ActorMemory? actorMemory = pinnedActor.GetMemory();

			if (actorMemory == null)
				continue;

			if (actorMemory.DisplayName == name)
			{
				return actorMemory;
			}
		}

		return null;
	}

	public class ActorEntry
	{
		public Vector Position { get; set; }
		public Vector Scale { get; set; }
		public Quaternion Rotation { get; set; }
		public CharacterFile? Appearance { get; set; }
		public PoseFile? Pose { get; set; }
	}
}
