// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Controls;

using System.Windows.Controls;
using System.Windows;
using Anamnesis.Libraries.Items;

public partial class EntryIcon : UserControl
{
	public EntryIcon()
	{
		this.InitializeComponent();
	}

	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		EntryBase? entry = this.DataContext as EntryBase;

		if (entry != null && entry.HasThumbnail)
		{
			this.ThumbnailArea.Visibility = Visibility.Visible;
			this.IconArea.HorizontalAlignment = HorizontalAlignment.Left;
			this.IconArea.VerticalAlignment = VerticalAlignment.Bottom;
			this.IconArea.Height = 32;
			this.IconShadow.Visibility = Visibility.Visible;
		}
		else
		{
			this.ThumbnailArea.Visibility = Visibility.Collapsed;
			this.IconArea.HorizontalAlignment = HorizontalAlignment.Stretch;
			this.IconArea.VerticalAlignment = VerticalAlignment.Stretch;
			this.IconArea.Height = double.NaN;
			this.IconShadow.Visibility = Visibility.Collapsed;
		}
	}

	private void OnSizeChanged(object sender, SizeChangedEventArgs e)
	{
		EntryBase? entry = this.DataContext as EntryBase;

		if (entry != null && entry.HasThumbnail)
		{
			this.IconArea.Height = this.ActualHeight / 3.0;
		}
		else
		{
			this.IconArea.Height = double.NaN;
		}
	}
}
