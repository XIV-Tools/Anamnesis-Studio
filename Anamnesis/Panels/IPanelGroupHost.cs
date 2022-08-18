// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using FontAwesome.Sharp;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

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

public interface IPanel : INotifyPropertyChanged
{
	string Id { get; }
	string? TitleKey { get; set; }
	string? Title { get; set; }
	IconChar Icon { get; set; }
	Color? TitleColor { get; set; }
	string FinalTitle { get; }
	Rect Rect { get; }

	bool ShowBackground { get; set; }
	bool CanResize { get; set; }
	bool IsOpen { get; }

	IPanelHost Host { get; }

	void DragMove();
	void Close();
}