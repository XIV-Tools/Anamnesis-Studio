// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using System.Threading.Tasks;
using System;

public partial class PoseFileImporter : ActorFileImporterBase
{
	private PoseFile? poseBackup;
	private PoseFile.Mode mode = PoseFile.Mode.All;

	public PoseFile File => (PoseFile)this.BaseFile!;

	public PoseFile.Mode Mode
	{
		get => this.mode;
		set
		{
			this.mode = value;
			this.OnConfigurationChanged();
		}
	}

	public override bool CanRevert => base.CanRevert && this.poseBackup != null;

	public override async Task Apply(bool isPreview)
	{
		await base.Apply(isPreview);

		// backup the character info we're about to stomp on.
		if (this.poseBackup == null)
		{
			this.poseBackup = new();
			this.poseBackup.WriteToFile(this.Actor, null);
		}

		await this.File.Apply(this.Actor, null, this.Mode);

		this.RaisePropertyChanged(nameof(this.CanRevert));
	}

	public override async Task Revert()
	{
		if (this.poseBackup == null)
			throw new Exception("No character backup to revert");

		await base.Revert();

		await this.poseBackup.Apply(this.Actor, null, PoseFile.Mode.All);
		this.poseBackup = null;

		this.RaisePropertyChanged(nameof(this.CanRevert));
	}
}
