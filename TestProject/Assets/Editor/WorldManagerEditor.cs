using UnityEngine;
using System.Collections;
using UnityEditor;
using CivGrid;


namespace CivGrid.Editors
{
    [CustomEditor(typeof(WorldManager))]
    public class WorldManagerEditor : Editor
    {
        WorldManager worldManager;

        void Awake()
        {
            worldManager = (WorldManager)target;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Label("LOL WAT DA FUQ; FIX MEH STUPAHD");
            //base.OnInspectorGUI();
        }

        protected override void OnHeaderGUI()
        {
            EditorGUILayout.SelectableLabel("LOL");
        }
    }
}