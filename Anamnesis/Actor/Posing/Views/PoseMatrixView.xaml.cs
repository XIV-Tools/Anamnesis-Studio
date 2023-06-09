// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

/// <summary>
/// Interaction logic for PoseMatrixPage.xaml.
/// </summary>
public partial class PoseMatrixView : UserControl
{
	public PoseMatrixView()
	{
		this.InitializeComponent();
	}

	public Task OnActorChanged()
	{
		return Task.CompletedTask;
	}

	private void OnGroupClicked(object sender, System.Windows.RoutedEventArgs e)
	{
		if (sender is DependencyObject ob)
		{
			GroupBox? groupBox = ob.FindParent<GroupBox>();
			if (groupBox == null)
				return;

			/*SkeletonVisual3d skeleton = (SkeletonVisual3d)this.DataContext;

			if (!Keyboard.IsKeyDown(Key.LeftCtrl))
				skeleton.SelectedBones.Clear();

			List<BoneView> bones = groupBox.FindChildren<BoneView>();
			skeleton.Select(bones);*/
		}
	}
}
