// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using Anamnesis.Actor.Panels;
using Anamnesis.Actor.Posing;
using Anamnesis.Posing;
using PropertyChanged;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using XivToolsWpf.DependencyProperties;
using static Anamnesis.Files.PoseFile;

[AddINotifyPropertyChangedInterface]
public partial class BoneView : UserControl
{
	public static readonly IBind<string> LabelDp = Binder.Register<string, BoneView>(nameof(Label));
	public static readonly IBind<string> NameDp = Binder.Register<string, BoneView>(nameof(BoneName));
	public static readonly IBind<string> FlippedNameDp = Binder.Register<string, BoneView>(nameof(FlippedBoneName));

	private BoneViewModel? boneViewModel;

	public BoneView()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

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

	// TODO: if this is pose GUI, use flipped if enabled.
	public string CurrentBoneName => this.BoneName;
	/*{
		get
		{
			if (string.IsNullOrEmpty(this.FlippedBoneName) || !this.skeleton.FlipSides)
				return this.BoneName;

			return this.FlippedBoneName;
		}
	}*/

	public void Hover(bool hover)
	{
		// TODO: it woudl be nice if we could get our control to act as though its hovered.
	}

	public void Select(bool select)
	{
		if (select)
		{
			string boneName = LegacyBoneNameConverter.GetModernName(this.BoneName) ?? this.BoneName;
			this.boneViewModel = this.FindParent<BonesPanel>()?.Actor?.ModelObject?.Skeleton?.GetBone(boneName);

			if (this.boneViewModel == null)
				return;

			PoseService.Instance.SelectedBones.Add(this.boneViewModel);
		}
		else
		{
			if (this.boneViewModel == null)
				return;

			PoseService.Instance.SelectedBones.Remove(this.boneViewModel);
			this.boneViewModel = null;
		}
	}

	private void OnLoaded(object sender, RoutedEventArgs e) => this.FindParent<BonesPanel>()?.BoneViews.Add(this);
	private void OnUnloaded(object sender, RoutedEventArgs e) => this.FindParent<BonesPanel>()?.BoneViews.Remove(this);

	private void OnChecked(object sender, RoutedEventArgs e)
	{
		this.Select(true);
	}

	private void OnUnchecked(object sender, RoutedEventArgs e)
	{
		this.Select(false);
	}
}
