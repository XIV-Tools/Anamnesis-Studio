// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using FontAwesome.Sharp;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

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
	bool CanScroll { get; set; }
	bool IsOpen { get; }

	IPanelHost Host { get; }

	void DragMove();
	void Close();
}