// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Anamnesis.Actor;
using Anamnesis.Core.Memory;
using Anamnesis.Memory;
using PropertyChanged;
using static Anamnesis.Memory.ActorBasicMemory;

[AddINotifyPropertyChangedInterface]
public class ActorService : ServiceBase<ActorService>
{
	private const int TickDelay = 10;
	private const int ActorTableSize = 424;
	private const int GPoseIndexStart = 200;
	private const int GPoseIndexEnd = 244;
	private const int OverworldPlayerIndex = 0;
	private const int GPosePlayerIndex = 201;

	private readonly IntPtr[] actorTable = new IntPtr[ActorTableSize];

	public ReadOnlyCollection<IntPtr> ActorTable => Array.AsReadOnly(this.actorTable);

	/// <summary>
	/// Gets the first actor in the actor table that has a valid object kind.
	/// </summary>
	public ActorBasicMemory? GetDefaultActor()
	{
		List<ActorBasicMemory> actors = this.GetAllActors();
		foreach (ActorBasicMemory actor in actors)
		{
			if (actor.ObjectKind == ActorTypes.None)
				continue;

			if (!actor.IsValid)
				continue;

			return actor;
		}

		return null;
	}

	public async Task<bool> RefreshActor(ActorMemory actor)
	{
		if (Services.DalamudIpc.IsConnected)
		{
			// TODO: either batch this together into one request, or only send values that have _actually_ changed.
			bool result = true;
			result &= await Services.DalamudIpc.ChangeGear(actor.Address, Dalamud.EquipIndex.Head, actor.Equipment?.Head);
			result &= await Services.DalamudIpc.ChangeGear(actor.Address, Dalamud.EquipIndex.Chest, actor.Equipment?.Body);
			result &= await Services.DalamudIpc.ChangeGear(actor.Address, Dalamud.EquipIndex.Hands, actor.Equipment?.Hands);
			result &= await Services.DalamudIpc.ChangeGear(actor.Address, Dalamud.EquipIndex.Legs, actor.Equipment?.Legs);
			result &= await Services.DalamudIpc.ChangeGear(actor.Address, Dalamud.EquipIndex.Feet, actor.Equipment?.Feet);
			result &= await Services.DalamudIpc.ChangeGear(actor.Address, Dalamud.EquipIndex.Earring, actor.Equipment?.Ears);
			result &= await Services.DalamudIpc.ChangeGear(actor.Address, Dalamud.EquipIndex.Necklace, actor.Equipment?.Neck);
			result &= await Services.DalamudIpc.ChangeGear(actor.Address, Dalamud.EquipIndex.Bracelet, actor.Equipment?.Neck);
			result &= await Services.DalamudIpc.ChangeGear(actor.Address, Dalamud.EquipIndex.RingRight, actor.Equipment?.RightRing);
			result &= await Services.DalamudIpc.ChangeGear(actor.Address, Dalamud.EquipIndex.RingLeft, actor.Equipment?.LeftRing);
			return result;
		}
		else
		{
			if (PoseService.Instance.IsEnabled)
				return false;

			if (PoseService.Instance.FreezeWorldPosition)
				return false;

			if (!actor.IsValid)
				return false;

			if (actor.IsGPoseActor)
				return false;

			await Task.Delay(16);

			if (actor.ObjectKind == ActorTypes.Player)
			{
				actor.ObjectKind = ActorTypes.BattleNpc;
				actor.RenderMode = RenderModes.Unload;
				await Task.Delay(75);
				actor.RenderMode = RenderModes.Draw;
				await Task.Delay(75);
				actor.ObjectKind = ActorTypes.Player;
				actor.RenderMode = RenderModes.Draw;
			}
			else
			{
				actor.RenderMode = RenderModes.Unload;
				await Task.Delay(75);
				actor.RenderMode = RenderModes.Draw;
			}

			await Task.Delay(150);
			return true;
		}
	}

	public int GetActorTableIndex(IntPtr pointer, bool refresh = false)
	{
		if (pointer == IntPtr.Zero)
			return -1;

		if (refresh)
			this.UpdateActorTable();

		return Array.IndexOf(this.actorTable, pointer);
	}

	public bool IsActorInTable(IntPtr ptr, bool refresh = false)
	{
		return this.GetActorTableIndex(ptr, refresh) != -1;
	}

	public bool IsActorInTable(MemoryBase memory, bool refresh = false) => this.IsActorInTable(memory.Address, refresh);

	public bool IsGPoseActor(int objectIndex) => objectIndex >= GPoseIndexStart && objectIndex < GPoseIndexEnd;

	public bool IsGPoseActor(IntPtr actorAddress)
	{
		int objectIndex = this.GetActorTableIndex(actorAddress);

		if (objectIndex == -1)
			return false;

		return this.IsGPoseActor(objectIndex);
	}

	public bool IsOverworldActor(int objectIndex) => !this.IsGPoseActor(objectIndex);
	public bool IsOverworldActor(IntPtr actorAddress) => !this.IsGPoseActor(actorAddress);

	public bool IsLocalOverworldPlayer(int objectIndex) => objectIndex == OverworldPlayerIndex;
	public bool IsLocalOverworldPlayer(IntPtr actorAddress)
	{
		int objectIndex = this.GetActorTableIndex(actorAddress);

		if (objectIndex == -1)
			return false;

		return this.IsLocalOverworldPlayer(objectIndex);
	}

	public bool IsLocalGPosePlayer(int objectIndex) => objectIndex == GPosePlayerIndex;
	public bool IsLocalGPosePlayer(IntPtr actorAddress)
	{
		int objectIndex = this.GetActorTableIndex(actorAddress);

		if (objectIndex == -1)
			return false;

		return this.IsLocalGPosePlayer(objectIndex);
	}

	public bool IsLocalPlayer(int objectIndex) => this.IsLocalOverworldPlayer(objectIndex) || this.IsLocalGPosePlayer(objectIndex);
	public bool IsLocalPlayer(IntPtr actorAddress) => this.IsLocalOverworldPlayer(actorAddress) || this.IsLocalGPosePlayer(actorAddress);

	public List<ActorBasicMemory> GetAllActors(bool refresh = false)
	{
		if (refresh)
			this.UpdateActorTable();

		List<ActorBasicMemory> results = new();

		foreach (var ptr in this.actorTable)
		{
			if (ptr == IntPtr.Zero)
				continue;

			try
			{
				ActorBasicMemory actor = new();
				actor.SetAddress(ptr);
				results.Add(actor);
			}
			catch (Exception ex)
			{
				Log.Warning(ex, $"Failed to create Actor Basic View Model for address: {ptr}");
			}
		}

		return results;
	}

	public void ForceRefresh()
	{
		this.UpdateActorTable();
	}

	public override async Task Initialize()
	{
		await base.Initialize();
	}

	public override Task Start()
	{
		this.UpdateActorTable();

		_ = Task.Run(this.TickTask);
		return base.Start();
	}

	public override async Task Shutdown()
	{
		await base.Shutdown();
	}

	private async Task TickTask()
	{
		while (this.IsAlive)
		{
			await Task.Delay(TickDelay);

			this.ForceRefresh();
		}
	}

	private void UpdateActorTable()
	{
		lock (this.actorTable)
		{
			for (int i = 0; i < ActorTableSize; i++)
			{
				IntPtr ptr = MemoryService.ReadPtr(AddressService.ActorTable + (i * 8));
				this.actorTable[i] = ptr;
			}
		}

		this.RaisePropertyChanged(nameof(this.ActorTable));
	}
}
