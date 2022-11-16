// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using Anamnesis.Services;
using Anamnesis.XamlBehaviours;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using XivToolsWpf.Behaviours;
using XivToolsWpf.DragAndDrop;
using XivToolsWpf.Extensions;
using XivToolsWpf.Localization;

public static class Behaviours
{
	public static void SetIsReorderable(ItemsControl items, bool enable)
	{
		items.AttachHandler<Reorderable>(enable);
	}

	public static void SetTooltip(DependencyObject host, string key)
	{
		host.AttachHandler<LocalizedTooltipBehaiour>(true, key);
	}

	public static void SetSmoothScroll(DependencyObject host, bool enable)
	{
		host.AttachHandler<SmoothScrollBehaviour>(enable);
	}

	public static void SetNavigation(Button host, string destination)
	{
		host.Click += (s, e) =>
		{
			App.Services.Navigation.Navigate(new(destination)).Run();
		};
	}

	public static void SetDesignerLocalization(DependencyObject host, bool enable)
	{
		if (!DesignerProperties.GetIsInDesignMode(host))
			return;

		TextBlockHook.GetLocalizedText = LocalizationService.GetLocalizedText;
		TextBlockHook.Attach();
	}
}