// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Anamnesis.Panels;
using PropertyChanged;

[Serializable]
[AddINotifyPropertyChangedInterface]
public class Settings : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	public enum Fonts
	{
		Default,
		Hyperlegible,
	}

	public string Language { get; set; } = "EN";
	public bool OverlayWindow { get; set; } = true;
	public double Opacity { get; set; } = 1.0;
	public string DefaultPoseDirectory { get; set; } = "%MyDocuments%/Anamnesis/Poses/";
	public string DefaultCharacterDirectory { get; set; } = "%MyDocuments%/Anamnesis/Characters/";
	public string DefaultCameraShotDirectory { get; set; } = "%MyDocuments%/Anamnesis/CameraShots/";
	public string DefaultSceneDirectory { get; set; } = "%MyDocuments%/Anamnesis/Scenes/";
	public string LocalPacksDirectory { get; set; } = "%MyDocuments%/Anamnesis/Packs/";
	public bool FlipPoseGuiSides { get; set; } = false;
	public Fonts Font { get; set; } = Fonts.Default;
	public Color? TrimColor { get; set; } = null;
	public bool UseLightTheme { get; set; } = false;
	public bool WrapRotationSliders { get; set; } = true;
	public string? DefaultAuthor { get; set; }
	public DateTimeOffset LastUpdateCheck { get; set; } = DateTimeOffset.MinValue;
	public string? GamePath { get; set; }
	public Dictionary<string, bool> PosingBoneLinks { get; set; } = new();
	public Dictionary<string, PanelService.PanelSettings> Panels { get; set; } = new();
	public bool HasBeenWelcomed { get; set; } = false;

	public Color GetTrimColor()
	{
		if (SystemParameters.WindowGlassColor == Colors.Black)
			return Color.FromArgb(255, 247, 99, 12);

		if (this.TrimColor != null)
			return (Color)this.TrimColor;

		return SystemParameters.WindowGlassColor;
	}
}
