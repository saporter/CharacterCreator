using Assets.HeroEditor4D.Common.CommonScripts;
using Assets.HeroEditor4D.FantasyInventory.Scripts.Enums;
using UnityEngine;

namespace Assets.HeroEditor4D.FantasyInventory.Scripts.Interface.Elements
{
    /// <summary>
    /// Represents equipment slot. Inventory items can be placed here.
    /// </summary>
    public class ItemSlot : MonoBehaviour
    {
        public ItemType Type;
        public ItemClass Class;

        public void OnValidate()
        {
            if (gameObject.activeSelf)
            {
                Type = name == "Slot" ? ItemType.Undefined : name.ToEnum<ItemType>();
            }
        }
    }
}