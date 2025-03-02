﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;
using System.Threading.Tasks;
using Anamnesis.Files;
using Anamnesis.Services;
using Anamnesis.Utils;
using PropertyChanged;

public class ActorMemory : ActorBasicMemory
{
	private readonly FuncQueue refreshQueue;
	private readonly FuncQueue backupQueue;
	private bool isRestoringBackup = false;

	private CharacterFile? gposeCharacterBackup;
	private CharacterFile? originalCharacterBackup;

	public ActorMemory()
	{
		this.refreshQueue = new(this.RefreshAsync, 50);
		this.backupQueue = new(this.BackupAsync, 250);
	}

	public enum BackupModes
	{
		Gpose,
		Original,
	}

	public enum CharacterModes : byte
	{
		None = 0,
		Normal = 1,
		EmoteLoop = 3,
		Mounted = 4,
		AnimLock = 8,
		Carrying = 9,
		InPositionLoop = 11,
		Performance = 16,
	}

	[Flags]
	public enum CharacterFlagDefs : byte
	{
		None = 0,
		WeaponsVisible = 1 << 0,
		WeaponsDrawn = 1 << 1,
		VisorToggled = 1 << 3,
	}

	[Bind(0x008D)] public byte SubKind { get => this.GetValue<byte>(); set => this.SetValue(value); }
	[Bind(0x00C4)] public float Scale { get => this.GetValue<float>(); set => this.SetValue(value); }
	[Bind(0x0100, BindFlags.Pointer)] public ActorModelMemory? ModelObject { get => this.GetValue<ActorModelMemory?>(); set => this.SetValue(value); }
	[Bind(0x01B4, BindFlags.ActorRefresh)] public int ModelType { get => this.GetValue<int>(); set => this.SetValue(value); }
	[Bind(0x01E0)] public byte ClassJob { get => this.GetValue<byte>(); set => this.SetValue(value); }
	[Bind(0x0670, BindFlags.Pointer)] public ActorMemory? Mount { get => this.GetValue<ActorMemory?>(); set => this.SetValue(value); }
	[Bind(0x0678)] public ushort MountId { get => this.GetValue<ushort>(); set => this.SetValue(value); }
	[Bind(0x06D8, BindFlags.Pointer)] public ActorMemory? Companion { get => this.GetValue<ActorMemory?>(); set => this.SetValue(value); }
	[Bind(0x06F8)] public WeaponMemory? MainHand { get => this.GetValue<WeaponMemory?>(); set => this.SetValue(value); }
	[Bind(0x0760)] public WeaponMemory? OffHand { get => this.GetValue<WeaponMemory?>(); set => this.SetValue(value); }
	[Bind(0x0830)] public ActorEquipmentMemory? Equipment { get => this.GetValue<ActorEquipmentMemory?>(); set => this.SetValue(value); }
	[Bind(0x0858)] public ActorCustomizeMemory? Customize { get => this.GetValue<ActorCustomizeMemory?>(); set => this.SetValue(value); }
	[Bind(0x0876, BindFlags.ActorRefresh)] public bool HatHidden { get => this.GetValue<bool>(); set => this.SetValue(value); }
	[Bind(0x0877, BindFlags.ActorRefresh)] public CharacterFlagDefs CharacterFlags { get => this.GetValue<CharacterFlagDefs>(); set => this.SetValue(value); }
	[Bind(0x0888, BindFlags.Pointer)] public ActorMemory? Ornament { get => this.GetValue<ActorMemory?>(); set => this.SetValue(value); }
	[Bind(0x0890)] public ushort OrnamentId { get => this.GetValue<ushort>(); set => this.SetValue(value); }
	[Bind(0x0930)] public AnimationMemory? Animation { get => this.GetValue<AnimationMemory?>(); set => this.SetValue(value); }
	[Bind(0x1244)] public bool IsMotionEnabled { get => this.GetValue<bool>(); set => this.SetValue(value); }
	[Bind(0x1A48)] public float Transparency { get => this.GetValue<float>(); set => this.SetValue(value); }
	[Bind(0x1B24)] public byte Voice { get => this.GetValue<byte>(); set => this.SetValue(value); }
	[Bind(0x1B26)] public byte CharacterModeRaw { get => this.GetValue<byte>(); set => this.SetValue(value); }
	[Bind(0x1B27)] public byte CharacterModeInput { get => this.GetValue<byte>(); set => this.SetValue(value); }
	[Bind(0x1B44)] public byte AttachmentPoint { get => this.GetValue<byte>(); set => this.SetValue(value); }

	public History History { get; private set; } = new();

	public bool AutomaticRefreshEnabled { get; set; } = true;
	public bool IsRefreshing { get; set; } = false;
	public bool PendingRefresh => this.refreshQueue.Pending;

	public bool IsHuman => this.ModelObject != null && this.ModelObject.IsHuman;

	[DependsOn(nameof(ModelType))]
	public bool IsChocobo => this.ModelType == 1;

	[DependsOn(nameof(CharacterModeRaw))]
	public CharacterModes CharacterMode
	{
		get
		{
			return (CharacterModes)this.CharacterModeRaw;
		}
		set
		{
			this.CharacterModeRaw = (byte)value;
		}
	}

	[DependsOn(nameof(MountId), nameof(Mount))]
	public bool IsMounted => this.MountId != 0 && this.Mount != null;

	[DependsOn(nameof(OrnamentId), nameof(Ornament))]
	public bool IsUsingOrnament => this.Ornament != null && this.OrnamentId != 0;

	[DependsOn(nameof(Companion))]
	public bool HasCompanion => this.Companion != null;

	[DependsOn(nameof(CharacterFlags))]
	public bool VisorToggled
	{
		get => this.CharacterFlags.HasFlag(CharacterFlagDefs.VisorToggled);
		set
		{
			if (value)
			{
				this.CharacterFlags |= CharacterFlagDefs.VisorToggled;
			}
			else
			{
				this.CharacterFlags &= ~CharacterFlagDefs.VisorToggled;
			}
		}
	}

	[DependsOn(nameof(ObjectIndex), nameof(CharacterMode))]
	public bool CanAnimate => (this.CharacterMode == CharacterModes.Normal || this.CharacterMode == CharacterModes.AnimLock) || !ActorService.Instance.IsLocalOverworldPlayer(this.ObjectIndex);

	[DependsOn(nameof(CharacterMode))]
	public bool IsAnimationOverridden => this.CharacterMode == CharacterModes.AnimLock;

	/// <summary>
	/// Refresh the actor to force the game to load any changed values for appearance.
	/// </summary>
	public void Refresh()
	{
		this.refreshQueue.Invoke();
	}

	public override void Tick()
	{
		this.History.Tick();

		// Since writing is immadiate from poperties, we don't want to tick (read) anything
		// during a refresh.
		if (this.IsRefreshing || this.PendingRefresh)
			return;

		base.Tick();
	}

	/// <summary>
	/// Refresh the actor to force the game to load any changed values for appearance.
	/// </summary>
	public async Task RefreshAsync()
	{
		if (this.IsRefreshing)
			return;

		if (this.Address == IntPtr.Zero)
			return;

		try
		{
			Log.Information($"Attempting actor refresh for actor address: {this.Address}");

			this.IsRefreshing = true;

			if(await ActorService.Instance.RefreshActor(this))
			{
				Log.Information($"Completed actor refresh for actor address: {this.Address}");
			}
			else
			{
				Log.Information($"Could not refresh actor: {this.Address}");
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex, $"Error refreshing actor: {this.Address}");
		}
		finally
		{
			this.IsRefreshing = false;
			this.WriteDelayedBinds();
		}

		this.RaisePropertyChangedSomeMethod2(nameof(this.IsHuman));
		await Task.Delay(150);
		this.RaisePropertyChangedSomeMethod2(nameof(this.IsHuman));
	}

	public async Task BackupAsync()
	{
		while (this.IsRefreshing)
			await Task.Delay(10);

		this.CreateCharacterBackup(BackupModes.Gpose);
	}

	protected override void HandlePropertyChanged(PropertyChange change)
	{
		this.History.Record(change);

		if (change.Origin != PropertyChange.Origins.Game)
			this.backupQueue.Invoke();

		if (!this.AutomaticRefreshEnabled)
			return;

		if (this.IsRefreshing)
		{
			// dont refresh because of a refresh!
			if (change.TerminalPropertyName == nameof(this.ObjectKind) || change.TerminalPropertyName == nameof(this.RenderMode))
			{
				return;
			}
		}

		if (change.OriginBind.Flags.HasFlag(BindFlags.ActorRefresh) && change.Origin != PropertyChange.Origins.Game)
		{
			this.Refresh();
		}
	}

	protected override bool CanWrite(BindInfo bind)
	{
		if (this.IsRefreshing)
		{
			if (bind.Memory != this)
			{
				Log.Warning("Skipping Bind " + bind);

				// Do not allow writing of any properties form sub-memory while we are refreshing
				return false;
			}
			else
			{
				// do not allow writing of any properties except the ones needed for refresh during a refresh.
				return bind.Name == nameof(this.ObjectKind) || bind.Name == nameof(this.RenderMode);
			}
		}

		return base.CanWrite(bind);
	}

	private void CreateCharacterBackup(BackupModes mode)
	{
		if (this.isRestoringBackup)
			return;

		if (mode == BackupModes.Original)
		{
			if (this.originalCharacterBackup == null)
				this.originalCharacterBackup = new();

			this.originalCharacterBackup.WriteToFile(this, CharacterFile.SaveModes.All);
		}
		else if (mode == BackupModes.Gpose)
		{
			if (this.gposeCharacterBackup == null)
				this.gposeCharacterBackup = new();

			this.gposeCharacterBackup.WriteToFile(this, CharacterFile.SaveModes.All);
		}
	}

	private async Task RestoreCharacterBackup(BackupModes mode)
	{
		CharacterFile? backup = null;

		if (mode == BackupModes.Gpose)
		{
			backup = this.gposeCharacterBackup;
		}
		else if (mode == BackupModes.Original)
		{
			backup = this.originalCharacterBackup;
		}

		if (backup == null)
			return;

		this.isRestoringBackup = true;

		bool allowRefresh = !GposeService.GetIsGPose();
		await backup.Apply(this, CharacterFile.SaveModes.All, allowRefresh);

		// If we were a player, really make sure we are again.
		if (allowRefresh && backup.ObjectKind == ActorTypes.Player)
		{
			this.ObjectKind = backup.ObjectKind;
		}

		this.isRestoringBackup = false;
	}
}
