// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using Anamnesis.Files;
using Anamnesis.Memory;
using System;
using System.Windows;
using XivToolsWpf.Extensions;

public partial class ImportCharacterPanel : PanelBase
{
	public ImportCharacterPanel(IPanelHost host, OpenResult openFile)
		: base(host)
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		if (openFile.File is not CharacterFile characterFile)
			throw new Exception("Import file was not a character file");

		this.Title = openFile.Path?.Name;
		this.CharacterFile = characterFile;
	}

	public CharacterFile.SaveModes Mode { get; set; } = CharacterFile.SaveModes.All;
	public CharacterFile CharacterFile { get; set; }
	public PinnedActor? Actor { get; set; }

	private void OnImportClicked(object sender, RoutedEventArgs e)
	{
		ActorMemory? memory = this.Actor?.GetMemory();

		if (memory == null)
			throw new Exception("Actor has no memory");

		this.CharacterFile.Apply(memory, this.Mode).Run();
		this.Close();
	}
}
