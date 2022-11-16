// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using Anamnesis.Memory;
using System;
using System.Threading.Tasks;

public partial class CharacterFileImporter : ActorFileImporterBase
{
	private readonly CharacterFile characterBackup = new();

	public CharacterFile File => (CharacterFile)this.BaseFile!;

	public CharacterFile.SaveModes Mode { get; set; } = CharacterFile.SaveModes.All;

	public override async Task Apply(bool isPreview)
	{
		await base.Apply(isPreview);

		// backup the character info we're about to stomp on.
		this.characterBackup.WriteToFile(this.Actor, CharacterFile.SaveModes.All);

		await this.File.Apply(this.Actor, this.Mode);
	}

	public override async Task Revert()
	{
		await base.Revert();

		await this.characterBackup.Apply(this.Actor, CharacterFile.SaveModes.All);
	}
}
