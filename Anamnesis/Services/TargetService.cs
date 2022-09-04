// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using System;
using System.Threading.Tasks;
using Anamnesis.Core.Memory;
using Anamnesis.Memory;
using Anamnesis.Services;
using PropertyChanged;
using XivToolsWpf.Extensions;

[AddINotifyPropertyChangedInterface]
public class TargetService : ServiceBase<TargetService>
{
	public static readonly int OverworldPlayerTargetOffset = 0x80;
	public static readonly int GPosePlayerTargetOffset = 0x98;

	public delegate void TargetEvent(ActorBasicMemory? actor);

	public static event TargetEvent? TargetChanged;

	public ActorBasicMemory PlayerTarget { get; private set; } = new();

	public static ActorBasicMemory GetTargetedActor()
	{
		Instance.UpdatePlayerTarget();
		return Instance.PlayerTarget;
	}

	public static void SetPlayerTarget(ActorBasicMemory actor)
	{
		if (actor.IsValid)
		{
			Instance.SetPlayerTarget(actor.Address);
		}
	}

	public static ActorBasicMemory? GetPlayerTarget()
	{
		return Instance.PlayerTarget;
	}

	public override async Task Initialize()
	{
		await base.Initialize();
		this.TargetWatcherThread().Run();
	}

	private void UpdatePlayerTarget()
	{
		IntPtr currentPlayerTargetPtr = IntPtr.Zero;

		try
		{
			if (Services.Gpose.IsGpose)
			{
				currentPlayerTargetPtr = MemoryService.Read<IntPtr>(IntPtr.Add(AddressService.PlayerTargetSystem, GPosePlayerTargetOffset));
			}
			else
			{
				currentPlayerTargetPtr = MemoryService.Read<IntPtr>(IntPtr.Add(AddressService.PlayerTargetSystem, OverworldPlayerTargetOffset));
			}
		}
		catch
		{
			// If the memory read fails the target will be 0x0
		}

		try
		{
			if (currentPlayerTargetPtr != this.PlayerTarget.Address)
			{
				if (currentPlayerTargetPtr == IntPtr.Zero)
				{
					this.PlayerTarget.Dispose();
				}
				else
				{
					this.PlayerTarget.SetAddress(currentPlayerTargetPtr);
				}

				this.RaisePropertyChanged(nameof(TargetService.PlayerTarget));
				TargetChanged?.Invoke(this.PlayerTarget);
			}
		}
		catch
		{
			// This section can only fail when FFXIV isn't running (fail to set address) so it should be safe to ignore
		}

		// Tick the actor if it still exists
		if(this.PlayerTarget != null && this.PlayerTarget.Address != IntPtr.Zero)
		{
			try
			{
				this.PlayerTarget.Tick();
			}
			catch
			{
				// Should only fail to tick if the game isn't running
			}
		}
	}

	private void SetPlayerTarget(IntPtr? ptr)
	{
		if (ptr != null && ptr != IntPtr.Zero)
		{
			if (Services.Actor.IsActorInTable((IntPtr)ptr))
			{
				if (GposeService.Instance.IsGpose)
				{
					MemoryService.Write<IntPtr>(IntPtr.Add(AddressService.PlayerTargetSystem, GPosePlayerTargetOffset), (IntPtr)ptr, "Update player target");
				}
				else
				{
					MemoryService.Write<IntPtr>(IntPtr.Add(AddressService.PlayerTargetSystem, OverworldPlayerTargetOffset), (IntPtr)ptr, "Update player target");
				}
			}
		}
	}

	private async Task TargetWatcherThread()
	{
		while (this.IsAlive)
		{
			await Task.Delay(33);

			this.UpdatePlayerTarget();
		}
	}
}