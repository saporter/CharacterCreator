using Assets.HeroEditor4D.Common.CharacterScripts;
using Assets.HeroEditor4D.Common.CommonScripts;
using HeroEditor.Common;
using UnityEngine;

namespace Assets.HeroEditor4D.Common.ExampleScripts
{
	/// <summary>
	/// Changing equipment at runtime examples.
	/// </summary>
	public class RuntimeSetup : MonoBehaviour
	{
		public Character Character;

        /// <summary>
        /// Example call: SetBody("HeadScar", "Basic", "HumanMale", "Basic");
        /// </summary>
        public void SetBody(string headName, string headCollection, string bodyName, string bodyCollection)
		{
			var sprites = Character.SpriteCollection.Body.FindSprites(bodyName, bodyCollection);

            Character.SetBody(sprites);
		}

		public void EquipMeleeWeapon1H(string eqName, string collection)
		{
			var sprite = Character.SpriteCollection.MeleeWeapon1H.FindSprite(eqName, collection);
			
			Character.EquipMeleeWeapon(sprite);
		}

		public void EquipMeleeWeapon2H(string eqName, string collection)
		{
			var sprite = Character.SpriteCollection.MeleeWeapon2H.FindSprite(eqName, collection);

            Character.EquipMeleeWeapon(sprite, twoHanded: true);
		}

		public void EquipBow(string eqName, string collection)
		{
			var sprites = Character.SpriteCollection.Bow.FindSprites(eqName, collection);

            Character.EquipBow(sprites);
		}

		public void EquipShield(string eqName, string collection)
		{
			var sprites = Character.SpriteCollection.Shield.FindSprites(eqName, collection);

            Character.EquipShield(sprites);
		}

		public void EquipArmor(string eqName, string collection)
		{
			var sprites = Character.SpriteCollection.Armor.FindSprites(eqName, collection);

            Character.EquipArmor(sprites);
		}

		public void EquipHelmet(string eqName, string collection)
		{
			var sprites = Character.SpriteCollection.Armor.FindSprites(eqName, collection);
            var sprite = Character.HelmetRenderer.GetComponent<SpriteMapping>().FindSprite(sprites);

		    Character.EquipHelmet(sprite);
        }
	}
}