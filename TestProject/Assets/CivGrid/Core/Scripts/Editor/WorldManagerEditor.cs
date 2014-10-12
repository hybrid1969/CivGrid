using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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

            if((worldManager.chunkSize % 2) != 0)
            {
                EditorGUILayout.HelpBox("Chunk Size must be an even number", MessageType.Error);
            }

            worldManager.hexRadiusSize = EditorGUILayout.FloatField("Hex Radius Size", worldManager.hexRadiusSize);

            EditorGUI.indentLevel--;


            EditorGUILayout.SelectableLabel("Tile Settings", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            worldManager.levelOfDetail = EditorGUILayout.IntSlider("Level of Detail", worldManager.levelOfDetail, 0, 3);
            worldManager.LOD2 = (Mesh)EditorGUILayout.ObjectField("LOD 2", worldManager.LOD2, typeof(Mesh), false);
            worldManager.LOD3 = (Mesh)EditorGUILayout.ObjectField("LOD 3", worldManager.LOD3, typeof(Mesh), false);
            EditorGUI.indentLevel--;

            EditorGUI.indentLevel++;
            EditorGUILayout.SelectableLabel("Hill Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            worldManager.hillMaximumHeight = EditorGUILayout.FloatField("Maximum Height", worldManager.hillMaximumHeight);
            worldManager.hillNoiseScale = EditorGUILayout.FloatField("Noise Scale", worldManager.hillNoiseScale);
            worldManager.hillNoiseSize = EditorGUILayout.FloatField("Noise Size", worldManager.hillNoiseSize);
            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;

            EditorGUI.indentLevel++;
            EditorGUILayout.SelectableLabel("Mountain Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            worldManager.mountainScaleY = EditorGUILayout.FloatField("Vertical Size", worldManager.mountainScaleY);
            worldManager.mountainMaximumHeight = EditorGUILayout.FloatField("Maximum Height", worldManager.mountainMaximumHeight);
            worldManager.mountainNoiseScale = EditorGUILayout.FloatField("Noise Scale", worldManager.mountainNoiseScale);
            worldManager.mountainNoiseSize = EditorGUILayout.FloatField("Noise Size", worldManager.mountainNoiseSize);
			worldManager.mountainHeightMap = (Texture2D)EditorGUILayout.ObjectField("Base Heightmap", worldManager.mountainHeightMap, typeof(Texture2D), false);
            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;

            EditorGUILayout.SelectableLabel("Border Settings", EditorStyles.boldLabel);
            
            EditorGUI.indentLevel++;
            worldManager.borderTexture = (Texture2D)EditorGUILayout.ObjectField("Border Texture", worldManager.borderTexture, typeof(Texture2D), false);
            worldManager.sprShDefBorders = (BorderTextureData)EditorGUILayout.ObjectField("Border Definitions", worldManager.sprShDefBorders, typeof(BorderTextureData), false);
            if(GUILayout.Button("Add New Border Define"))
            {
                worldManager.borderColors.Add(Color.black);
            }
            for(int i = 0; i < worldManager.borderColors.Count; i++)
            {
                worldManager.borderColors[i] = EditorGUILayout.ColorField("Border Color " + i, worldManager.borderColors[i]);
            }
            
            EditorGUI.indentLevel--;

        }
    }
}