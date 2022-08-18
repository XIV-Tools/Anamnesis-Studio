// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Panels;

using Anamnesis.Memory;
using Anamnesis.Panels;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

using CmQuaternion = Anamnesis.Memory.Quaternion;

public partial class TransformPanel : ActorPanelBase
{
	public TransformPanel(IPanelHost host)
		: base(host)
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		PoseService.EnabledChanged += this.OnPoseServiceEnabledChanged;
	}

	public bool IsFlipping { get; private set; }
	public SkeletonVisual3d? Skeleton { get; private set; }

	protected override async Task OnActorChanged()
	{
		await base.OnActorChanged();

		if (this.Actor == null)
			return;

		this.Skeleton = await this.Services.Pose.GetSkeleton(this.Actor);
	}

	private void OnPoseServiceEnabledChanged(bool value)
	{
		if (!value)
		{
			this.Close();
		}
	}

	private void OnParentClicked(object sender, RoutedEventArgs e)
	{
		if (this.Skeleton?.CurrentBone?.Parent == null)
			return;

		this.Skeleton.Select(this.Skeleton.CurrentBone.Parent);
	}

	private void OnSelectChildrenClicked(object sender, RoutedEventArgs e)
	{
		if (this.Skeleton == null)
			return;

		List<BoneVisual3d> bones = new List<BoneVisual3d>();
		foreach (BoneVisual3d bone in this.Skeleton.SelectedBones)
		{
			bone.GetChildren(ref bones);
		}

		this.Skeleton.Select(bones, SkeletonVisual3d.SelectMode.Add);
	}

	private void OnClearClicked(object? sender, RoutedEventArgs? e)
	{
		this.Skeleton?.ClearSelection();
	}

	private void OnFlipClicked(object sender, RoutedEventArgs e)
	{
		if (this.Skeleton != null && !this.IsFlipping)
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
		}
	}

	/* Basic Idea:
		* get mirrored quat of targetBone
		* check if its a 'left' bone
		*- if it is:
		*          - get the mirrored quat of the corresponding right bone
		*          - store the first quat (from left bone) on the right bone
		*          - store the second quat (from right bone) on the left bone
		*      - if not:
		*          - store the quat on the target bone
		*  - recursively flip on all child bones
		*/

	// TODO: This doesn't seem to be working correctly after the skeleton upgrade. not sure why...
	private void FlipBone(BoneVisual3d? targetBone, bool shouldFlip = true)
	{
		if (targetBone != null)
		{
			CmQuaternion newRotation = targetBone.TransformMemory.Rotation.Mirror(); // character-relative transform
			if (shouldFlip && targetBone.BoneName.EndsWith("_l"))
			{
				string rightBoneString = targetBone.BoneName.Substring(0, targetBone.BoneName.Length - 2) + "_r"; // removes the "_l" and replaces it with "_r"
				/*	Useful debug lines to make sure the correct bones are grabbed...
					*	Log.Information("flipping: " + targetBone.BoneName);
					*	Log.Information("right flip target: " + rightBoneString); */
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
	}
}
