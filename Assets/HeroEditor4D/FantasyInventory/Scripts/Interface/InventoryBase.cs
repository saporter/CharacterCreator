using System;
using System.Collections.Generic;
using System.Linq;
using Assets.HeroEditor4D.Common.CommonScripts;
using Assets.HeroEditor4D.FantasyInventory.Scripts.Data;
using Assets.HeroEditor4D.FantasyInventory.Scripts.Enums;
using Assets.HeroEditor4D.FantasyInventory.Scripts.Interface.Elements;
using UnityEngine;
using UnityEngine.UI;

#if TAP_HEROES

using Assets.SimpleLocalization;
using Assets.TapHeroes.Scripts.Data;
using Assets.TapHeroes.Scripts.Enums;
using Assets.TapHeroes.Scripts.Interface;

#endif

namespace Assets.HeroEditor4D.FantasyInventory.Scripts.Interface
{
    /// <summary>
    /// High-level inventory interface.
    /// </summary>
    public class InventoryBase : ItemWorkspace
    {
	    public ItemCollection ItemCollection;
		public Equipment Equipment;
        public ScrollInventory Bag;
        public ScrollInventory Materials;
        public Button EquipButton;
        public Button RemoveButton;
        public Button CraftButton;
        public Button LearnButton;
        public Button UseButton;
        public AudioClip EquipSound;
        public AudioClip CraftSound;
        public AudioClip UseSound;
        public AudioSource AudioSource;
        public bool InitializeExample;
        
        public void Start()
        {
            if (InitializeExample)
            {
                Initialize();
                Reset();
            }
        }

        /// <summary>
        /// Initialize owned items (just for example).
        /// </summary>
        public void Initialize()
        {
            InventoryItem.OnLeftClick = SelectItem;
            InventoryItem.OnRightClick = InventoryItem.OnDoubleClick = OnDoubleClick;

            var inventory = ItemCollection.UserItems.Select(i => new Item(i.Id)).ToList(); // inventory.Clear();
			var equipped = new List<Item>();

            Bag.Initialize(ref inventory);
            Equipment.Initialize(ref equipped);
		}

        public void Initialize(ref List<Item> playerItems, ref  List<Item> equippedItems, int bagSize, Action onRefresh)
        {
            InventoryItem.OnLeftClick = SelectItem;
            InventoryItem.OnRightClick = InventoryItem.OnDoubleClick = OnDoubleClick;
            Bag.Initialize(ref playerItems);
            Equipment.Initialize(ref equippedItems);
            Equipment.SetBagSize(bagSize);
            Equipment.OnRefresh = onRefresh;

            if (!Equipment.SelectAny() && !Bag.SelectAny())
            {
                ItemInfo.Reset();
            }
        }

        private void OnDoubleClick(Item item)
        {
            SelectItem(item);

            if (Equipment.Items.Contains(item))
            {
                Remove();
            }
            else if (CanEquip())
            {
                Equip();
            }
        }

        public void SelectItem(Item item)
        {
            SelectedItem = item;
            ItemInfo.Initialize(SelectedItem);
            Refresh();
        }

        public void Equip()
        {
            #if TAP_HEROES

            SkillId? skillId = null;

            switch (SelectedItem.Params.Type)
            {
                case ItemType.Weapon:
                    switch (SelectedItem.Params.Class)
                    {
                        case ItemClass.Dagger: skillId = SkillId.DaggerMastery; break;
                        case ItemClass.Sword: skillId = SkillId.SwordMastery; break;
                        case ItemClass.Axe: skillId = SkillId.AxeMastery; break;
                        case ItemClass.Blunt: skillId = SkillId.BluntMastery; break;
                        case ItemClass.Wand: skillId = SkillId.WandMastery; break;
                        case ItemClass.Bow: skillId = SkillId.BowMastery; break;
                        case ItemClass.Pickaxe: break;
                        default: throw new NotSupportedException("Unsupported class.");
                    }
                    break;
                case ItemType.Armor:
                case ItemType.Helmet:
                    switch (SelectedItem.Params.Class)
                    {
                        case ItemClass.Light: skillId = SkillId.LightArmorMastery; break;
                        case ItemClass.Heavy: skillId = SkillId.HeavyArmorMastery; break;
                        default: throw new NotSupportedException("Unsupported class.");
                    }
                    break;
                case ItemType.Shield:
                    skillId = SkillId.Blocking; break;
            }

            if (skillId != null && SelectedItem.Params.Level > 0 && (!Profile.Hero.Skills.ContainsKey(skillId.Value) || Profile.Hero.Skills[skillId.Value] < SelectedItem.Params.Level - 1))
            {
                Dialog.Instance.ShowMessage(LocalizationManager.Localize("Inventory.GradeLocked", $"<color=yellow>{SelectedItem.Params.Grade}</color>", LocalizationManager.Localize("ItemType." + SelectedItem.Params.Type).ToLower(), $"<color=yellow>{LocalizationManager.Localize($"Skill.{skillId.Value}.Name")}</color>"));
                return;
            }

            #endif

            var equipped = Equipment.Items.LastOrDefault(i => i.Params.Type == SelectedItem.Params.Type);

            if (equipped != null)
            {
                AutoRemove(SelectedItem.Params.Type, Equipment.Slots.Count(i => i.Type == SelectedItem.Params.Type));
            }

            if (SelectedItem.IsTwoHanded) AutoRemove(ItemType.Shield, 1);
            if (SelectedItem.IsShield && Equipment.Items.Any(i => i.IsTwoHanded)) AutoRemove(ItemType.Weapon, 1);

            if (SelectedItem.Params.Tags.Contains(ItemTag.TwoHanded))
            {
                var shield = Equipment.Items.SingleOrDefault(i => i.Params.Type == ItemType.Shield);

                if (shield != null)
                {
                    MoveItem(shield, Equipment, Bag);
                }
            }
            else if (SelectedItem.Params.Type == ItemType.Shield)
            {
                var weapon2H = Equipment.Items.SingleOrDefault(i => i.Params.Tags.Contains(ItemTag.TwoHanded));

                if (weapon2H != null)
                {
                    MoveItem(weapon2H, Equipment, Bag);
                }
            }

            MoveItem(SelectedItem, Bag, Equipment);
            AudioSource.PlayOneShot(EquipSound, SfxVolume);
        }

        public void Remove()
        {
            MoveItem(SelectedItem, Equipment, Bag);
            SelectItem(SelectedItem);
            AudioSource.PlayOneShot(EquipSound, SfxVolume);
        }

        public void Craft()
        {
            var materials = MaterialList;

            if (CanCraft(materials))
            {
                materials.ForEach(i => Bag.Items.Single(j => j.Hash == i.Hash).Count -= i.Count);
                Bag.Items.RemoveAll(i => i.Count == 0);

                var itemId = SelectedItem.Params.FindProperty(PropertyId.Craft).Value;
                var existed = Bag.Items.SingleOrDefault(i => i.Id == itemId && i.Modifier == null);

                if (existed == null)
                {
                    Bag.Items.Add(new Item(itemId));
                }
                else
                {
                    existed.Count++;
                }

                Bag.Refresh(SelectedItem);
                CraftButton.interactable = CanCraft(materials);
                AudioSource.PlayOneShot(CraftSound, SfxVolume);

                #if TAP_HEROES

                TapHeroes.Scripts.Service.Events.Event("CraftItem", "Item", itemId);

                #endif
            }
            else
            {
                Debug.Log("No materials.");
            }
        }

        public void Learn()
        {
            // Implement your logic here!
        }

        public void Use()
        {
            #if TAP_HEROES

            TapHeroes.Scripts.Data.Profile.Hero.Boosters.Clear();
            TapHeroes.Scripts.Data.Profile.Hero.Boosters.Add(new Item(SelectedItem.Id));

            #endif

            if (SelectedItem.Count == 1)
            {
                Bag.Items.Remove(SelectedItem);
                SelectedItem = Bag.Items.FirstOrDefault();

                if (SelectedItem == null)
                {
                    Bag.Refresh(null);
                    SelectedItem = Equipment.Items.FirstOrDefault();

                    if (SelectedItem != null)
                    {
                        Equipment.Refresh(SelectedItem);
                    }
                }
                else
                {
                    Bag.Refresh(SelectedItem);
                }
            }
            else
            {
                SelectedItem.Count--;
                Bag.Refresh(SelectedItem);
            }

            Equipment.OnRefresh?.Invoke();
            AudioSource.PlayOneShot(UseSound, SfxVolume);
        }

        public override void Refresh()
        {
            if (SelectedItem == null)
            {
                ItemInfo.Reset();
                EquipButton.SetActive(false);
                RemoveButton.SetActive(false);
            }
            else
            {
                var equipped = Equipment.Items.Contains(SelectedItem);

                EquipButton.SetActive(!equipped && CanEquip());
                RemoveButton.SetActive(equipped);
                UseButton.SetActive(CanUse());
            }

            var receipt = SelectedItem != null && SelectedItem.Params.Type == ItemType.Receipt;

            if (CraftButton != null) CraftButton.SetActive(false);
            if (LearnButton != null) LearnButton.SetActive(false);

            if (receipt)
            {
                if (LearnButton == null)
                {
                    var materialSelected = !Bag.Items.Contains(SelectedItem) && !Equipment.Items.Contains(SelectedItem);

                    CraftButton.SetActive(true);
                    Materials.SetActive(materialSelected);
                    Equipment.Grid.parent.SetActive(!materialSelected);

                    var materials = MaterialList;

                    Materials.Initialize(ref materials);
                }
                else
                {
                    LearnButton.SetActive(true);
                }
            }
        }

        private List<Item> MaterialList => SelectedItem.Params.FindProperty(PropertyId.Materials).Value.Split(',').Select(i => i.Split(':')).Select(i => new Item(i[0], int.Parse(i[1]))).ToList();

        private bool CanEquip()
        {
            return Bag.Items.Contains(SelectedItem) && Equipment.Slots.Any(i => i.Type == SelectedItem.Params.Type && (i.Class == ItemClass.Unknown || i.Class == SelectedItem.Params.Class)) && SelectedItem.Params.Class != ItemClass.Booster;
        }

        private bool CanUse()
        {
            return SelectedItem.Params.Class == ItemClass.Booster;
        }

        private bool CanCraft(List<Item> materials)
        {
            return materials.All(i => Bag.Items.Any(j => j.Hash == i.Hash && j.Count >= i.Count));
        }

        /// <summary>
        /// Automatically removes items if target slot is busy.
        /// </summary>
        private void AutoRemove(ItemType itemType, int max)
        {
            var items = Equipment.Items.Where(i => i.Params.Type == itemType).ToList();
            long sum = 0;

            foreach (var p in items)
            {
                sum += p.Count;
            }

            if (sum == max)
            {
                MoveItem(items.LastOrDefault(i => i != SelectedItem) ?? items.Last(), Equipment, Bag, silent: true);
            }
        }
    }
}