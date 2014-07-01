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
        bool foldoutOpen;

        void Awake()
        {
            worldManager = (WorldManager)target;
        }

        public override void OnInspectorGUI()
        {
            worldManager.keepSymmetrical = EditorGUILayout.Toggle("Keep Symmetrical", worldManager.keepSymmetrical);
            worldManager.useCivGridCamera = EditorGUILayout.Toggle("Use Default CivGrid Camera", worldManager.useCivGridCamera);
            worldManager.useWorldTypeValues = EditorGUILayout.Toggle("Use Preset World Values", worldManager.useWorldTypeValues);

            if (worldManager.useWorldTypeValues == false)
            {
                worldManager.noiseScale = EditorGUILayout.FloatField("Noise Scale", worldManager.noiseScale);
            }
            else
            {
                worldManager.worldType = (WorldType)EditorGUILayout.EnumPopup("World Type", worldManager.worldType);
            }
            worldManager.mapSize = EditorGUILayout.Vector2Field("Map Size", worldManager.mapSize);
            worldManager.chunkSize = EditorGUILayout.IntField("Chunk Size", worldManager.chunkSize);

            if (((worldManager.mapSize.x % worldManager.chunkSize) + (worldManager.mapSize.y % worldManager.chunkSize)) != 0)
            {
                EditorGUILayout.HelpBox("Map Size must be divisible by Chunk Size", MessageType.Error);
            }

            worldManager.hexRadiusSize = EditorGUILayout.FloatField("Hex Radius Size", worldManager.hexRadiusSize);
            worldManager.mountainMap = (Texture2D)EditorGUILayout.ObjectField("Base Mountain Heightmap", worldManager.mountainMap, typeof(Texture2D), false);

            //base.OnInspectorGUI();
        }

        protected override void OnHeaderGUI()
        {
            EditorGUILayout.SelectableLabel("LOL");
        }
    }
}