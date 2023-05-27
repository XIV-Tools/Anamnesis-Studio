// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using Anamnesis.Actor.Panels;
using Anamnesis.Actor.Posing;
using Anamnesis.Posing;
using PropertyChanged;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XivToolsWpf;
using XivToolsWpf.DependencyProperties;

[AddINotifyPropertyChangedInterface]
public partial class BoneView : UserControl
{
	public static readonly IBind<string> LabelDp = Binder.Register<string, BoneView>(nameof(Label));
	public static readonly IBind<string> NameDp = Binder.Register<string, BoneView>(nameof(BoneName));
	public static readonly IBind<string> FlippedNameDp = Binder.Register<string, BoneView>(nameof(FlippedBoneName));

	private BoneViewModel? boneViewModel;
	private bool lockChanges = false;

	public BoneView()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public string? InternalBoneName => this.BoneViewModel?.Name;
	public string? LocalizedBoneName => this.BoneViewModel?.LocalizedBoneName;

	public BoneViewModel? BoneViewModel
	{
		get
		{
			if (this.boneViewModel == null)
			{
				string boneName = this.BoneName;

				bool flipSides = this.FindParent<PoseGuiView>()?.FlipSides == true;
				if (!string.IsNullOrEmpty(this.FlippedBoneName) && flipSides)
					boneName = this.FlippedBoneName;

				boneName = LegacyBoneNameConverter.GetModernName(boneName) ?? boneName;
				this.boneViewModel = this.FindParent<BonesPanel>()?.Actor?.ModelObject?.Skeleton?.GetBone(boneName);
			}

			return this.boneViewModel;
		}
	}

	public bool IsSelected { get; set; }

	public string Label
	{
		get => LabelDp.Get(this);
		set => LabelDp.Set(this, value);
	}

	public string BoneName
	{
		get => NameDp.Get(this);
		set => NameDp.Set(this, value);
	}

	public string FlippedBoneName
	{
		get => FlippedNameDp.Get(this);
		set => FlippedNameDp.Set(this, value);
	}

	public void Hover(bool hover)
	{
		// TODO: it woudl be nice if we could get our control to act as though its hovered.
	}

	public void Select(bool select, bool add)
	{
		if (this.BoneViewModel == null)
			return;

		if (select)
		{
			if (!add)
				PoseService.Instance.SelectedBones.Clear();

			PoseService.Instance.SelectedBones.Add(this.BoneViewModel);
			PoseService.Instance.SelectedActor = this.BoneViewModel.Actor;
		}
		else
		{
			PoseService.Instance.SelectedBones.Remove(this.BoneViewModel);
			this.boneViewModel = null;
		}
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		this.FindParent<BonesPanel>()?.BoneViews.Add(this);
		PoseService.Instance.SelectedBones.CollectionChanged += this.OnSelectedBonesChanged;
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		this.FindParent<BonesPanel>()?.BoneViews.Remove(this);
		PoseService.Instance.SelectedBones.CollectionChanged -= this.OnSelectedBonesChanged;
	}

	private void OnChecked(object sender, RoutedEventArgs e)
	{
		if (this.lockChanges)
			return;

		bool ctrlDown = Keyboard.GetKeyStates(Key.LeftCtrl) == KeyStates.Down || Keyboard.GetKeyStates(Key.RightCtrl) == KeyStates.Down;
		this.Select(true, ctrlDown);
	}

	private void OnUnchecked(object sender, RoutedEventArgs e)
	{
		if (this.lockChanges)
			return;

		this.Select(false, false);
	}

	private async void OnSelectedBonesChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		List<BoneViewModel> newSelection = new(App.Services.Pose.SelectedBones);

		await this.Dispatcher.MainThread();

		if (this.BoneViewModel == null)
			return;

		this.lockChanges = true;

		this.IsSelected = false;
		foreach (BoneViewModel selectedBone in newSelection)
		{
			if (selectedBone.Name == this.BoneViewModel.Name)
			{
				this.IsSelected = true;
				break;
			}
		}

		this.lockChanges = false;
	}
}
