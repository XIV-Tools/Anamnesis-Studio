// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries;

using Anamnesis.Files;
using Anamnesis.Libraries.Sources;

public class PoseLibrary : LibraryBase
{
	public override void Initialize()
	{
		this.AddSource(new FileSource<PoseFile>("Anamnesis Poses", FileService.DefaultPoseDirectory.Directory, true));
		this.AddSource(new FileSource<CmToolPoseFile>("CmTool Poses", FileService.CMToolPoseSaveDir.Directory, true));
	}
}
