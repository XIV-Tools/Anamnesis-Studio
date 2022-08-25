// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Sources;

using Anamnesis.GameData;
using Anamnesis.Libraries.Items;
using Anamnesis.Services;
using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using XivToolsWpf;

public class GameDataSource : LibrarySourceBase
{
	private enum Packs
	{
		NPCs,
		Mounts,
		Companions,
		Ornaments,
	}

	public override IconChar Icon => IconChar.Database;
	public override string Name => LocalizationService.GetString("Library_GameDataSource");

	protected override async Task Load(Pack pack)
	{
		Packs packType = Enum.Parse<Packs>(pack.Id);

		pack.Author = LocalizationService.GetString("Library_GameData_Author");
		pack.Name = LocalizationService.GetString($"Library_GameData_{pack.Id}");
		pack.Description = LocalizationService.GetString($"Library_GameData_{pack.Id}_Description");

		pack.ClearItems();

		if (packType == Packs.NPCs)
		{
			await this.Populate(pack, GameDataService.ResidentNPCs, "Library_GameData_NPC", "Library_GameData_NPC_Resident");
			await this.Populate(pack, GameDataService.BattleNPCs, "Library_GameData_NPC", "Library_GameData_NPC_Battle");
			await this.Populate(pack, GameDataService.EventNPCs, "Library_GameData_NPC", "Library_GameData_NPC_Event");
		}
		else if (packType == Packs.Mounts)
		{
			await this.Populate(pack, GameDataService.Mounts, "Library_GameData_Mount");
		}
		else if (packType == Packs.Companions)
		{
			await this.Populate(pack, GameDataService.Companions, "Library_GameData_Companion");
		}
		else if (packType == Packs.Ornaments)
		{
			await this.Populate(pack, GameDataService.Ornaments, "Library_GameData_Ornament");
		}
	}

	protected override Task Update(Pack pack)
	{
		throw new NotSupportedException();
	}

	protected override async Task Load()
	{
		foreach (Packs pack in Enum.GetValues<Packs>())
		{
			await this.CreatePack(pack);
		}
	}

	private async Task CreatePack(Packs packType)
	{
		Pack pack = new Pack(packType.ToString(), this);

		BitmapImage bi = new BitmapImage();
		bi.BeginInit();
		bi.UriSource = new Uri("https://cdn.discordapp.com/attachments/811569064895184956/1010781496564527135/Affleck.png");
		bi.EndInit();

		pack.HeaderImage = bi;

		await this.Load(pack);
		await this.AddPack(pack);
	}

	private async Task Populate(Pack pack, IEnumerable<INpcBase> npcs, params string[] tags)
	{
		await Dispatch.NonUiThread();

		foreach (INpcBase npc in npcs)
		{
			pack.AddEntry(new NpcItem(npc, tags));
		}
	}

	public class NpcItem : ItemEntry
	{
		private readonly INpcBase npc;

		public NpcItem(INpcBase npc, params string[] tags)
		{
			this.npc = npc;
			this.Name = npc.Name;
			this.Description = npc.Description;

			if (!string.IsNullOrEmpty(this.Description))
				this.Tags.Add("Description");

			this.Tags.Add(LocalizationService.GetString(npc.HasName ? "Library_GameData_Named" : "Library_GameData_Unnamed"));

			foreach (string tag in tags)
			{
				this.Tags.Add(LocalizationService.GetString(tag));
			}
		}

		public override bool CanLoad => true;

		public override ImageSource? Icon => this.npc.Icon?.GetImageSource();
	}
}