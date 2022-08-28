// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Controls;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using XivToolsWpf.DependencyProperties;

public partial class Image : UserControl
{
	public static readonly IBind<ImageSource?> SourceDp = Binder.Register<ImageSource?, Image>(nameof(Source), BindMode.OneWay);

	public Image()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public ImageSource? Source
	{
		get => SourceDp.Get(this);
		set => SourceDp.Set(this, value);
	}

	public CornerRadius CornerRadius { get; set; }
}
