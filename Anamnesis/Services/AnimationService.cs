﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anamnesis.Core.Memory;
using Anamnesis.Memory;
using Newtonsoft.Json.Linq;
using PropertyChanged;

[AddINotifyPropertyChangedInterface]
public class AnimationService : ServiceBase<AnimationService>
{
	private const int TickDelay = 1000;
	private const ushort DrawWeaponAnimationId = 34;
	private const ushort IdleAnimationId = 3;

	private readonly HashSet<ActorMemory> overriddenActors = new();
	private readonly HashSet<ActorMemory> pausedActors = new();

	private NopHookViewModel? animationSpeedHook;
	private bool speedControlEnabled = false;

	public bool SpeedControlEnabled
	{
		get => this.speedControlEnabled;
		set
		{
			if (this.speedControlEnabled != value)
				this.SetSpeedControlEnabled(value && GposeService.Instance.IsGpose);
		}
	}

	public override Task Start()
	{
		GposeService.GposeStateChanged += this.OnGposeStateChanged;

		this.animationSpeedHook = new NopHookViewModel(AddressService.AnimationSpeedPatch, 0x9);

		this.OnGposeStateChanged(GposeService.Instance.IsGpose);

		_ = Task.Run(this.CheckThread);

		return base.Start();
	}

	public override async Task Shutdown()
	{
		GposeService.GposeStateChanged -= this.OnGposeStateChanged;

		this.SpeedControlEnabled = false;

		this.ResetOverriddenActors();
		this.PausePinnedActors(false);

		await base.Shutdown();
	}

	public bool ApplyAnimationOverride(ActorMemory memory, ushort? animationId, bool interrupt)
	{
		if (!memory.IsValid)
			return false;

		if (!memory.CanAnimate)
			return false;

		this.ApplyBaseAnimationInternal(memory, animationId, interrupt, ActorMemory.CharacterModes.AnimLock, 0);
		this.overriddenActors.Add(memory);

		return true;
	}

	public async Task<bool> BlendAnimation(ActorMemory memory, ushort animationId)
	{
		if (!memory.IsValid)
			return false;

		if(memory.Animation!.BlendLocked)
			return false;

		if (!memory.CanAnimate)
			return false;

		memory.Animation!.BlendLocked = true;

		ushort oldAnim = memory.Animation!.BaseOverride;
		memory.Animation!.BaseOverride = animationId;
		await Task.Delay(66);
		memory.Animation!.BaseOverride = oldAnim;

		memory.Animation!.BlendLocked = false;

		return true;
	}

	public void ResetAnimationOverride(ActorMemory memory)
	{
		if (!memory.IsValid)
			return;

		this.ApplyBaseAnimationInternal(memory, 0, true, ActorMemory.CharacterModes.Normal, 0);

		AnimationMemory animation = memory.Animation!;

		animation.LipsOverride = 0;
		animation.LinkSpeeds = true;
		animation.Speeds![(int)AnimationMemory.AnimationSlots.FullBody].Value = 1.0f;

		this.overriddenActors.Remove(memory);
	}

	public void ApplyIdle(ActorMemory memory) => this.ApplyAnimationOverride(memory, IdleAnimationId, true);

	public void DrawWeapon(ActorMemory memory) => this.ApplyAnimationOverride(memory, DrawWeaponAnimationId, true);

	public void PausePinnedActors(bool pause)
	{
		this.SpeedControlEnabled = true;

		if (!this.SpeedControlEnabled)
			return;

		foreach (PinnedActor pinned in TargetService.Instance.PinnedActors)
		{
			if (!pinned.IsValid)
				continue;

			ActorMemory? actor = pinned.GetMemory();
			if (actor == null || !actor.IsValid)
				continue;

			this.PauseActor(actor, pause);
		}
	}

	public void PauseActor(ActorMemory actor, bool pause)
	{
		if (!actor.IsValid)
			return;

		if (actor.Animation == null)
			return;

		actor.Animation.LinkSpeeds = true;
		actor.Animation.SetSpeed(AnimationMemory.AnimationSlots.FullBody, pause ? 0.0f : 1.0f);

		if (pause)
		{
			this.pausedActors.Add(actor);
		}
		else
		{
			this.pausedActors.Remove(actor);
		}
	}

	private async Task CheckThread()
	{
		while (this.IsAlive)
		{
			await Task.Delay(TickDelay);

			this.CleanupInvalidActors();
		}
	}

	private void ApplyBaseAnimationInternal(ActorMemory memory, ushort? animationId, bool interrupt, ActorMemory.CharacterModes? mode, byte? modeInput)
	{
		if (animationId != null && memory.Animation!.BaseOverride != animationId)
		{
			if (animationId < GameDataService.ActionTimelines.RowCount)
			{
				memory.Animation!.BaseOverride = (ushort)animationId;
			}
		}

		// Always set the input before the mode
		if (modeInput != null && memory.CharacterModeInput != modeInput)
		{
			MemoryService.Write(memory.GetAddressOfProperty(nameof(ActorMemory.CharacterModeInput)), modeInput, "Animation Mode Input Override");
		}

		if (mode != null && memory.CharacterMode != mode)
		{
			MemoryService.Write(memory.GetAddressOfProperty(nameof(ActorMemory.CharacterModeRaw)), mode, "Animation Mode Override");
		}

		if (interrupt)
		{
			memory.Animation!.AnimationIds![(int)AnimationMemory.AnimationSlots.FullBody].Value = 0;
		}

		memory.Tick();
	}

	private void SetSpeedControlEnabled(bool enabled)
	{
		if (this.speedControlEnabled == enabled)
			return;

		if (enabled)
		{
			this.animationSpeedHook?.SetEnabled(true);
		}
		else
		{
			this.animationSpeedHook?.SetEnabled(false);
		}

		this.speedControlEnabled = enabled;
	}

	private void OnGposeStateChanged(bool isGPose)
	{
		if (!isGPose)
			this.SpeedControlEnabled = false;
	}

	private void ResetOverriddenActors()
	{
		foreach (var actor in this.overriddenActors.ToList())
		{
			if (actor.IsValid && actor.IsAnimationOverridden)
			{
				this.ResetAnimationOverride(actor);
			}
		}
	}

	private void CleanupInvalidActors()
	{
		var stale = this.overriddenActors.Where(actor => !actor.IsValid).ToList();
		foreach (var actorRef in stale)
		{
			this.overriddenActors.Remove(actorRef);
		}
	}
}
