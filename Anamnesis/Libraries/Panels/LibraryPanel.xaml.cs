// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Panels;

using Anamnesis.Libraries.Items;
using Anamnesis.Panels;
using System.Windows;

public partial class LibraryPanel : PanelBase
{
	public LibraryPanel(IPanelHost host)
		: base(host)
	{
		this.InitializeComponent();

		this.ContentArea.DataContext = this;
	}

	public Pack? SelectedPack { get; set; }

	private void OnGroupSelected(object sender, RoutedPropertyChangedEventArgs<object> e)
	{
		if (e.NewValue is Pack pack)
		{
			this.SelectedPack = pack;
		}
	}
}
