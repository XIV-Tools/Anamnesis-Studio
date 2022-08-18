﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using System.Collections.Generic;
using System.Windows;

public interface IPanelHost
{
	string Id { get; }
	bool IsVisible { get; }

	IEnumerable<IPanel> Panels { get; }
	Rect Rect { get; set; }
	Rect RelativeRect { get; set; }

	void Show();
	void Show(IPanelHost copy);
	void AddPanel(IPanel panel);
	void RemovePanel(IPanel panel);

	void DragMove();
}
