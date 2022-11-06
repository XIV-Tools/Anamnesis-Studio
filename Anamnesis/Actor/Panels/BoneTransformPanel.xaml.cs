// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Panels;

using Anamnesis.Actor.Posing;
using Anamnesis.Services;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using XivToolsWpf;

public partial class BoneTransformPanel : ActorPanelBase
{
	public BoneTransformPanel()
		: base()
	{
		PoseService.EnabledChanged += this.OnPoseServiceEnabledChanged;
		this.Services.Pose.SelectedBones.CollectionChanged += this.OnSelectedBonesChanged;
	}

	public BoneViewModel? CurrentBone { get; set; }

	private async void OnPoseServiceEnabledChanged(bool value)
	{
		if (!value)
		{
			await this.Dispatcher.MainThread();
			this.Close();
		}
	}

	private void OnSelectedBonesChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (this.Services.Pose.SelectedBones.Count <= 0)
			return;

		this.CurrentBone = this.Services.Pose.SelectedBones[0];

		this.Dispatcher.BeginInvoke(() =>
		{
			string? title = LocalizationService.GetLocalizedText("[Navigation_Actor_Transform]");

			if (this.CurrentBone == null)
			{
				this.Title = title;
			}
			else
			{
				string? bone = LocalizationService.GetLocalizedText(this.CurrentBone.LocalizedBoneName);
				this.Title = $"{title} - {bone}";
			}
		});
	}

	private void OnParentClicked(object sender, RoutedEventArgs e)
	{
		BoneViewModel? parent = this.CurrentBone?.GetParent();
		if (parent == null)
			return;

		this.Services.Pose.SelectedBones.Clear();
		this.Services.Pose.SelectedBones.Add(parent);
	}

	private void OnSelectChildrenClicked(object sender, RoutedEventArgs e)
	{
		List<BoneViewModel>? children = this.CurrentBone?.GetChildren();
		if (children == null)
			return;

		this.Services.Pose.SelectedBones.Clear();
		this.Services.Pose.SelectedBones.AddRange(children);
	}

	private void OnClearClicked(object? sender, RoutedEventArgs? e)
	{
		this.Services.Pose.SelectedBones.Clear();
	}

	private void OnFlipClicked(object sender, RoutedEventArgs e)
	{
		/*if (this.Skeleton != null && !this.IsFlipping)
		{
			// if no bone selected, flip both lumbar and waist bones
			this.IsFlipping = true;
			if (this.Skeleton.CurrentBone == null)
			{
				BoneVisual3d? waistBone = this.Skeleton.GetBone("Waist");
				BoneVisual3d? lumbarBone = this.Skeleton.GetBone("SpineA");
				this.FlipBone(waistBone);
				this.FlipBone(lumbarBone);
				waistBone?.ReadTransform(true);
				lumbarBone?.ReadTransform(true);
			}
			else
			{
				// if targeted bone is a limb don't switch the respective left and right sides
				BoneVisual3d targetBone = this.Skeleton.CurrentBone;
				if (targetBone.BoneName.EndsWith("_l") || targetBone.BoneName.EndsWith("_r"))
				{
					this.FlipBone(targetBone, false);
				}
				else
				{
					this.FlipBone(targetBone);
				}

				targetBone.ReadTransform(true);
			}

			this.IsFlipping = false;
		}*/
	}

	/*private void FlipBone(BoneVisual3d? targetBone, bool shouldFlip = true)
	{
		if (targetBone != null)
		{
			CmQuaternion newRotation = targetBone.TransformMemory.Rotation.Mirror(); // character-relative transform
			if (shouldFlip && targetBone.BoneName.EndsWith("_l"))
			{
				string rightBoneString = targetBone.BoneName.Substring(0, targetBone.BoneName.Length - 2) + "_r"; // removes the "_l" and replaces it with "_r"
				BoneVisual3d? rightBone = targetBone.Skeleton.GetBone(rightBoneString);
				if (rightBone != null)
				{
					CmQuaternion rightRot = rightBone.TransformMemory.Rotation.Mirror();
					foreach (TransformMemory transformMemory in targetBone.TransformMemories)
					{
						transformMemory.Rotation = rightRot;
					}

					foreach (TransformMemory transformMemory in rightBone.TransformMemories)
					{
						transformMemory.Rotation = newRotation;
					}
				}
				else
				{
					this.Log.Warning("could not find right bone of: " + targetBone.BoneName);
				}
			}
			else if (shouldFlip && targetBone.BoneName.EndsWith("_r"))
			{
				// do nothing so it doesn't revert...
			}
			else
			{
				foreach (TransformMemory transformMemory in targetBone.TransformMemories)
				{
					transformMemory.Rotation = newRotation;
				}
			}

			if (PoseService.Instance.EnableParenting)
			{
				foreach (Visual3D? child in targetBone.Children)
				{
					if (child is BoneVisual3d childBone)
					{
						this.FlipBone(childBone, shouldFlip);
					}
				}
			}
		}
	}*/
}
