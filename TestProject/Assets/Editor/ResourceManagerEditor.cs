using UnityEngine;
using System.Collections;
using UnityEditor;
using CivGrid;


namespace CivGrid.Editors
{
    [CustomEditor(typeof(ResourceManager))]
    public class ResourceManagerEditor : Editor
    {

        ResourceManager resourceManager;
        TileManager tileManager;
        bool[] foldoutOpen;
        bool[] extraInfoFoldout;

        void Awake()
        {
            resourceManager = (ResourceManager)target;
            tileManager = resourceManager.GetComponent<TileManager>();
            foldoutOpen = new bool[resourceManager.resources.Count];
            extraInfoFoldout = new bool[resourceManager.resources.Count];
        }

        public override void OnInspectorGUI()
        {
            if (resourceManager == null) { resourceManager = (ResourceManager)target; }
            if (tileManager == null) { tileManager = resourceManager.GetComponent<TileManager>(); }
            if (foldoutOpen == null || foldoutOpen.Length != resourceManager.resources.Count) { foldoutOpen = new bool[resourceManager.resources.Count]; }
            if (extraInfoFoldout == null || extraInfoFoldout.Length != resourceManager.resources.Count) { extraInfoFoldout = new bool[resourceManager.resources.Count]; }

            if (GUILayout.Button("Add New Resource"))
            {
                ResourceEditorWindow window = EditorWindow.CreateInstance<ResourceEditorWindow>();
                EditorWindow.GetWindow<ResourceEditorWindow>();
                window.editMode = false;
                window.resourceIndexToEdit = 0;
            }

            if (resourceManager.resources != null)
            {
                for (int i = 0; i < resourceManager.resources.Count; i++)
                {
                    Resource r = resourceManager.resources[i];

                    EditorGUILayout.BeginHorizontal();

                    foldoutOpen[i] = EditorGUILayout.Foldout(foldoutOpen[i], r.name);

                    if (GUILayout.Button("Edit"))
                    {
                        ResourceEditorWindow window = EditorWindow.CreateInstance<ResourceEditorWindow>();
                        EditorWindow.GetWindow<ResourceEditorWindow>();
                        window.editMode = true;
                        window.resourceIndexToEdit = i;
                    }

                    if (GUILayout.Button("Remove"))
                    {
                        resourceManager.DeleteResource(r);
                    }

                    EditorGUILayout.EndHorizontal();

                    EditorGUI.indentLevel++;
                    if (foldoutOpen[i])
                    {
                        r.name = EditorGUILayout.TextField("Resource Name:", r.name);
                        r.rarity = EditorGUILayout.FloatField("Rarity:", r.rarity);
                        r.spawnAmount = EditorGUILayout.IntField("Spawn Amount:", r.spawnAmount);

                        extraInfoFoldout[i] = EditorGUILayout.Foldout(extraInfoFoldout[i], "Rules:");

                        if (extraInfoFoldout[i])
                        {
                            EditorGUILayout.SelectableLabel("Possible Tiles", EditorStyles.boldLabel, GUILayout.ExpandHeight(false), GUILayout.MaxHeight(15));
                            foreach (int t in r.rule.possibleTiles)
                            {
                                EditorGUILayout.SelectableLabel(tileManager.TryGet(t).name, GUILayout.ExpandHeight(false), GUILayout.MaxHeight(18));
                            }

                            EditorGUILayout.SelectableLabel("Possible Features", EditorStyles.boldLabel, GUILayout.ExpandHeight(false), GUILayout.MaxHeight(15));
                            foreach (Feature f in r.rule.possibleFeatures)
                            {
                                EditorGUILayout.SelectableLabel(f.ToString(), GUILayout.ExpandHeight(false), GUILayout.MaxHeight(18));
                            }
                        }
                        r.meshToSpawn = (Mesh)EditorGUILayout.ObjectField("Resource Mesh", (Object)r.meshToSpawn, typeof(Mesh), false);
                        r.resourceMeshTexture = (Texture2D)EditorGUILayout.ObjectField("Resource Mesh Texture:", (Object)r.resourceMeshTexture, typeof(Texture2D), false, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(true));
                    }
                    EditorGUI.indentLevel--;
                }
            }
            else
            {
                GUILayout.Label("No Resources Created; Please Add Some");
            }

            if (GUILayout.Button("Finalize"))
            {
                resourceManager.UpdateResourceNames();
            }

            //base.DrawDefaultInspector();
        }
    }
}