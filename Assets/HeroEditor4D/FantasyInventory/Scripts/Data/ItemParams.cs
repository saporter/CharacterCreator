using System;
using System.Collections.Generic;
using System.Linq;
using Assets.HeroEditor4D.FantasyInventory.Scripts.Enums;
using UnityEngine;

namespace Assets.HeroEditor4D.FantasyInventory.Scripts.Data
{
    /// <summary>
    /// Represents generic item params (common for all items).
    /// </summary>
    [Serializable]
    public class ItemParams
    {
	    public string Id;
        public int Level;
        public ItemRarity Rarity;
        public ItemType Type;
        public ItemClass Class;
        public List<ItemTag> Tags = new List<ItemTag>();
        public List<Property> Properties = new List<Property>();
        public int Price;
        public int Weight;
        public ItemMaterial Material;
        public string Meta;
        public string Path;
        public List<LocalizedValue> Localization;

        public char Grade => (char) (65 + Level);

        public Property FindProperty(PropertyId id)
        {
            var target = Properties.SingleOrDefault(i => i.Id == id && i.Element == ElementId.Physic);

            return target;
        }

        public Property FindProperty(PropertyId id, ElementId element)
        {
            var target = Properties.SingleOrDefault(i => i.Id == id && i.Element == element);

            return target;
        }

        #if TAP_HEROES

        public string GetLocalizedName(string language)
        {
            if (Type == ItemType.Receipt)
            {
                return $"{Assets.SimpleLocalization.LocalizationManager.Localize("Common.Receipt")}: {new Item(FindProperty(PropertyId.Craft).Value).Params.GetLocalizedName(language)}";
            }

            var localized = Localization.SingleOrDefault(i => i.Language == language) ?? Localization.SingleOrDefault(i => i.Language == "English");
            
            if (localized == null)
            {
                Debug.LogError($"Translation missed: {Id}.");

                return "Translation missed";
            }

            return localized.Value;
        }

        #else

        public string GetLocalizedName(string language)
        {
            var localized = Localization.SingleOrDefault(i => i.Language == language) ?? Localization.SingleOrDefault(i => i.Language == "English");
            
            if (localized == null)
            {
                return Id;
            }

            return localized.Value;
        }

        #endif
    }
}