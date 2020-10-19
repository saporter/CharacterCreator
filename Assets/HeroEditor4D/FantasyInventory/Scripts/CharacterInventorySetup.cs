using System;
using System.Collections.Generic;
using System.Linq;
using Assets.HeroEditor4D.Common.CharacterScripts;
using Assets.HeroEditor4D.Common.ExampleScripts;
using Assets.HeroEditor4D.FantasyInventory.Scripts.Data;
using Assets.HeroEditor4D.FantasyInventory.Scripts.Enums;
using HeroEditor.Common;
using HeroEditor.Common.Enums;
using UnityEngine;

namespace Assets.HeroEditor4D.FantasyInventory.Scripts
{
    public class CharacterInventorySetup
    {
        public static void Setup(Character character, List<Item> equipped, CharacterAppearance appearance)
        {
            character.Underwear = character.SpriteCollection.Armor.Single(i => i.Name == appearance.Underwear).Sprites;
            character.UnderwearColor = appearance.UnderwearColor;
            appearance.Setup(character, initialize: false);
            Setup(character, equipped);
        }

        public static void Setup(Character character, List<Item> equipped)
        {
            character.ResetEquipment();
            character.HideEars = false;
            character.CropHair = false;

            foreach (var item in equipped)
            {
                try
                {
                    switch (item.Params.Type)
                    {
                        case ItemType.Weapon:
                            if (item.IsBow)
                            {
                                character.CompositeWeapon = character.SpriteCollection.Bow.FindSprites(item.Params.Path);
                                character.WeaponType = WeaponType.Bow;
                            }
                            else
                            {
                                character.WeaponType = item.Params.Tags.Contains(ItemTag.TwoHanded) ? WeaponType.Melee2H : WeaponType.Melee1H;
                                character.PrimaryMeleeWeapon = (character.WeaponType == WeaponType.Melee1H ? character.SpriteCollection.MeleeWeapon1H : character.SpriteCollection.MeleeWeapon2H).FindSprite(item.Params.Path);
                            }
                            break;
                        case ItemType.Shield:
                            character.Shield = character.SpriteCollection.Shield.FindSprites(item.Params.Path);
                            character.WeaponType = WeaponType.Melee1H;
                            break;
                        case ItemType.Armor:
                            character.Armor = character.SpriteCollection.Armor.FindSprites(item.Params.Path);
                            break;
                        case ItemType.Helmet:
                            var path = item.Params.Path.Replace(".Helmet", null).Replace("Helmet/", "Armor/");
                            var entry = character.SpriteCollection.Armor.Single(i => i.Path == path);
                            character.Helmet = character.HelmetRenderer.GetComponent<SpriteMapping>().FindSprite(entry.Sprites);
                            character.HideEars = !entry.Tags.Contains("ShowEars");
                            character.CropHair = !entry.Tags.Contains("FullHair");
                            break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("Unable to equip {0} ({1})", item.Params.Path, e.Message);
                }
            }

            if (equipped.Any(i => i.Params.Type == ItemType.Armor))
            {
                character.ArmorRenderers.ForEach(i => i.color = Color.white);
            }
            else if (character.Underwear.Any())
            {
                character.Armor = character.Underwear.ToList();
                character.ArmorRenderers.ForEach(i => i.color = character.UnderwearColor);
            }

            character.Initialize();
        }
    }
}