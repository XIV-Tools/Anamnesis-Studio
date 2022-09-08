﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Anamnesis.Core.Memory;
using Anamnesis.Files;
using Anamnesis.Memory;
using Anamnesis.Navigation;
using Anamnesis.Services;
using PropertyChanged;
using XivToolsWpf;
using XivToolsWpf.Extensions;

[AddINotifyPropertyChangedInterface]
public class PoseService : ServiceBase<PoseService>
{
	public readonly Dictionary<ActorMemory, SkeletonVisual3d> ActorSkeletons = new();

	private NopHookViewModel? freezeRot1;
	private NopHookViewModel? freezeRot2;
	private NopHookViewModel? freezeRot3;
	private NopHookViewModel? freezeScale1;
	private NopHookViewModel? freezePosition;
	private NopHookViewModel? freezePosition2;
	private NopHookViewModel? freeseScale2;
	private NopHookViewModel? freezePhysics1;
	private NopHookViewModel? freezePhysics2;
	private NopHookViewModel? freezePhysics3;
	private NopHookViewModel? freezeWorldPosition;
	private NopHookViewModel? freezeWorldRotation;
	private NopHookViewModel? freezeGposeTargetPosition1;
	private NopHookViewModel? freezeGposeTargetPosition2;

	private bool isEnabled;
	private Task? writeSkeletonTask;

	public delegate void PoseEvent(bool value);

	public static event PoseEvent? EnabledChanged;
	public static event PoseEvent? FreezeWorldPositionsEnabledChanged;

	public static string? SelectedBoneName { get; set; }

	public bool IsEnabled
	{
		get
		{
			return this.isEnabled;
		}

		set
		{
			if (this.IsEnabled == value)
				return;

			this.SetEnabled(value);
		}
	}

	public bool FreezePhysics
	{
		get
		{
			return this.freezePhysics1?.Enabled ?? false;
		}
		set
		{
			this.freezePhysics1?.SetEnabled(value);
			this.freezePhysics2?.SetEnabled(value);
		}
	}

	public bool FreezePositions
	{
		get
		{
			return this.freezePosition?.Enabled ?? false;
		}
		set
		{
			this.freezePosition?.SetEnabled(value);
			this.freezePosition2?.SetEnabled(value);
		}
	}

	public bool FreezeScale
	{
		get
		{
			return this.freezeScale1?.Enabled ?? false;
		}
		set
		{
			this.freezeScale1?.SetEnabled(value);
			this.freezePhysics3?.SetEnabled(value);
			this.freeseScale2?.SetEnabled(value);
		}
	}

	public bool FreezeRotation
	{
		get
		{
			return this.freezeRot1?.Enabled ?? false;
		}
		set
		{
			this.freezeRot1?.SetEnabled(value);
			this.freezeRot2?.SetEnabled(value);
			this.freezeRot3?.SetEnabled(value);
		}
	}

	public bool WorldPositionNotFrozen => !this.FreezeWorldPosition;

	public bool FreezeWorldPosition
	{
		get
		{
			return this.freezeWorldPosition?.Enabled ?? false;
		}
		set
		{
			this.freezeWorldPosition?.SetEnabled(value);
			this.freezeWorldRotation?.SetEnabled(value);
			this.freezeGposeTargetPosition1?.SetEnabled(value);
			this.freezeGposeTargetPosition2?.SetEnabled(value);
			this.RaisePropertyChanged(nameof(PoseService.FreezeWorldPosition));
			this.RaisePropertyChanged(nameof(PoseService.WorldPositionNotFrozen));
			FreezeWorldPositionsEnabledChanged?.Invoke(this.IsEnabled);
		}
	}

	public bool EnableParenting { get; set; } = true;

	public bool CanEdit { get; set; }

	public async Task<SkeletonVisual3d?> GetSkeleton(ActorMemory actor)
	{
		try
		{
			if (!this.ActorSkeletons.ContainsKey(actor))
			{
				this.ActorSkeletons.Add(actor, new());
				await this.ActorSkeletons[actor].SetActor(actor);

				if (this.writeSkeletonTask == null || this.writeSkeletonTask.IsCompleted)
				{
					this.writeSkeletonTask = Task.Run(this.WriteSkeletonThread);
				}
			}

			return this.ActorSkeletons[actor];
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to bind skeleton to view");
		}

		return null;
	}

	public override async Task Initialize()
	{
		await base.Initialize();

		this.freezePosition = new NopHookViewModel(AddressService.SkeletonFreezePosition, 5);
		this.freezePosition2 = new NopHookViewModel(AddressService.SkeletonFreezePosition2, 5);
		this.freezeRot1 = new NopHookViewModel(AddressService.SkeletonFreezeRotation, 6);
		this.freezeRot2 = new NopHookViewModel(AddressService.SkeletonFreezeRotation2, 6);
		this.freezeRot3 = new NopHookViewModel(AddressService.SkeletonFreezeRotation3, 4);
		this.freezeScale1 = new NopHookViewModel(AddressService.SkeletonFreezeScale, 6);
		this.freeseScale2 = new NopHookViewModel(AddressService.SkeletonFreezeScale2, 6);
		this.freezePhysics1 = new NopHookViewModel(AddressService.SkeletonFreezePhysics, 4);
		this.freezePhysics2 = new NopHookViewModel(AddressService.SkeletonFreezePhysics2, 3);
		this.freezePhysics3 = new NopHookViewModel(AddressService.SkeletonFreezePhysics3, 4);
		this.freezeWorldPosition = new NopHookViewModel(AddressService.WorldPositionFreeze, 16);
		this.freezeWorldRotation = new NopHookViewModel(AddressService.WorldRotationFreeze, 4);

		// We need to keep the MOV in the middle here otherwise we invalidate the ptr, but we patch the rest:
		//     MOVSS dword ptr[RCX + 0xa0],XMM1
		//     MOV RBX,RCX
		//     MOVSS dword ptr[RCX + 0xa4],XMM2
		//     MOVSS dword ptr[RCX + 0xa8],XMM3
		this.freezeGposeTargetPosition1 = new NopHookViewModel(AddressService.GPoseCameraTargetPositionFreeze, 8);
		this.freezeGposeTargetPosition2 = new NopHookViewModel(AddressService.GPoseCameraTargetPositionFreeze + 8 + 3, 16);

		GposeService.GposeStateChanged += this.OnGposeStateChanged;
	}

	public override async Task Shutdown()
	{
		await base.Shutdown();
		this.SetEnabled(false);
		this.FreezeWorldPosition = false;
	}

	public void SetEnabled(bool enabled)
	{
		// Don't try to enable posing unless we are in gpose
		if (enabled && !GposeService.Instance.IsGpose)
			throw new Exception("Attempt to enable posing outside of gpose");

		if (this.isEnabled == enabled)
			return;

		this.isEnabled = enabled;
		this.FreezePhysics = enabled;
		this.FreezeRotation = enabled;
		this.FreezePositions = false;
		this.FreezeScale = false;
		this.EnableParenting = true;

		this.FreezeWorldPosition = enabled;
		AnimationService.Instance.SpeedControlEnabled = enabled;
		AnimationService.Instance.PausePinnedActors(enabled);

		EnabledChanged?.Invoke(enabled);

		this.RaisePropertyChanged(nameof(this.IsEnabled));

		if (enabled)
		{
			NavigationService.Navigate(new("Bones")).Run();
			NavigationService.Navigate(new("Transform")).Run();
		}
		else
		{
			foreach ((ActorMemory actor, SkeletonVisual3d skeleton) in this.ActorSkeletons)
			{
				skeleton.Clear();
			}

			this.ActorSkeletons.Clear();
		}
	}

	private void OnGposeStateChanged(bool isGPose)
	{
		if (!isGPose)
		{
			this.SetEnabled(false);
			this.FreezeWorldPosition = false;
		}
	}

	private async Task WriteSkeletonThread()
	{
		while (this.IsAlive)
		{
			if (Application.Current == null)
				return;

			foreach ((ActorMemory actor, SkeletonVisual3d skeleton) in this.ActorSkeletons)
			{
				if (!actor.IsValid)
					continue;

				skeleton.WriteSkeleton();
			}

			// up to 60 times a second
			await Task.Delay(16);
		}
	}
}
