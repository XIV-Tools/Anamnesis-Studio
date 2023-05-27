// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Panels;

using Anamnesis.Actor;
using Anamnesis.Actor.Posing;
using Anamnesis.Actor.Views;
using Anamnesis.Services;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XivToolsWpf;

public partial class BonesPanel : ActorPanelBase
{
	public const double DragThreshold = 20;

	public HashSet<BoneView> BoneViews = new HashSet<BoneView>();

	private bool isLeftMouseButtonDownOnWindow;
	private bool isDragging;
	private Point origMouseDownPoint;

	public BonesPanel()
		: base()
	{
		PoseService.EnabledChanged += this.OnPoseServiceEnabledChanged;
	}

	private async void OnPoseServiceEnabledChanged(bool value)
	{
		if (!value)
		{
			await this.Dispatcher.MainThread();
			this.Close();
		}
	}

	/*

	private async void OnExportClicked(object sender, RoutedEventArgs e)
	{
		lastSaveDir = await PoseFile.Save(lastSaveDir, this.Actor, this.Skeleton);
	}

	private async void OnExportMetaClicked(object sender, RoutedEventArgs e)
	{
		lastSaveDir = await PoseFile.Save(lastSaveDir, this.Actor, this.Skeleton, null, true);
	}

	private async void OnExportSelectedClicked(object sender, RoutedEventArgs e)
	{
		if (this.Skeleton == null)
			return;

		HashSet<string> bones = new HashSet<string>();
		foreach (BoneVisual3d bone in this.Skeleton.SelectedBones)
		{
			bones.Add(bone.BoneName);
		}

		lastSaveDir = await PoseFile.Save(lastSaveDir, this.Actor, this.Skeleton, bones);
	}*/

	private void OnCanvasMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left)
		{
			this.isLeftMouseButtonDownOnWindow = true;
			this.origMouseDownPoint = e.GetPosition(this.SelectionCanvas);
		}
	}

	private Point GetBonePosition(BoneView bone)
	{
		Point relativePoint = bone.TransformToAncestor(this).Transform(new Point(0, 0));
		relativePoint = this.TransformToDescendant(this.MouseCanvas).Transform(relativePoint);
		return relativePoint;
	}

	private void OnCanvasMouseMove(object sender, MouseEventArgs e)
	{
		Point curMouseDownPoint = e.GetPosition(this.SelectionCanvas);

		if (this.isDragging)
		{
			e.Handled = true;

			try
			{
				this.DragSelectionBorder.Visibility = Visibility.Visible;

				double minx = Math.Min(curMouseDownPoint.X, this.origMouseDownPoint.X);
				double miny = Math.Min(curMouseDownPoint.Y, this.origMouseDownPoint.Y);
				double maxx = Math.Max(curMouseDownPoint.X, this.origMouseDownPoint.X);
				double maxy = Math.Max(curMouseDownPoint.Y, this.origMouseDownPoint.Y);

				minx = Math.Max(minx, 0);
				miny = Math.Max(miny, 0);
				maxx = Math.Min(maxx, this.SelectionCanvas.ActualWidth);
				maxy = Math.Min(maxy, this.SelectionCanvas.ActualHeight);

				Canvas.SetLeft(this.DragSelectionBorder, minx);
				Canvas.SetTop(this.DragSelectionBorder, miny);
				this.DragSelectionBorder.Width = maxx - minx;
				this.DragSelectionBorder.Height = maxy - miny;

				List<BoneView> bones = new List<BoneView>();

				foreach (BoneView boneView in this.BoneViews)
				{
					if (!boneView.IsDescendantOf(this.MouseCanvas))
						continue;

					if (!boneView.IsVisible)
						continue;

					Point relativePoint = this.GetBonePosition(boneView);
					if (relativePoint.X > minx && relativePoint.X < maxx && relativePoint.Y > miny && relativePoint.Y < maxy)
					{
						boneView.Hover(true);
					}
					else
					{
						boneView.Hover(false);
					}
				}
			}
			catch (Exception ex)
			{
				this.Log.Error(ex, "Error performing drag select");
			}
		}
		else if (this.isLeftMouseButtonDownOnWindow)
		{
			Vector dragDelta = curMouseDownPoint - this.origMouseDownPoint;
			double dragDistance = Math.Abs(dragDelta.Length);
			if (dragDistance > DragThreshold)
			{
				this.isDragging = true;
				this.MouseCanvas.CaptureMouse();
				e.Handled = true;
			}
		}
	}

	private void OnCanvasMouseUp(object sender, MouseButtonEventArgs e)
	{
		if (!this.isLeftMouseButtonDownOnWindow)
			return;

		this.isLeftMouseButtonDownOnWindow = false;
		if (this.isDragging)
		{
			this.isDragging = false;

			try
			{
				double minx = Canvas.GetLeft(this.DragSelectionBorder);
				double miny = Canvas.GetTop(this.DragSelectionBorder);
				double maxx = minx + this.DragSelectionBorder.Width;
				double maxy = miny + this.DragSelectionBorder.Height;

				List<BoneViewModel> toSelect = new();
				foreach (BoneView boneView in this.BoneViews)
				{
					if (!boneView.IsVisible)
						continue;

					if (boneView.BoneViewModel == null)
						continue;

					boneView.Hover(false);

					Point relativePoint = this.GetBonePosition(boneView);
					if (relativePoint.X > minx && relativePoint.X < maxx && relativePoint.Y > miny && relativePoint.Y < maxy)
					{
						toSelect.Add(boneView.BoneViewModel);
					}
				}

				this.Services.Pose.SelectedBones.Replace(toSelect);
			}
			catch (Exception ex)
			{
				this.Log.Error(ex, "Error performing drag select");
			}

			this.DragSelectionBorder.Visibility = Visibility.Collapsed;
			e.Handled = true;
		}
		else
		{
			this.Services.Pose.SelectedBones.Clear();
		}

		this.MouseCanvas.ReleaseMouseCapture();
	}
}
