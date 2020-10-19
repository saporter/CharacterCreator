using System;
using System.Collections.Generic;
using System.Linq;
using Assets.HeroEditor4D.Common.CharacterScripts;
using Assets.HeroEditor4D.FantasyInventory.Scripts.Data;
using Assets.HeroEditor4D.FantasyInventory.Scripts.Enums;
using UnityEngine;

namespace Assets.HeroEditor4D.FantasyInventory.Scripts.Interface.Elements
{
    /// <summary>
    /// Represents hero (player) equipment. Based on equipment slots.
    /// </summary>
    public class Equipment : ItemContainer
    {
        public Action OnRefresh;

        /// <summary>
        /// Defines what kinds of items can be equipped.
        /// </summary>
        public List<ItemSlot> Slots;

        /// <summary>
        /// Used as parent object for new item instances.
        /// </summary>
        public Transform Grid;

        /// <summary>
        /// Equipped items will be instantiated in front of equipment slots.
        /// </summary>
        public GameObject ItemPrefab;

	    /// <summary>
	    /// Character preview.
	    /// </summary>
		public Character Preview;

        private readonly List<InventoryItem> _inventoryItems = new List<InventoryItem>(); 

        public void OnValidate()
        {
            if (Application.isPlaying) return;

            Slots = GetComponentsInChildren<ItemSlot>(true).Where(i => i.gameObject.activeSelf).ToList();

            //if (Character == null)
            //{
            //    Character = FindObjectOfType<Character>();
            //}
        }

	    public void Start()
	    {
		    //Character.Animator.SetBool("Ready", false);
		}

        public void SetBagSize(int size)
        {
            //var supplies = GetComponentsInChildren<ItemSlot>(true).Where(i => i.Type == ItemType.Supply).ToList();

            //for (var i = 0; i < supplies.Count; i++)
            //{
            //    supplies[i].gameObject.SetActive(i < size);
            //}

            //Slots = supplies.Take(size).ToList();
        }

        public bool SelectAny()
        {
            if (_inventoryItems.Count > 0)
            {
                _inventoryItems[0].Select(true);

                return true;
            }

            return false;
        }

        public override void Refresh(Item selected)
        {
            var items = Slots.Select(FindItem).Where(i => i != null).ToList();

            Reset();

            foreach (var slot in Slots)
            {
                var item = FindItem(slot);

                slot.gameObject.SetActive(item == null);

                if (item == null) continue;

                var inventoryItem = Instantiate(ItemPrefab, Grid).GetComponent<InventoryItem>();

                inventoryItem.Item = item;
                inventoryItem.Count.text = null;
                inventoryItem.transform.position = slot.transform.position;
                inventoryItem.transform.SetSiblingIndex(slot.transform.GetSiblingIndex());
                
                if (SelectOnRefresh) inventoryItem.Select(item == selected);

                _inventoryItems.Add(inventoryItem);
            }

            if (Preview)
            {
                CharacterInventorySetup.Setup(Preview, items);
                Preview.Initialize();
            }

            OnRefresh?.Invoke();
        }

        private void Reset()
        {
            foreach (var inventoryItem in _inventoryItems)
            {
                Destroy(inventoryItem.gameObject);
            }

            _inventoryItems.Clear();
        }

        private Item FindItem(ItemSlot slot)
        {
            if (slot.Type == ItemType.Shield)
            {
                var twoHandedWeapon = Items.SingleOrDefault(i => i.Params.Type == ItemType.Weapon && i.Params.Tags.Contains(ItemTag.TwoHanded));

                if (twoHandedWeapon != null)
                {
                    return twoHandedWeapon;
                }
            }

            var index = Slots.Where(i => i.Type == slot.Type).ToList().IndexOf(slot);
            var items = Items.Where(i => i.Params.Type == slot.Type).ToList();

            return index < items.Count ? items[index] : null;
        }
    }
}