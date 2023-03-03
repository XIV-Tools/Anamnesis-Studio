// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles;

using Anamnesis.Memory;
using FontAwesome.Sharp.Pro;

public static class ActorTypesExtensions
{
	public static bool IsSupportedType(this ActorTypes actorType)
	{
		switch (actorType)
		{
			case ActorTypes.Player:
			case ActorTypes.BattleNpc:
			case ActorTypes.EventNpc:
			case ActorTypes.Companion:
			case ActorTypes.Mount:
			case ActorTypes.Ornament:
			case ActorTypes.Retainer:
				return true;
		}

		return false;
	}

	public static ProIcons GetIcon(this ActorTypes type)
	{
		switch (type)
		{
			case ActorTypes.Player: return ProIcons.UserAlt;
			case ActorTypes.BattleNpc: return ProIcons.UserShield;
			case ActorTypes.EventNpc: return ProIcons.UserNinja;
			case ActorTypes.Treasure: return ProIcons.Coins;
			case ActorTypes.Aetheryte: return ProIcons.Gem;
			case ActorTypes.Companion: return ProIcons.Cat;
			case ActorTypes.Retainer: return ProIcons.ConciergeBell;
			case ActorTypes.Housing: return ProIcons.Chair;
			case ActorTypes.Mount: return ProIcons.Horse;
			case ActorTypes.Ornament: return ProIcons.HatCowboy;
		}

		return ProIcons.Question;
	}
}
