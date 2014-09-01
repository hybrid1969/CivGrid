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
            EditorGUILayout.SelectableLabel("World Settings", EditorStyles.boldLabel);
            
            EditorGUI.indentLevel++;

            worldManager.generateOnStart = EditorGUILayout.Toggle("Generate World On Startup", worldManager.generateOnStart);
            worldManager.useCivGridCamera = EditorGUILayout.Toggle("Use Default CivGrid Camera", worldManager.useCivGridCamera);
            worldManager.generateNodeLocations = EditorGUILayout.Toggle("Generate Pathfinding Node Locations", worldManager.generateNodeLocations);
            worldManager.useWorldTypeValues = EditorGUILayout.Toggle("Use Preset World Values", worldManager.useWorldTypeValues);

            EditorGUILayout.Separator();

            if (worldManager.useWorldTypeValues == false)
            {
                worldManager.noiseScale = EditorGUILayout.FloatField("Noise Scale", worldManager.noiseScale);
            }
            else
            {
                worldManager.worldType = (WorldType)EditorGUILayout.EnumPopup("World Type", worldManager.worldType);
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.Separator();

            EditorGUILayout.SelectableLabel("Map Size", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;

            worldManager.mapSize = EditorGUILayout.Vector2Field("Map Size", worldManager.mapSize);
            worldManager.chunkSize = EditorGUILayout.IntField("Chunk Size", worldManager.chunkSize);

            if (((worldManager.mapSize.x % worldManager.chunkSize) + (worldManager.mapSize.y % worldManager.chunkSize)) != 0)
            {
                EditorGUILayout.HelpBox("Map Size must be divisible by Chunk Size", MessageType.Error);
            }

            worldManager.hexRadiusSize = EditorGUILayout.FloatField("Hex Radius Size", worldManager.hexRadiusSize);

            EditorGUI.indentLevel--;

            EditorGUILayout.SelectableLabel("Tile Settings", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            EditorGUILayout.SelectableLabel("Mountain Settings", EditorStyles.boldLabel);
            worldManager.mountainScaleY = EditorGUILayout.FloatField("Vertical Size", worldManager.mountainScaleY);
            worldManager.mountainHeightMap = (Texture2D)EditorGUILayout.ObjectField("Base Heightmap", worldManager.mountainHeightMap, typeof(Texture2D), false);
            EditorGUI.indentLevel--;

        }
    }
}