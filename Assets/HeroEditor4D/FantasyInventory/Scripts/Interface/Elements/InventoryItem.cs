using System;
using System.Collections;
using Assets.HeroEditor4D.FantasyInventory.Scripts.Data;
using Assets.HeroEditor4D.FantasyInventory.Scripts.Enums;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.HeroEditor4D.FantasyInventory.Scripts.Interface.Elements
{
    /// <summary>
    /// Represents inventory item and handles drag & drop operations.
    /// </summary>
    public class InventoryItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Image Icon;
        public Image Background;
        public Text Count;
        public GameObject Modificator;
        public Item Item;
        public Toggle Toggle;

        private float _clickTime;

        /// <summary>
        /// These actions should be set when inventory UI is opened.
        /// </summary>
        public static Action<Item> OnLeftClick;
		public static Action<Item> OnRightClick;
	    public static Action<Item> OnDoubleClick;
        public static Action<Item> OnMouseEnter;
        public static Action<Item> OnMouseExit;

        public void Start()
        {
            if (Icon != null)
            {
                var collection = IconCollection.Instances["FantasyHeroes"];

                Icon.sprite = collection.GetIcon(Item.Params.Path);

                #if TAP_HEROES

                switch (Item.Id)
                {
                    case "Gold": Background.sprite = collection.Backgrounds[5]; break;
                    case "Gemstone": Background.sprite = collection.Backgrounds[0]; break;
                    case "Exp": Background.sprite = collection.Backgrounds[2]; break;
                }

                #endif
            }

            if (Toggle)
            {
                Toggle.group = GetComponentInParent<ToggleGroup>();
            }

            if (Modificator)
            {
                var mod = Item.Modifier != null && Item.Modifier.Id != ItemModifier.None;

                Modificator.SetActive(mod);

                if (mod)
                {
                    Modificator.GetComponentInChildren<Text>().text = Item.Modifier.Id.ToString().ToUpper()[0].ToString();
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            StartCoroutine(OnPointerClickDelayed(eventData));
        }

        private IEnumerator OnPointerClickDelayed(PointerEventData eventData) // TODO: A workaround. We should wait for initializing other components.
        {
            yield return null;

            OnPointerClick(eventData.button);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnMouseEnter?.Invoke(Item);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnMouseExit?.Invoke(Item);
        }

        public void OnPointerClick(PointerEventData.InputButton button)
        {
            if (button == PointerEventData.InputButton.Left)
            {
                OnLeftClick?.Invoke(Item);

                var delta = Mathf.Abs(Time.time - _clickTime);

                if (delta < 0.5f) // If double click.
                {
                    _clickTime = 0;
                    OnDoubleClick?.Invoke(Item);
                }
                else
                {
                    _clickTime = Time.time;
                }
            }
            else if (button == PointerEventData.InputButton.Right)
            {
                OnRightClick?.Invoke(Item);
            }
        }

        public void Select(bool selected)
        {
            if (Toggle != null)
            {
                Toggle.isOn = selected;

                if (selected)
                {
                    OnLeftClick?.Invoke(Item);
                }
            }
        }
    }
}