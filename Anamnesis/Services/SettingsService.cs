﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Anamnesis;
using Anamnesis.Files;
using Anamnesis.Serialization;
using Anamnesis.Windows;
using XivToolsWpf;

public class SettingsService : ServiceBase<SettingsService>
{
	private static readonly string SettingsPath = FileService.ParseToFilePath(FileService.StoreDirectory + "/Settings.json");

	public static event PropertyChangedEventHandler? SettingsChanged;

	public static Settings Current => Instance.Settings!;

	public Settings? Settings { get; private set; }
	public bool FirstTimeUser { get; private set; }

	public static void ShowDirectory()
	{
		FileService.ShowDirectoryInExplorer(FileService.StoreDirectory);
	}

	public static void Save()
	{
		lock (Instance)
		{
			string json = SerializerService.Serialize(Instance.Settings!);
			File.WriteAllText(SettingsPath, json);
		}
	}

	public override async Task Initialize()
	{
		await base.Initialize();

		if (!File.Exists(SettingsPath))
		{
			this.FirstTimeUser = true;
			this.Settings = new Settings();
			Save();
		}
		else
		{
			this.FirstTimeUser = false;
			try
			{
				await App.Current.Dispatcher.MainThread();

				if (Keyboard.IsKeyDown(Key.LeftShift))
					throw new Exception("User Abort");

				string json = File.ReadAllText(SettingsPath);
				this.Settings = SerializerService.Deserialize<Settings>(json);
			}
			catch (Exception ex)
			{
				Log.Warning(ex, "Failed to load settings");
				await GenericDialog.ShowAsync("Failed to load Settings. Your settings have been reset.", "Error", MessageBoxButton.OK);
				this.FirstTimeUser = true;
				this.Settings = new Settings();
				Save();
			}
		}

		this.Settings.PropertyChanged += this.OnSettingsChanged;
		this.OnSettingsChanged(null, new PropertyChangedEventArgs(null));
	}

	private void OnSettingsChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (this.Settings == null)
			return;

		if (sender is Settings settings)
		{
			Save();
		}

		SettingsChanged?.Invoke(sender, e);
	}
}
