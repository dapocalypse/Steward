﻿using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Steward.Context;
using Steward.Context.Models;

namespace Steward.Services
{
	public class CharacterService
	{
		private readonly RollService _rollService;

		public CharacterService(RollService rollService)
		{
			_rollService = rollService;
		}

		public EmbedFieldBuilder ComposeStatEmbedField(PlayerCharacter character, Year year)
		{
			var strString = $"STR: {_rollService.CalculateStat(CharacterAttribute.STR, character)}"
							+ $"({_rollService.GetStatAsModifier(CharacterAttribute.STR, character)})";
			var endString = $"END: {_rollService.CalculateStat(CharacterAttribute.END, character)}"
			                + $"({_rollService.GetStatAsModifier(CharacterAttribute.END, character)})";
			var dexString = $"DEX: {_rollService.CalculateStat(CharacterAttribute.DEX, character)}"
			                + $"({_rollService.GetStatAsModifier(CharacterAttribute.DEX, character)})";
			var perString = $"PER: {_rollService.CalculateStat(CharacterAttribute.PER, character)}"
			                + $"({_rollService.GetStatAsModifier(CharacterAttribute.PER, character)})";
			var intString = $"INT: {_rollService.CalculateStat(CharacterAttribute.INT, character)}"
			                + $"({_rollService.GetStatAsModifier(CharacterAttribute.INT, character)})";

			var armorClassString = $"AC: {CalculateMaximumArmorClass(character)}";

			var abilityPointString = $"AP: {CalculateMaximumAbilityPoint(character)}";

			var healthPoolString = $"HP: {CalculateMaximumHealthPool(character)}";

			if (character.House == null)
			{
				return new EmbedFieldBuilder()
				{
					IsInline = false,
					Name = $"{character.CharacterName} ({character.GetAge(year.CurrentYear)})",
					Value = $"{strString}\n{endString}\n{dexString}\n{perString}\n{intString}\n{armorClassString}\n{abilityPointString}\n{healthPoolString}"
				};
			}

			var embedFieldBuilder = new EmbedFieldBuilder()
			{
				IsInline = false,
				Name = $"{character.CharacterName} ({character.GetAge(year.CurrentYear)}) of House {character.House.HouseName}",
				Value = $"{strString}\n{endString}\n{dexString}\n{perString}\n{intString}\n{armorClassString}\n{abilityPointString}\n{healthPoolString}"
			};

			return embedFieldBuilder;
		}

		/// <summary>
		/// Calculates the armor class of a character, taking Traits into account.
		/// </summary>
		/// <param name="character"></param>
		/// <returns></returns>
		public int CalculateMaximumArmorClass(PlayerCharacter character)
		{
			var dexMod = _rollService.GetStatAsModifier(CharacterAttribute.DEX, character);

			var armorClassBonus = 0;

			if (character.House != null)
			{
				armorClassBonus = character.House.ArmorClassBonus;
			}

			foreach (var trait in character.CharacterTraits)
			{
				armorClassBonus += trait.Trait.ArmorClassBonus;
			}

			var armorClass = 10 + dexMod + armorClassBonus;

			//armor shit

			return armorClass;
		}

		public int CalculateMaximumAbilityPoint(PlayerCharacter character)
		{
			var hasDexBonus = _rollService.GetStatAsModifier(CharacterAttribute.DEX, character) > 1;

			var traitBonus = 0;

			if (character.House != null)
			{
				traitBonus = character.House.ArmorClassBonus;
			}

			foreach (var trait in character.CharacterTraits)
			{
				traitBonus += trait.Trait.AbilityPointBonus;
			}

			if (hasDexBonus)
			{
				traitBonus++;
			}

			return traitBonus + 6;
		}

		/// <summary>
		/// Calculates the maximum health pool of a character, taking Traits into account.
		/// </summary>
		/// <param name="character"></param>
		/// <returns></returns>
		public int CalculateMaximumHealthPool(PlayerCharacter character)
		{
			var strMod = _rollService.GetStatAsModifier(CharacterAttribute.STR, character);
			var endMod = _rollService.GetStatAsModifier(CharacterAttribute.END, character);

			var hp = 35;

			//if (strMod > 0)
			//{
            var bonus = strMod * 5;
            hp += bonus;
			//}
			//any% Blue Articuno Valkfinder Update speedrun WR

			//if (endMod > 0)
			//{
            bonus = endMod * 10;
            hp += bonus;
			//}

            if (hp < 10)
                hp = 10; //let's make sure that people with 1 END don't just suddenly drop dead

			foreach (var trait in character.CharacterTraits)
			{
				hp += trait.Trait.HealthPoolBonus;
			}

			if (character.House != null)
			{
				hp += character.House.HealthPoolBonus;
			}

			return hp;
		}
	}
}
