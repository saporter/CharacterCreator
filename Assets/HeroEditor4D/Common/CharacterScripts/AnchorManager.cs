using System.Collections.Generic;
using UnityEngine;

namespace Assets.HeroEditor4D.Common.CharacterScripts
{
    public class AnchorManager : MonoBehaviour
    {
        public Transform Body;
        public Transform PrimaryWeapon;
        public Transform Bow;
        public Transform Shield;

        public List<Transform> Custom;
    }
}