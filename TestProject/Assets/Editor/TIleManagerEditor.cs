using UnityEngine;
using System.Collections;
using UnityEditor;

namespace CivGrid.Editors
{
    [CustomEditor(typeof(TileManager))]
    public class TileManagerEditor : Editor
    {
        TileManager tileManager;
        bool[] foldoutOpen;

        public void Awake()
        {
            tileManager = (TileManager)target;
            foldoutOpen = new bool[tileManager.tiles.Count];
        }

        public override void OnInspectorGUI()
        {
            if (tileManager == null) { tileManager = (TileManager)target; }
            if (foldoutOpen == null || foldoutOpen.Length != tileManager.tiles.Count) { foldoutOpen = new bool[tileManager.tiles.Count]; }

            if (GUILayout.Button("Add New Tile"))
            {
                TileEditorWindow window = EditorWindow.CreateInstance<TileEditorWindow>();
                EditorWindow.GetWindow<TileEditorWindow>();
                window.editMode = false;
                window.tileIndexToEdit = 0;
            }

            if (tileManager.tiles != null)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < tileManager.tiles.Count; i++)
                {
                    Tile tile = tileManager.tiles[i];

                    EditorGUILayout.BeginHorizontal();

                    foldoutOpen[i] = EditorGUILayout.Foldout(foldoutOpen[i], tile.name);

                    if (GUILayout.Button("Edit"))
                    {
                        TileEditorWindow window = EditorWindow.CreateInstance<TileEditorWindow>();
                        EditorWindow.GetWindow<TileEditorWindow>();
                        window.editMode = true;
                        window.tileIndexToEdit = i;
                    }

                    if (GUILayout.Button("Remove"))
                    {
                        tileManager.DeleteTile(tile);
                    }

                    EditorGUILayout.EndHorizontal();

                    if (foldoutOpen[i])
                    {
                        tile.name = EditorGUILayout.TextField("Name:", tile.name);
                        tile.isWater = EditorGUILayout.Toggle("Is Water:", tile.isWater);
                        tile.topLat = EditorGUILayout.FloatField("Top Lattitude:", tile.topLat);
                        tile.bottomLat = EditorGUILayout.FloatField("Bottom Lattitude:", tile.bottomLat);
                    }
                }
            }
            else
            {
                GUILayout.Label("No Tiles Created; Please Add Some");
            }
            EditorGUI.indentLevel--;

            if (GUILayout.Button("Finalize"))
            {
                tileManager.UpdateTileNames();
            }
        }
    }
}