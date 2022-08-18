// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Navigation;

using Anamnesis.Actor.Panels;
using Anamnesis.Panels;
using System;
using System.Collections.Generic;
using System.Windows;
using static Anamnesis.Navigation.NavigationService;

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
	};

	/// <summary>
	/// Navigate to a panel.
	/// </summary>
	public static PanelBase Navigate(Request request)
	{
		try
		{
			Type? panelType;
			if (!Panels.TryGetValue(request.Destination, out panelType))
				throw new Exception($"No panel type found for navigation: {request.Destination}");

			PanelBase panel = PanelService.Show(panelType, request.Context);

			// Move the panel to the target position next to the origin panel
			/*PanelBase? originPanel = request.GetOriginPanel();
			if (originPanel != null)
			{
				if (panel.CloseMode == CloseModes.AutoClose ||
					panel.CloseMode == CloseModes.Both)
				{
					Rect panelRect = panel.Rect;
					Rect navRect = originPanel.Rect;
					Point pos = originPanel.GetSubPanelDockOffset();

					panelRect.X = navRect.X + pos.X;
					panelRect.Y = navRect.Y + pos.Y;

					panel.Rect = panelRect;
				}

				panel.SetParent(originPanel);
			}*/

			return panel;
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
		public PanelBase? Navigate() => NavigationService.Navigate(this);

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
