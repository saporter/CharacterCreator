using System;
using System.Collections.Generic;
using System.Linq;
using Assets.HeroEditor4D.Common.CharacterScripts;
using Assets.HeroEditor4D.Common.CommonScripts;
using Assets.HeroEditor4D.FantasyInventory.Scripts.Data;
using Assets.HeroEditor4D.FantasyInventory.Scripts.Enums;
using Assets.HeroEditor4D.FantasyInventory.Scripts.Interface.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.HeroEditor4D.FantasyInventory.Scripts.Interface
{
    /// <summary>
    /// High-level shop interface.
    /// </summary>
    public class ShopBase : ItemWorkspace
    {
        public ScrollInventory Trader;
        public ScrollInventory Bag;
        public Button BuyButton;
        public Button SellButton;
        public AudioSource AudioSource;
        public AudioClip TradeSound;
        public AudioClip NoMoney;
        public Character Dummy;
        public bool CanBuy = true;
        public bool CanSell = true;
        public bool ExampleInitialize;

        public string CurrencyId = "Gold";
        public const float SellRatio = 2;
	    public Action<Item> OnRefresh; // Can be used to customize shop behaviour;
        public Action<Item> OnBuy;
        public Action<Item> OnSell;

        public void Start()
        {
            if (ExampleInitialize)
            {
                Initialize();
            }
        }

        public void Subscribe()
        {
            InventoryItem.OnLeftClick = SelectItem;
            InventoryItem.OnRightClick = InventoryItem.OnDoubleClick = item => { SelectItem(item); if (Trader.Items.Contains(item)) Buy(); else Sell(); };
        }

        /// <summary>
        /// Initialize owned items and trader items (just for example).
        /// </summary>
        public void Initialize()
        {
            var inventory = new List<Item> { new Item(CurrencyId, 10000) };
            var shop = ItemCollection.Instance.Dict.Values.Select(i => new Item(i.Id, 2)).ToList();

            shop.Single(i => i.Id == CurrencyId).Count = 99999;

            Subscribe();
            Trader.Initialize(ref shop);
            Bag.Initialize(ref inventory);

            if (!Trader.SelectAny() && !Bag.SelectAny())
            {
                ItemInfo.Reset();
            }
        }

        public void Initialize(ref List<Item> traderItems, ref List<Item> playerItems)
        {
            Subscribe();
            Trader.Initialize(ref traderItems);
            Bag.Initialize(ref playerItems);

            if (!Trader.SelectAny() && !Bag.SelectAny())
            {
                ItemInfo.Reset();
            }
        }

	    public void SelectItem(Item item)
        {
            var trader = Trader.Items.Contains(item);

            SelectedItem = item;
            ItemInfo.Initialize(SelectedItem, trader);
            Refresh();
        }

        public void Buy()
        {
			if (!BuyButton.gameObject.activeSelf || !BuyButton.interactable || !CanBuy) return;

            if (GetCurrency(Bag, CurrencyId) < SelectedItem.Params.Price)
            {
                AudioSource.PlayOneShot(NoMoney, SfxVolume);

                #if TAP_HEROES

                TapHeroes.Scripts.Interface.Dialog.Instance.ShowMessage(SimpleLocalization.LocalizationManager.Localize("Trader.NoFunds." + CurrencyId));

                #else

                Debug.LogWarning("You don't have enough gold!");

                #endif

                return;
            }

            AddMoney(Bag, -SelectedItem.Params.Price, CurrencyId);
			AddMoney(Trader, SelectedItem.Params.Price, CurrencyId);
			MoveItem(SelectedItem, Trader, Bag);
            AudioSource.PlayOneShot(TradeSound, SfxVolume);
            OnBuy?.Invoke(SelectedItem);
        }

        public void Sell()
        {
	        if (!SellButton.gameObject.activeSelf || !SellButton.interactable || !CanSell) return;

            var price = Mathf.CeilToInt(SelectedItem.Params.Price / SellRatio);

            if (GetCurrency(Trader, CurrencyId) < price)
            {
                AudioSource.PlayOneShot(NoMoney, SfxVolume);
                
                #if TAP_HEROES

                TapHeroes.Scripts.Interface.Dialog.Instance.ShowMessage(SimpleLocalization.LocalizationManager.Localize("Trader.NoFunds." + CurrencyId));

                #else

                Debug.LogWarning("Trader doesn't have enough gold!");

                #endif

                return;
            }

            AddMoney(Bag, price, CurrencyId);
            AddMoney(Trader, -price, CurrencyId);
            MoveItem(SelectedItem, Bag, Trader);
            AudioSource.PlayOneShot(TradeSound, SfxVolume);
            OnSell?.Invoke(SelectedItem);
        }

        public override void Refresh()
        {
            if (SelectedItem == null)
            {
                ItemInfo.Reset();
                BuyButton.SetActive(false);
                SellButton.SetActive(false);
            }
            else
            {
                if (Trader.Items.Contains(SelectedItem))
                {
                    InitBuy();
                }
                else if (Bag.Items.Contains(SelectedItem))
                {
                    InitSell();
                }
                else if (Trader.Items.Any(i => i.Hash == SelectedItem.Hash))
                {
                    InitBuy();
                }
                else if (Bag.Items.Any(i => i.Hash == SelectedItem.Hash))
                {
                    InitSell();
                }
            }

            OnRefresh?.Invoke(SelectedItem);
        }

        private void InitBuy()
        {
            BuyButton.SetActive(!SelectedItem.Params.Tags.Contains(ItemTag.NotForSale) && CanBuy);
            SellButton.SetActive(false);
            //BuyButton.interactable = GetCurrency(Bag, CurrencyId) >= SelectedItem.Params.Price;
        }

        private void InitSell()
        {
            BuyButton.SetActive(false);
            SellButton.SetActive(!SelectedItem.Params.Tags.Contains(ItemTag.NotForSale) && SelectedItem.Id != CurrencyId && CanSell);
            //SellButton.interactable = GetCurrency(Trader, CurrencyId) >= SelectedItem.Params.Price;
        }

        public static long GetCurrency(ItemContainer bag, string currencyId)
        {
            var currency = bag.Items.SingleOrDefault(i => i.Id == currencyId);

            return currency?.Count ?? 0;
        }

        private static void AddMoney(ItemContainer inventory, int value, string currencyId)
        {
            var currency = inventory.Items.SingleOrDefault(i => i.Id == currencyId);

            if (currency == null)
            {
                inventory.Items.Insert(0, new Item(currencyId, value));
            }
            else
            {
                currency.Count += value;

                if (currency.Count == 0)
                {
                    inventory.Items.Remove(currency);
                }
            }
        }
    }
}