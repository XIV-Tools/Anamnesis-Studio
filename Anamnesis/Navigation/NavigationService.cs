// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Navigation;

using Anamnesis.Actor.Panels;
using Anamnesis.Libraries.Panels;
using Anamnesis.Panels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using XivToolsWpf.Extensions;

public class NavigationService : ServiceBase<NavigationService>
{
	private static readonly Dictionary<string, Type> Panels = new()
	{
		{ "GenericDialog", typeof(GenericDialogPanel) },
		{ "Exception", typeof(ExceptionPanel) },
		{ "Settings", typeof(SettingsPanel) },
		{ "Weather", typeof(WeatherPanel) },
		{ "ActorInfo", typeof(ActorInfoPanel) },
		{ "ActorCustomize", typeof(CustomizePanel) },
		{ "ActorEquipment", typeof(EquipmentPanel) },
		{ "Camera", typeof(CameraPanel) },
		{ "Bones", typeof(BonesPanel) },
		{ "Transform", typeof(TransformPanel) },
		{ "ImportCharacter", typeof(ImportCharacterPanel) },
		{ "ImportPose", typeof(ImportPosePanel) },
		{ "Library", typeof(LibraryPanel) },
	};

	/// <summary>
	/// Navigate to a panel.
	/// </summary>
	public static async Task<PanelBase> Navigate(Request request)
	{
		try
		{
			Type? panelType;
			if (!Panels.TryGetValue(request.Destination, out panelType))
				throw new Exception($"No panel type found for navigation: {request.Destination}");

			return await PanelService.Show(panelType, request.Context);
		}
		catch (Exception ex)
		{
			throw new Exception($"Failed to handle navigation request to Uri: \"{request}\"", ex);
		}
	}

	public struct Request
	{
		public object? Origin;
		public string Destination;
		public object? Context;

		public Request(object origin, string destination, object? context = null)
		{
			this.Origin = origin;
			this.Destination = destination;
			this.Context = context;
		}

		public Request(string destination, object? context = null)
		{
			this.Origin = null;
			this.Destination = destination;
			this.Context = context;
		}

		/// <summary>
		/// Navigate to a panel.
		/// </summary>
		public async Task<PanelBase?> Navigate() => await NavigationService.Navigate(this);

		public PanelBase? GetOriginPanel()
		{
			if (this.Origin is PanelBase panel)
				return panel;

			if (this.Origin is FrameworkElement fe)
				return fe.FindParent<PanelBase>();

			return null;
		}

		public readonly override string? ToString()
		{
			if (this.Context != null)
				return $"{this.Destination} - {this.Context}";

			return this.Destination;
		}
	}
}
