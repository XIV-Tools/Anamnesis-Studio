// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Anamnesis.Files;
using Anamnesis.Keyboard;
using Anamnesis.Services;

public partial class SettingsPanel : PanelBase
{
	public SettingsPanel()
		: base()
	{
		List<FontOption> fonts = new();
		foreach (Settings.Fonts font in Enum.GetValues<Settings.Fonts>())
		{
			fonts.Add(new FontOption(font));
		}

		this.Fonts = fonts;

		List<LanguageOption> languages = new();
		foreach ((string key, string name) in LocalizationService.GetAvailableLocales())
		{
			languages.Add(new LanguageOption(key, name));
		}

		this.Languages = languages;
	}

	public SettingsService SettingsService => SettingsService.Instance;

	public IEnumerable<FontOption> Fonts { get; }
	public IEnumerable<LanguageOption> Languages { get; }

	public FontOption SelectedFont
	{
		get
		{
			foreach (FontOption font in this.Fonts)
			{
				if (font.Font == SettingsService.Current.Font)
				{
					return font;
				}
			}

			return this.Fonts.First();
		}

		set
		{
			SettingsService.Current.Font = value.Font;
		}
	}

	public LanguageOption SelectedLanguage
	{
		get
		{
			foreach (LanguageOption language in this.Languages)
			{
				if (language.Key == SettingsService.Current.Language)
				{
					return language;
				}
			}

			return this.Languages.First();
		}

		set
		{
			SettingsService.Current.Language = value.Key;
			LocalizationService.SetLocale(value.Key);
		}
	}

	private void OnBrowseCharacter(object sender, RoutedEventArgs e)
	{
		FolderBrowserDialog dlg = new FolderBrowserDialog();
		dlg.SelectedPath = FileService.ParseToFilePath(SettingsService.Current.DefaultCharacterDirectory);
		DialogResult result = dlg.ShowDialog();

		if (result != DialogResult.OK)
			return;

		SettingsService.Current.DefaultCharacterDirectory = FileService.ParseFromFilePath(dlg.SelectedPath);
	}

	private void OnBrowsePose(object sender, RoutedEventArgs e)
	{
		FolderBrowserDialog dlg = new FolderBrowserDialog();
		dlg.SelectedPath = FileService.ParseToFilePath(SettingsService.Current.DefaultPoseDirectory);
		DialogResult result = dlg.ShowDialog();

		if (result != DialogResult.OK)
			return;

		SettingsService.Current.DefaultPoseDirectory = FileService.ParseFromFilePath(dlg.SelectedPath);
	}

	private void OnBrowseCamera(object sender, RoutedEventArgs e)
	{
		FolderBrowserDialog dlg = new FolderBrowserDialog();
		dlg.SelectedPath = FileService.ParseToFilePath(SettingsService.Current.DefaultCameraShotDirectory);
		DialogResult result = dlg.ShowDialog();

		if (result != DialogResult.OK)
			return;

		SettingsService.Current.DefaultCameraShotDirectory = FileService.ParseFromFilePath(dlg.SelectedPath);
	}

	private void OnBrowseScene(object sender, RoutedEventArgs e)
	{
		FolderBrowserDialog dlg = new FolderBrowserDialog();
		dlg.SelectedPath = FileService.ParseToFilePath(SettingsService.Current.DefaultSceneDirectory);
		DialogResult result = dlg.ShowDialog();

		if (result != DialogResult.OK)
			return;

		SettingsService.Current.DefaultSceneDirectory = FileService.ParseFromFilePath(dlg.SelectedPath);
	}

	public class FontOption
	{
		public FontOption(Settings.Fonts font)
		{
			this.Key = $"[Settings_Font_{font}]";
			this.Font = font;
		}

		public string Key { get; }
		public Settings.Fonts Font { get; }
	}

	public class LanguageOption
	{
		public LanguageOption(string key, string display)
		{
			this.Key = key;
			this.Display = display;
		}

		public string Key { get; }
		public string Display { get; }
	}
}
