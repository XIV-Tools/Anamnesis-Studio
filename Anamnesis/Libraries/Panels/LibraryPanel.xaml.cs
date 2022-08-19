// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Panels;

using Anamnesis.Libraries.Items;
using Anamnesis.Panels;
using System.Windows;

public partial class LibraryPanel : PanelBase
{
	public LibraryPanel(IPanelHost host, LibraryBase library)
		: base(host)
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		this.Library = library;

		this.TitleKey = this.Library.NameKey;
	}

	public LibraryBase Library { get; set; }
	public Group? SelectedGroup { get; set; }

	private void OnGroupSelected(object sender, RoutedPropertyChangedEventArgs<object> e)
	{
		if (e.NewValue is Group gi)
		{
			this.SelectedGroup = gi;
		}
	}
}
