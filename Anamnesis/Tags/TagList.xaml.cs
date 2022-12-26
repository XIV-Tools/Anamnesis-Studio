// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tags;

using System.Windows;
using System.Windows.Controls;
using XivToolsWpf.DependencyProperties;

public partial class TagList : UserControl
{
	public static readonly IBind<TagCollection?> TagsDp = Binder.Register<TagCollection?, TagList>(nameof(Tags), BindMode.OneWay);

	public TagList()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public TagCollection? Tags
	{
		get => TagsDp.Get(this);
		set => TagsDp.Set(this, value);
	}

	private void OnTagClicked(object sender, RoutedEventArgs e)
	{
	}
}
