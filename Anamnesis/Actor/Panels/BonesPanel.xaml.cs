﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Panels;

using Anamnesis.Actor;
using Anamnesis.Actor.Views;
using Anamnesis.Memory;
using Anamnesis.Navigation;
using Anamnesis.Panels;
using Anamnesis.Services;
using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using XivToolsWpf;

using CmQuaternion = Anamnesis.Memory.Quaternion;

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

	private void OnViewChanged(object sender, SelectionChangedEventArgs e)
	{
		int selected = this.ViewSelector.SelectedIndex;

		if (this.GuiView == null)
			return;

		this.GuiView.Visibility = selected == 0 ? Visibility.Visible : Visibility.Collapsed;
		this.MatrixView.Visibility = selected == 1 ? Visibility.Visible : Visibility.Collapsed;
		this.ThreeDView.Visibility = selected == 2 ? Visibility.Visible : Visibility.Collapsed;
	}

	private void OnCanvasMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left)
		{
			this.isLeftMouseButtonDownOnWindow = true;
			this.origMouseDownPoint = e.GetPosition(this.SelectionCanvas);
		}
	}

	private void OnCanvasMouseMove(object sender, MouseEventArgs e)
	{
		Point curMouseDownPoint = e.GetPosition(this.SelectionCanvas);

		if (this.isDragging)
		{
			e.Handled = true;

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

				Point relativePoint = boneView.TransformToAncestor(this.MouseCanvas).Transform(new Point(0, 0));
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
		else if (this.isLeftMouseButtonDownOnWindow)
		{
			System.Windows.Vector dragDelta = curMouseDownPoint - this.origMouseDownPoint;
			double dragDistance = Math.Abs(dragDelta.Length);
			if (dragDistance > DragThreshold)
			{
				this.isDragging = true;
				this.MouseCanvas.CaptureMouse();
				e.Handled = true;
			}
		}
	}

	private void OnCanvasMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
	{
		if (!this.isLeftMouseButtonDownOnWindow)
			return;

		this.isLeftMouseButtonDownOnWindow = false;
		if (this.isDragging)
		{
			this.isDragging = false;

			double minx = Canvas.GetLeft(this.DragSelectionBorder);
			double miny = Canvas.GetTop(this.DragSelectionBorder);
			double maxx = minx + this.DragSelectionBorder.Width;
			double maxy = miny + this.DragSelectionBorder.Height;

			foreach (BoneView boneView in this.BoneViews)
			{
				if (!boneView.IsVisible)
					continue;

				boneView.Hover(false);

				Point relativePoint = boneView.TransformToAncestor(this.MouseCanvas).Transform(new Point(0, 0));
				if (relativePoint.X > minx && relativePoint.X < maxx && relativePoint.Y > miny && relativePoint.Y < maxy)
				{
					boneView.Select();
				}
			}

			this.DragSelectionBorder.Visibility = Visibility.Collapsed;
			e.Handled = true;
		}
		else
		{
			// TODO: clear selection
		}

		this.MouseCanvas.ReleaseMouseCapture();
	}
}
