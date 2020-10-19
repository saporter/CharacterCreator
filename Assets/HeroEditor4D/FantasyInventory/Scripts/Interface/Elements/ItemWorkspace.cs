using System.Linq;
using Assets.HeroEditor4D.FantasyInventory.Scripts.Data;
using UnityEngine;

namespace Assets.HeroEditor4D.FantasyInventory.Scripts.Interface.Elements
{
    /// <summary>
    /// Abstract item workspace. It can be shop or player inventory. Items can be managed here (selected, moved and so on).
    /// </summary>
    public abstract class ItemWorkspace : MonoBehaviour
    {
        public ItemInfo ItemInfo;

        public static float SfxVolume = 1;

        public Item SelectedItem { get; protected set; }
        
        public abstract void Refresh();

        protected void Reset()
        {
            SelectedItem = null;
            ItemInfo.Reset();
        }

        protected void MoveItem(Item item, ItemContainer from, ItemContainer to, bool silent = false)
        {
            if (to.Expanded)
            {
                to.Items.Add(new Item(item.Id, item.Modifier));
            }
            else
            {
                var target = to.Items.SingleOrDefault(i => i.Hash == item.Hash);

                if (target == null)
                {
                    to.Items.Add(new Item(item.Id, item.Modifier));
                }
                else
                {
                    target.Count++;
                }
            }

            var moved = to.Items.FirstOrDefault(i => i.Hash == item.Hash);

            if (from.Expanded)
            {
                from.Items.Remove(item);
                
                if (!silent) SelectedItem = moved;
            }
            else
            {
                if (item.Count > 1)
                {
                    item.Count--;
                }
                else
                {
                    from.Items.Remove(item);
                    
                    if (!silent) SelectedItem = moved;
                }
            }

            if (!silent)
            {
                Refresh();
                from.Refresh(SelectedItem);
                to.Refresh(SelectedItem);
            }
        }
    }
}