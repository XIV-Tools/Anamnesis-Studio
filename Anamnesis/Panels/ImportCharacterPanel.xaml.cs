// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using Anamnesis.Files;
using Anamnesis.Memory;
using Anamnesis.Windows;
using System;
using System.Windows;
using XivToolsWpf.Extensions;

public partial class ImportCharacterPanel : PanelBase
{
	public CharacterFile.SaveModes Mode { get; set; } = CharacterFile.SaveModes.All;
	public CharacterFile? CharacterFile { get; set; }
	public ActorMemory? Actor { get; set; }

	public override void SetContext(FloatingWindow host, object? context)
	{
		base.SetContext(host, context);

		if (context is not OpenResult openFile)
			return;

		if (openFile.File is not CharacterFile characterFile)
			throw new Exception("Import file was not a character file");

		this.Title = openFile.Path?.Name;
		this.CharacterFile = characterFile;
	}

	private void OnImportClicked(object sender, RoutedEventArgs e)
	{
		if (this.Actor == null)
			throw new Exception("Must select a target actor");

		this.CharacterFile?.Apply(this.Actor, this.Mode).Run();
		this.Close();
	}
}
