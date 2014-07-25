using UnityEngine;
using System.Collections;
using UnityEditor;
using CivGrid;


namespace CivGrid.Editors
{
    [CustomEditor(typeof(ImprovementManager))]
    public class ImprovementManagerEditor : Editor
    {

        ImprovementManager improvementManager;
        WorldManager worldManager;
        TileManager tileManager;
        bool[] foldoutOpen;
        bool[] extraInfoFoldout;

        void Awake()
        {
            ImprovementManager improvementManager = (ImprovementManager)target;
            tileManager = improvementManager.GetComponent<TileManager>();
            if (improvementManager.improvements != null)
            {
                foldoutOpen = new bool[improvementManager.improvements.Count];
                extraInfoFoldout = new bool[improvementManager.improvements.Count];
            }
        }

        bool done;
        public override void OnInspectorGUI()
        {
            if (done == false)
            {
                Awake();

                done = true;
            }

            if (improvementManager == null) { improvementManager = (ImprovementManager)target; }
            if (worldManager == null) { worldManager = improvementManager.GetComponent<WorldManager>(); }
            if (tileManager == null) { tileManager = improvementManager.GetComponent<TileManager>(); }
            if (improvementManager.improvements != null && (foldoutOpen == null || foldoutOpen.Length != improvementManager.improvements.Count)) { foldoutOpen = new bool[improvementManager.improvements.Count]; }
            if (improvementManager.improvements != null && (extraInfoFoldout == null || extraInfoFoldout.Length != improvementManager.improvements.Count)) { extraInfoFoldout = new bool[improvementManager.improvements.Count]; }

            if (GUILayout.Button("Add New Improvement"))
            {
                ImprovementEditorWindow window = EditorWindow.CreateInstance<ImprovementEditorWindow>();
                EditorWindow.GetWindow<ImprovementEditorWindow>();
                window.editMode = false;
                window.improvementIndexToEdit = 0;
            }

            if (improvementManager.improvements != null && improvementManager.improvements.Count > 0)
            {
                for (int i = 0; i < improvementManager.improvements.Count; i++)
                {
                    Improvement improvement = improvementManager.improvements[i];

                    EditorGUILayout.BeginHorizontal();

                    foldoutOpen[i] = EditorGUILayout.Foldout(foldoutOpen[i], improvement.name);

                    if (GUILayout.Button("Edit"))
                    {
                        ImprovementEditorWindow window = EditorWindow.CreateInstance<ImprovementEditorWindow>();
                        EditorWindow.GetWindow<ImprovementEditorWindow>();
                        window.editMode = true;
                        window.improvementIndexToEdit = i;
                    }

                    if (GUILayout.Button("Remove"))
                    {
                        improvementManager.DeleteImprovement(improvement);
                    }

                    EditorGUILayout.EndHorizontal();

                    EditorGUI.indentLevel++;
                    if (foldoutOpen[i])
                    {
                        improvement.name = EditorGUILayout.TextField("Improvement Name:", improvement.name);
                        improvement.replaceGroundTexture = EditorGUILayout.Toggle("Replace Ground Texture:", improvement.replaceGroundTexture);

                        //if ((worldManager.textureAtlas.improvementLocations.ContainsKey(improvement) == false) && improvement.replaceGroundTexture == true)
                        {
                          //  EditorGUILayout.HelpBox("Please add this improvement to the terrain atlas.", MessageType.Warning);
                        }

                        extraInfoFoldout[i] = EditorGUILayout.Foldout(extraInfoFoldout[i], "Rules:");

                        if (extraInfoFoldout[i])
                        {
                            EditorGUILayout.SelectableLabel("Possible Tiles:", EditorStyles.boldLabel, GUILayout.ExpandHeight(false), GUILayout.MaxHeight(15));
                            foreach (int t in improvement.rule.possibleTiles)
                            {
                                EditorGUI.indentLevel++;
                                EditorGUILayout.SelectableLabel(tileManager.tiles[t].name, GUILayout.ExpandHeight(false), GUILayout.MaxHeight(18));
                                EditorGUI.indentLevel--;
                            }

                            EditorGUILayout.SelectableLabel("Possible Features:", EditorStyles.boldLabel, GUILayout.ExpandHeight(false), GUILayout.MaxHeight(15));
                            foreach (Feature f in improvement.rule.possibleFeatures)
                            {
                                EditorGUI.indentLevel++;
                                EditorGUILayout.SelectableLabel(f.ToString(), GUILayout.ExpandHeight(false), GUILayout.MaxHeight(18));
                                EditorGUI.indentLevel--;
                            }
                        }
                        improvement.meshToSpawn = (Mesh)EditorGUILayout.ObjectField("Improvement Mesh", (Object)improvement.meshToSpawn, typeof(Mesh), false);
                        //if (improvement.meshToSpawn.isReadable == false) { EditorGUILayout.HelpBox("Please enable Read/Write on this mesh in its import settings", MessageType.Error); }
                        improvement.meshTexture = (Texture2D)EditorGUILayout.ObjectField("Improvement Mesh Texture:", (Object)improvement.meshTexture, typeof(Texture2D), false, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(true));
                        //improvement.improvementMeshTexture.SetPixel(0, 0, improvement.improvementMeshTexture.GetPixel(0, 0));
                        //try
                        //{
                        //   improvement.improvementMeshTexture.SetPixel(0, 0, improvement.improvementMeshTexture.GetPixel(0, 0));
                        //}
                        //catch (UnityException e)
                        //{
                        //  Debug.Log(e);
                        //EditorGUILayout.HelpBox("Please enable read/write on this texture in its import settings", MessageType.Error);
                        //}
                    }
                    EditorGUI.indentLevel--;
                }
            }
            else
            {
                GUILayout.Label("No Improvements Created; Please Add Some");
            }

            if (GUILayout.Button("Finalize"))
            {
                improvementManager.UpdateImprovementNames();
            }
        }
    }
}