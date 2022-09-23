// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tags;

using PropertyChanged;
using System.Windows.Controls;
using XivToolsWpf.DependencyProperties;
using XivToolsWpf.Extensions;

[AddINotifyPropertyChangedInterface]
public partial class TagFilter : UserControl
{
	public static readonly IBind<TagCollection?> AllTagsDp = Binder.Register<TagCollection?, TagFilter>(nameof(AllTags), OnAllTagsChanged, BindMode.OneWay);

	public TagFilter()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public FastObservableCollection<Tag> FilterByTags { get; init; } = new();

	public TagCollection? AllTags
	{
		get => AllTagsDp.Get(this);
		set => AllTagsDp.Set(this, value);
	}

	private static void OnAllTagsChanged(TagFilter sender, TagCollection? value)
	{
		if (value != null)
		{
			sender.FilterByTags.Replace(value);
		}
		else
		{
			sender.FilterByTags.Clear();
		}
	}

	private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
	{
		OnAllTagsChanged(this, this.AllTags);
	}
}