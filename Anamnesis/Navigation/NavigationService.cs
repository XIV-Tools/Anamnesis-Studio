// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Navigation;

using Anamnesis.Actor.Panels;
using Anamnesis.Actor.Posing.Panels;
using Anamnesis.Libraries.Panels;
using Anamnesis.Panels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

public class NavigationService : ServiceBase<NavigationService>
{
	private static readonly Dictionary<string, Type> Panels = new()
	{
		{ "GenericDialog", typeof(GenericDialogPanel) },
		{ "Settings", typeof(SettingsPanel) },
		{ "Weather", typeof(WeatherPanel) },
		{ "Character", typeof(CharacterPanel) },
		{ "Camera", typeof(CameraPanel) },
		{ "Posing", typeof(PosingPanel) },
		{ "Bones", typeof(BonesPanel) },
		{ "BoneTransform", typeof(BoneTransformPanel) },
		{ "ImportFile", typeof(ImportFilePanel) },
		{ "Library", typeof(LibraryPanel) },
	};

	/// <summary>
	/// Navigate to a panel.
	/// </summary>
	public async Task<PanelBase?> Navigate(Request request)
	{
		try
		{
			Type? panelType;
			if (!Panels.TryGetValue(request.Destination, out panelType))
				throw new Exception($"No panel type found for navigation: {request.Destination}");

			return await Services.Panels.Show(panelType, request.Context);
		}
		catch (Exception ex)
		{
			Log.Error(ex, $"Failed to handle navigation request to Uri: \"{request}\"");
			return null;
		}
	}

	public struct Request
	{
		public string Destination;
		public string? Context;

		public Request(string destination, string? context = null)
		{
			this.Destination = destination;
			this.Context = context;
		}

		/// <summary>
		/// Navigate to a panel.
		/// </summary>
		public async Task<PanelBase?> Navigate() => await Services.Navigation.Navigate(this);

		public readonly override string? ToString()
		{
			if (this.Context != null)
				return $"Anamnesis://{this.Destination}/{this.Context}";

			return $"Anamnesis://{this.Destination}";
		}

		public void Parse(string str)
		{
			throw new NotImplementedException();
		}
	}
}
