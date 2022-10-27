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
	public const int OverworldPlayerTargetOffset = 0x80;
	public const int GPosePlayerTargetOffset = 0x98;

	private readonly ActorBasicMemory playerTarget = new();

	public delegate void TargetEvent(ActorBasicMemory? actor);

	public static event TargetEvent? TargetChanged;

	public ActorBasicMemory? TargetedActor
	{
		get
		{
			if (!this.playerTarget.IsValid || this.playerTarget.ObjectKind == ActorTypes.None)
				return null;

			return this.playerTarget;
		}
	}

	public void SetTarget(ActorBasicMemory actor)
	{
		if (actor.IsValid)
		{
			if (GposeService.Instance.IsGpose)
			{
				MemoryService.Write<IntPtr>(IntPtr.Add(AddressService.PlayerTargetSystem, GPosePlayerTargetOffset), actor.Address, "Update player target");
			}
			else
			{
				MemoryService.Write<IntPtr>(IntPtr.Add(AddressService.PlayerTargetSystem, OverworldPlayerTargetOffset), actor.Address, "Update player target");
			}
		}
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
			if (currentPlayerTargetPtr != this.playerTarget.Address)
			{
				if (currentPlayerTargetPtr == IntPtr.Zero)
				{
					this.playerTarget.Dispose();
				}
				else
				{
					this.playerTarget.SetAddress(currentPlayerTargetPtr);
				}

				this.RaisePropertyChanged(nameof(TargetService.TargetedActor));
				TargetChanged?.Invoke(this.TargetedActor);
			}
		}
		catch
		{
			// This section can only fail when FFXIV isn't running (fail to set address) so it should be safe to ignore
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