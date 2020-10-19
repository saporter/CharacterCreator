using Assets.HeroEditor4D.FantasyInventory.Scripts;
using UnityEditor;
using UnityEngine;

namespace Assets.HeroEditor4D.FantasyInventory.Editor
{
    /// <summary>
    /// Add "Refresh" button to IconCollection script
    /// </summary>
    [CustomEditor(typeof(IconCollection))]
    public class IconCollectionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var collection = (IconCollection) target;

            if (GUILayout.Button("Refresh"))
            {
				collection.Refresh();
            }
        }
    }
}