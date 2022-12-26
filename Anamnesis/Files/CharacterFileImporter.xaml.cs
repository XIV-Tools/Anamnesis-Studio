// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using System;
using System.Threading.Tasks;

public partial class CharacterFileImporter : ActorFileImporterBase
{
	private CharacterFile? characterBackup;

	public CharacterFile File => (CharacterFile)this.BaseFile!;

	public CharacterFile.SaveModes Mode { get; set; } = CharacterFile.SaveModes.All;

	public override bool CanRevert => base.CanRevert && this.characterBackup != null;

	public override async Task Apply(bool isPreview)
	{
		await base.Apply(isPreview);

		// backup the character info we're about to stomp on.
		if (this.characterBackup == null)
		{
			this.characterBackup = new();
			this.characterBackup.WriteToFile(this.Actor, CharacterFile.SaveModes.All);
		}

		await this.File.Apply(this.Actor, this.Mode);
	}

	public override async Task Revert()
	{
		if (this.characterBackup == null)
			throw new Exception("No character backup to revert");

		await base.Revert();

		await this.characterBackup.Apply(this.Actor, CharacterFile.SaveModes.All);
		this.characterBackup = null;
	}
}
