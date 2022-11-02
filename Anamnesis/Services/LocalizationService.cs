// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Anamnesis.Files;
using XivToolsWpf.Designer;
using XivToolsWpf.Localization;

public class LocalizationService : ServiceBase<LocalizationService>
{
	public const string FallbackCulture = "EN";

	private static readonly Dictionary<string, Locale> Locales = new Dictionary<string, Locale>();

	private static Locale? fallbackLocale;
	private static Locale? currentLocale;

	public static bool Loaded => Exists && currentLocale != null;

	public static void Add(string culture, string cultureName, string key, string value)
	{
		culture = culture.ToUpperInvariant();

		if (!Locales.ContainsKey(culture))
			Locales.Add(culture, new Locale(culture, cultureName));

		Locale locale = Locales[culture];

		Locales[culture].Add(key, value);
	}

	public static void Add(string culture, string cultureName, Dictionary<string, string> values)
	{
		foreach ((string key, string value) in values)
		{
			Add(culture, cultureName, key, value);
		}
	}

	public static string GetLocalizedText(string key)
	{
		if (Locales.Count <= 0)
			Load();

		bool silent = false;

		if (App.Current == null)
			silent = true;

		return GetString(key, silent);
	}

	public static bool HasString(string key)
	{
		string val;
		if (currentLocale?.Get(key, out val) != true)
			return false;

		return true;
	}

	public static string GetString(string key, bool silent = false)
	{
		string val = string.Empty;

		if (key.StartsWith('['))
			key = key.Substring(1);

		if (key.EndsWith(']'))
			key = key.Substring(0, key.Length - 1);

		if (currentLocale?.Get(key, out val) ?? false)
			return val;

		if (!silent)
			Log.Warning("Missing Localized string: \"" + key + "\" in locale: \"" + currentLocale?.Culture + "\"");

		if (fallbackLocale?.Get(key, out val) ?? false)
			return val;

		if (!silent)
			Log.Error("Missing Localized string: \"" + key + "\"");

		if (!silent)
		{
			string erorString = "{" + key + "}";
			fallbackLocale?.Add(key, erorString);
			return erorString;
		}
		else
		{
			fallbackLocale?.Add(key, string.Empty);
			return string.Empty;
		}
	}

	public static void SetLocale(string locale)
	{
		locale = locale.ToUpper();

		if (Locales.ContainsKey(locale))
		{
			currentLocale = Locales[locale];
		}
		else
		{
			currentLocale = fallbackLocale;
		}

		// UPDATE ALL TEXT BLOCKS!!
	}

	public static Locale? GetLocale(string locale)
	{
		return Locales[locale];
	}

	public static Dictionary<string, string> GetAvailableLocales()
	{
		Dictionary<string, string> results = new Dictionary<string, string>();
		foreach (Locale locale in Locales.Values)
		{
			if (results.ContainsKey(locale.Culture))
				continue;

			results.Add(locale.Culture, locale.Name);
		}

		return results;
	}

	public override async Task Initialize()
	{
		await base.Initialize();

		TextBlockHook.GetLocalizedText = GetLocalizedText;
		TextBlockHook.Attach();

		Load();
	}

	public override async Task Start()
	{
		await base.Start();
	}

	private static void Load()
	{
		string[] languageFilePaths = EmbeddedFileUtility.GetAllFilesInDirectory("Languages");
		foreach (string languageFilePath in languageFilePaths)
		{
			try
			{
				string fileName = EmbeddedFileUtility.GetFileName(languageFilePath);
				Dictionary<string, string> values = EmbeddedFileUtility.Load<Dictionary<string, string>>(languageFilePath);

				string? name;
				if (!values.TryGetValue("Language", out name))
					name = fileName.ToUpper();

				Add(fileName, name, values);
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Failed to load language: {languageFilePath}");
			}
		}

		fallbackLocale = Locales[FallbackCulture];
		SetLocale(SettingsService.Current.Language);
	}

	public class Locale
	{
		public readonly Dictionary<string, string> Entries = new();
		public readonly string Culture;
		public string Name;

		public Locale(string culture, string name)
		{
			this.Culture = culture;
			this.Name = name;
		}

		public virtual void Add(string key, string value)
		{
			if (!this.Entries.ContainsKey(key))
				this.Entries.Add(key, value);

			this.Entries[key] = value;
		}

		public virtual bool Get(string key, out string value)
		{
			value = string.Empty;

			if (!this.Entries.ContainsKey(key))
				return false;

			value = this.Entries[key];
			return true;
		}
	}
}
