using UnityEngine;
using System.Collections;
using UnityEditor;

namespace CivGrid.Editors
{
    [CustomEditor(typeof(TileManager))]
    public class TileManagerEditor : Editor
    {
        TileManager tileManager;
        bool[] foldoutOpen = new bool[0];


        public void Awake()
        {
            tileManager = (TileManager)target;
            if (tileManager.tiles != null)
            {
                foldoutOpen = new bool[tileManager.tiles.Count];
            }
        }

        bool done;
        bool longOpen = false;
        bool latOpen = false;
        public override void OnInspectorGUI()
        {
            if (done == false)
            {
                Awake();

                done = true;
            }

            if (tileManager == null) { Awake(); }
            if (tileManager.tiles != null && foldoutOpen.Length != tileManager.tiles.Count) { Awake(); }

            if (GUILayout.Button("Add New Tile"))
            {
                TileEditorWindow window = EditorWindow.CreateInstance<TileEditorWindow>();
                EditorWindow.GetWindow<TileEditorWindow>();
                window.editMode = false;
                window.tileIndexToEdit = 0;
            }

            if (tileManager.tiles != null && tileManager.tiles.Count > 0)
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
                        tile.isShore = EditorGUILayout.Toggle("Is Shore:", tile.isShore);
                        tile.isOcean = EditorGUILayout.Toggle("Is Ocean:", tile.isOcean);
                        tile.isMountain = EditorGUILayout.Toggle("Is Mountain:", tile.isMountain);
                        if (tile.isShore == false && tile.isOcean == false && tile.isMountain == false)
                        {
                            EditorGUI.indentLevel++;
                            longOpen = EditorGUILayout.Foldout(longOpen, "Longitude Clamps:");
                            if (longOpen)
                            {
                                tile.topLongitude = EditorGUILayout.FloatField("Top Longitude:", tile.topLongitude);
                                tile.bottomLongitude = EditorGUILayout.FloatField("Bottom Longitude:", tile.bottomLongitude);
                            }
                            latOpen = EditorGUILayout.Foldout(latOpen, "Latitude Clamps:");
                            if (latOpen)
                            {
                                tile.leftLatitude = EditorGUILayout.FloatField("Left Latitude:", tile.leftLatitude);
                                tile.rightLatitude = EditorGUILayout.FloatField("Right Latitude:", tile.rightLatitude);
                            }
                            EditorGUI.indentLevel--;
                        }
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