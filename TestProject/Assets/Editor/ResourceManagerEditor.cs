using UnityEngine;
using System.Collections;
using UnityEditor;
using CivGrid;

[CustomEditor(typeof(ResourceManager))]
public class ResourceManagerEditor : Editor
{

    ResourceManager rm;

    void Awake()
    {
        rm = (ResourceManager)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginVertical();

        if (GUILayout.Button("Add New Resource"))
        {
            EditorWindow.GetWindow(typeof(ResourceEditorWindow));
        }

        if (rm.resources != null)
        {
            for (int i = 0; i < rm.resources.Count; i++)
            {
                Resource r = rm.resources[i];
                EditorGUILayout.BeginHorizontal();

                GUILayout.Label(r.resourceName);

                /*
                if (GUILayout.Button("Edit"))
                {
                    EditorWindow.GetWindow(typeof(ResourceEditorWindow));
                }
                 */
                if (GUILayout.Button("Remove"))
                {
                    rm.DeleteResource(r);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndVertical();

        base.DrawDefaultInspector();
    }
}