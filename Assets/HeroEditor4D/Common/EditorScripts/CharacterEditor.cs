using System;
using System.Collections.Generic;
using System.Linq;
using Assets.HeroEditor4D.Common.CharacterScripts;
using Assets.HeroEditor4D.Common.CommonScripts;
using Assets.HeroEditor4D.FantasyInventory.Scripts.Data;
using Assets.HeroEditor4D.FantasyInventory.Scripts.Interface.Elements;
using Assets.HeroEditor4D.SimpleColorPicker.Scripts;
using HeroEditor.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.HeroEditor4D.Common.EditorScripts
{
    /// <summary>
    /// Character editor UI and behaviour.
    /// </summary>
    public class CharacterEditor : CharacterEditorBase
    {
        [Header("Public")]
        public Transform Tabs;
        public ScrollInventory Inventory;
        public Text ItemName;

        [Header("Other")]
        public List<GameObject> ColorButtons;
        public ColorPicker ColorPicker;
        public string PrefabFolder;

        public Action<Item> EquipCallback;

        public Character4D Character4D => (Character4D) Character;

        public void OnValidate()
        {
            if (Character == null)
            {
                Character = FindObjectOfType<Character4D>();
            }
        }

        public new void Start()
        {
            base.Start();

            if (Tabs.gameObject.activeSelf)
            {
                Tabs.GetComponentInChildren<Toggle>().isOn = true;
            }
        }

        /// <summary>
        /// This can be used as an example for building your own inventory UI.
        /// </summary>
        public void OnSelectTab(bool value)
        {
            if (!value) return;

            Item.GetParams = null;

            Dictionary<string, SpriteGroupEntry> dict;
            Action<Item> equipAction;
            int equippedIndex;
            var tab = Tabs.GetComponentsInChildren<Toggle>().Single(i => i.isOn);

            switch (tab.name)
            {
                case "Armor":
                {
                    dict = SpriteCollection.Armor.ToDictionary(i => i.FullName, i => i);
                    equipAction = item => Character4D.EquipArmor(dict[item.Id]?.Sprites);
                    equippedIndex = Character.Front.Armor == null ? -1 : SpriteCollection.Armor.FindIndex(i => i.Sprites.SequenceEqual(Character.Front.Armor));
                    break;
                }
                case "Helmet":
                {
                    dict = SpriteCollection.Armor.ToDictionary(i => i.FullName, i => i);
                    equipAction = item => Character4D.EquipHelmet(dict[item.Id]);
                    equippedIndex = SpriteCollection.Armor.FindIndex(i => i.Sprites.Contains(Character.Front.Helmet));
                    Item.GetParams = item => new ItemParams { Id = item.Id, Path = dict[item.Id] == null ? null : dict[item.Id].Path.Replace("Armor/", "Helmet/") + ".Helmet", Meta = dict[item.Id]?.Id.ToString() };
                    break;
                }
                case "Shield":
                {
                    dict = SpriteCollection.Shield.ToDictionary(i => i.FullName, i => i);
                    equipAction = item => Character4D.EquipShield(dict[item.Id]?.Sprites);
                    equippedIndex = Character.Front.Shield == null ? -1 : SpriteCollection.Shield.FindIndex(i => i.Sprites.SequenceEqual(Character.Front.Shield));
                    break;
                }
                case "Melee1H":
                {
                    dict = SpriteCollection.MeleeWeapon1H.ToDictionary(i => i.FullName, i => i);
                    equipAction = item => Character4D.EquipMeleeWeapon1H(dict[item.Id]?.Sprite);
                    equippedIndex = SpriteCollection.MeleeWeapon1H.FindIndex(i => i.Sprites.Contains(Character.Front.PrimaryMeleeWeapon));
                    break;
                }
                case "Melee2H":
                {
                    dict = SpriteCollection.MeleeWeapon2H.ToDictionary(i => i.FullName, i => i);
                    equipAction = item => Character4D.EquipMeleeWeapon2H(dict[item.Id]?.Sprite);
                    equippedIndex = SpriteCollection.MeleeWeapon2H.FindIndex(i => i.Sprites.Contains(Character.Front.PrimaryMeleeWeapon));
                    break;
                }
                case "Bow":
                {
                    dict = SpriteCollection.Bow.ToDictionary(i => i.FullName, i => i);
                    equipAction = item => Character4D.EquipBow(dict[item.Id]?.Sprites);
                    equippedIndex = Character.Front.CompositeWeapon == null ? -1 : SpriteCollection.Bow.FindIndex(i => i.Sprites.SequenceEqual(Character.Front.CompositeWeapon));
                    break;
                }
                case "Crossbow":
                {
                    dict = SpriteCollection.Crossbow.ToDictionary(i => i.FullName, i => i);
                    equipAction = item => Character4D.EquipCrossbow(dict[item.Id]?.Sprites);
                    equippedIndex = Character.Front.CompositeWeapon == null ? -1 : SpriteCollection.Crossbow.FindIndex(i => i.Sprites.SequenceEqual(Character.Front.CompositeWeapon));
                    break;
                }
                case "Ears":
                {
                    dict = SpriteCollection.Ears.ToDictionary(i => i.FullName, i => i);
                    equipAction = item => Character4D.SetEars(dict[item.Id]?.Sprites);
                    equippedIndex = SpriteCollection.Ears.FindIndex(i => i.Sprites.SequenceEqual(Character.Front.Ears));
                    break;
                }
                case "Eyebrows":
                {
                    dict = SpriteCollection.Eyebrows.ToDictionary(i => i.FullName, i => i);
                    equipAction = item => Character4D.SetEyebrows(dict[item.Id]?.Sprites);
                    equippedIndex = SpriteCollection.Eyebrows.FindIndex(i => i.Sprites.Contains(Character.Front.Expressions[0].Eyebrows));
                    break;
                }
                case "Eyes":
                {
                    dict = SpriteCollection.Eyes.ToDictionary(i => i.FullName, i => i);
                    equipAction = item => Character4D.SetEyes(dict[item.Id]?.Sprites);
                    equippedIndex = SpriteCollection.Eyes.FindIndex(i => i.Sprites.Contains(Character.Front.Expressions[0].Eyes));
                    break;
                }
                case "Hair":
                {
                    dict = SpriteCollection.Hair.ToDictionary(i => i.FullName, i => i);
                    equipAction = item => Character4D.SetHair(dict[item.Id]);
                    equippedIndex = SpriteCollection.Hair.FindIndex(i => i.Sprites.Contains(Character.Front.Hair));
                    break;
                }
                case "Mouth":
                {
                    dict = SpriteCollection.Mouth.ToDictionary(i => i.FullName, i => i);
                    equipAction = item => Character4D.SetMouth(dict[item.Id]?.Sprites);
                    equippedIndex = SpriteCollection.Mouth.FindIndex(i => i.Sprites.Contains(Character.Front.Expressions[0].Mouth));
                    break;
                }
                default:
                    throw new NotImplementedException(tab.name);
            }

            var items = dict.Values.Select(i => new Item(i.FullName)).ToList();

            items.Insert(0, new Item("Empty"));
            dict.Add("Empty", null);

            if (Item.GetParams == null)
            {
                Item.GetParams = item => new ItemParams { Id = item.Id, Path = dict[item.Id]?.Path, Meta = dict[item.Id]?.Id.ToString() }; // We override GetParams method because we don't have a database with item params.
            }

            InventoryItem.OnLeftClick = item => { equipAction(item); EquipCallback?.Invoke(item); ItemName.text = item.Params.Id ?? "Empty"; };

            Inventory.Initialize(ref items, items[equippedIndex + 1], reset: true);

            ColorButtons[0].SetActive(tab.name == "Hair");
            ColorButtons[1].SetActive(tab.name == "Eyes");
            ColorButtons[2].SetActive(tab.name == "Body");

            if (ColorButtons.Count == 4) ColorButtons[3].SetActive(!ColorButtons[0].activeSelf && !ColorButtons[1].activeSelf && !ColorButtons[2].activeSelf);
        }

		/// <summary>
        /// Remove all equipment.
        /// </summary>
        public void Reset()
        {
            Character.Parts.ForEach(i => i.ResetEquipment());
	        InitializeDropdowns();
        }

        #if UNITY_EDITOR

        /// <summary>
        /// Save character to prefab.
        /// </summary>
        public void Save()
        {
            PrefabFolder = UnityEditor.EditorUtility.SaveFilePanel("Save character prefab", PrefabFolder, "New character", "prefab");

	        if (PrefabFolder.Length > 0)
	        {
		        Save("Assets" + PrefabFolder.Replace(Application.dataPath, null));
	        }
		}

	    /// <summary>
		/// Load character from prefab.
		/// </summary>
		public void Load()
        {
	        PrefabFolder = UnityEditor.EditorUtility.OpenFilePanel("Load character prefab", PrefabFolder, "prefab");

            if (PrefabFolder.Length > 0)
            {
                Load("Assets" + PrefabFolder.Replace(Application.dataPath, null));
            }

			//FeatureTip();
		}

	    /// <summary>
	    /// Save character to json.
	    /// </summary>
	    public void SaveToJson()
	    {
		    PrefabFolder = UnityEditor.EditorUtility.SaveFilePanel("Save character to json", PrefabFolder, "New character", "json");

		    if (PrefabFolder.Length > 0)
		    {
			    var path = "Assets" + PrefabFolder.Replace(Application.dataPath, null);
			    var json = Character.ToJson();

			    System.IO.File.WriteAllText(path, json);
			    Debug.LogFormat("Json saved to {0}: {1}", path, json);
		    }

		    //FeatureTip();
		}

		/// <summary>
		/// Load character from json.
		/// </summary>
		public void LoadFromJson()
	    {
		    PrefabFolder = UnityEditor.EditorUtility.OpenFilePanel("Load character from json", PrefabFolder, "json");

		    if (PrefabFolder.Length > 0)
		    {
				var path = "Assets" + PrefabFolder.Replace(Application.dataPath, null);
			    var json = System.IO.File.ReadAllText(path);

				Character.LoadFromJson(json, silent: false);
			}

		    //FeatureTip();
	    }

		public override void Save(string path)
		{
			Character.transform.localScale = Vector3.one;

			#if UNITY_2018_3_OR_NEWER

			UnityEditor.PrefabUtility.SaveAsPrefabAsset(Character.gameObject, path);

			#else

			UnityEditor.PrefabUtility.CreatePrefab(path, Character.gameObject);

			#endif

            Debug.LogFormat("Prefab saved as {0}", path);
        }

        public override void Load(string path)
        {
			var character = UnityEditor.AssetDatabase.LoadAssetAtPath<Character4D>(path);

	        //Character.GetComponent<Character>().Firearm.Params = character.Firearm.Params; // TODO: Workaround
			Load(character);
			//FindObjectOfType<CharacterBodySculptor>().OnCharacterLoaded(character);
        }

        #else

        public override void Save(string path)
        {
            throw new System.NotSupportedException();
        }

        public override void Load(string path)
        {
            throw new System.NotSupportedException();
        }

        #endif

        private Color _original;

        public Action OpenColorPicker = () => FindObjectOfType<CharacterEditor>().ColorPicker.SetActive(true); // Can be overridden.
        public Action CloseColorPicker = () => FindObjectOfType<CharacterEditor>().ColorPicker.SetActive(false);

        protected override void OpenPalette(Color selected)
        {
            ColorPicker.Color = _original = selected;
            ColorPicker.OnColorChanged = PickColor;
            OpenColorPicker();
        }

        public void ClosePalette(bool apply)
        {
            if (!apply) PickColor(_original);

            CloseColorPicker();
        }

	    protected override void FeedbackTip()
	    {
			#if UNITY_EDITOR

		    var success = UnityEditor.EditorUtility.DisplayDialog("HeroView Editor", "Hi! Thank you for using my asset! I hope you enjoy making your game with it. The only thing I would ask you to do is to leave a review on the Asset Store. It would be awesome support for my asset, thanks!", "Review", "Later");
			
			RequestFeedbackResult(success, false);

			#endif
	    }
    }
}