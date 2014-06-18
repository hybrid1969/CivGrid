using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CivGrid;

namespace CivGrid
{

    public sealed class CivGridEditor
    {

    }

    public class ResourceEditorWindow : EditorWindow
    {
        string rName = "None";
        float rRariety;
        Mesh rMesh;
        ResourceManager resourceManager;
        TileManager tileManager;

        List<int> rPossibleTiles = new List<int>();
        List<Feature> rPossibleFeatures = new List<Feature>();

        [MenuItem("CivGrid/New Resource")]
        static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(ResourceEditorWindow)).title= "Add Resource";
        }

        void OnEnable()
        {
            resourceManager = GameObject.FindObjectOfType<ResourceManager>();
            tileManager = GameObject.FindObjectOfType<TileManager>();
        }


        Vector2 scrollPosition = new Vector2();
        void OnGUI()
        {
            GUILayout.BeginVertical();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Add Resources", EditorStyles.boldLabel);
            rName = EditorGUILayout.TextField("Resource Name", rName);
            rRariety = EditorGUILayout.FloatField("Rariety", rRariety);
            rMesh = (Mesh)EditorGUILayout.ObjectField("Resource Mesh:", (Object)rMesh, typeof(Mesh), false);
            GUILayout.Label("Possible Tiles", EditorStyles.boldLabel);

            if(GUILayout.Button("+"))
            {
                rPossibleTiles.Add(0);
            }

            for (int i = 0; i < rPossibleTiles.Count; i++)
            {
                GUILayout.BeginHorizontal();

                rPossibleTiles[i] = (int)EditorGUILayout.Popup(rPossibleTiles[i], tileManager.tileNames);

                if (GUILayout.Button("-"))
                {
                    rPossibleTiles.RemoveAt(i);
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.Label("Possible Features", EditorStyles.boldLabel);

            if (GUILayout.Button("+"))
            {
                rPossibleFeatures.Add(Feature.Flat);
            }

            for (int i = 0; i < rPossibleFeatures.Count; i++)
            {
                GUILayout.BeginHorizontal();

                rPossibleFeatures[i] = (Feature)EditorGUILayout.EnumPopup(rPossibleFeatures[i]);

                if (GUILayout.Button("-"))
                {
                    rPossibleFeatures.RemoveAt(i);
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

              

            if (GUILayout.Button("Create"))
            {
                CreateResource(rName, rRariety, rMesh);
                this.Close();
            }

            GUILayout.EndVertical();
        }

        void CreateResource(string name, float rariety, Mesh mesh)
        {
            List<Tile> finalTiles = new List<Tile>();
            List<Feature> finalFeatures = new List<Feature>();

            //fill tile list with data from rule.possibleTiles
            for(int i = 0; i < rPossibleTiles.Count; i++)
            {
                finalTiles.Add(tileManager.tiles[rPossibleTiles[i]]);
            }

            //fill feature list with data from rule.possibleFeatures
            for(int i = 0; i < rPossibleFeatures.Count; i++)
            {
                finalFeatures.Add(rPossibleFeatures[i]);
            }


            //nullify duplicate tiles
            for (int i = 0; i < finalTiles.Count; i++)
            {
                for (int z = 0; z < finalTiles.Count; z++)
                {
                    if (finalTiles[i] == finalTiles[z] && i != z)
                    {
                        finalTiles.RemoveAt(z);
                    }
                }
            }

            //nullify duplicate features
            for (int i = 0; i < finalFeatures.Count; i++)
            {
                for (int z = 0; z < finalFeatures.Count; z++)
                {
                    if (finalFeatures[i] == finalFeatures[z] && i != z)
                    {
                        finalFeatures.RemoveAt(z);
                    }
                }
            }

            resourceManager.AddResource(new Resource(name, rariety, mesh, new ResourceRules(finalTiles.ToArray(), finalFeatures.ToArray())));
        }
    }

    public enum TypeofEditorTile { Terrain, Resource, Improvement }

    public sealed class TerrainManagerWindow : EditorWindow
    {
        WorldManager worldManager;
        ImprovementManager improvementManager;
        ResourceManager resourceManager;
        TileManager tileManager;

        string loc = Application.dataPath;
        string editedLoc;

        Vector2 terrainAtlasSize = new Vector2(1,1);
        Texture2D[,] textures;
        int[,] tempTileType;
        int[,] tempResourceType;
        int[,] tempImprovementType;
        TypeofEditorTile[,] catagory;
        List<TileItem> tileLocations = new List<TileItem>();
        List<ResourceItem> resourceLocations = new List<ResourceItem>();
        List<ImprovementItem> improvementLocations = new List<ImprovementItem>();

        private Vector2 internalAtlasDimension;

        [MenuItem("CivGrid/Terrain Manager")]
        static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(TerrainManagerWindow));
        }

        void OnEnable()
        {
            this.title = "Terrain Manager";
            worldManager = GameObject.FindObjectOfType<WorldManager>();
            improvementManager = GameObject.FindObjectOfType<ImprovementManager>();
            resourceManager = GameObject.FindObjectOfType<ResourceManager>();
            tileManager = GameObject.FindObjectOfType<TileManager>();

            improvementManager.UpdateImprovementNames();

            if (worldManager == null || improvementManager == null)
            {
                Debug.LogError("Need to have WorldManager and ImprovementManager in current scene");
            }

            tileManager.UpdateTileNames();
            resourceManager.UpdateResourceNames();
            improvementManager.UpdateImprovementNames();
        }

        Vector2 scrollPosition = new Vector2();
        void OnGUI()
        {
            terrainAtlasSize = EditorGUILayout.Vector2Field("Terrain Texture Size", terrainAtlasSize);

            if (GUILayout.Button("Generate Grid"))
            {
                AssignAtlasSize();
            }

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            for (int x = 0; x < internalAtlasDimension.x; x++)
            {
                //start new row
                GUILayout.BeginHorizontal();
                for (int y = 0; y < internalAtlasDimension.y; y++)
                {
                    textures[x, y] = (Texture2D)EditorGUILayout.ObjectField((Object)textures[x, y], typeof(Texture2D), false, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true), GUILayout.MaxHeight(150f), GUILayout.MaxWidth(150f));
                    GUILayout.BeginVertical();
                    GUILayout.Label("Settings for texture (" + x + "," + y + "):");
                    catagory[x, y] = (TypeofEditorTile)EditorGUILayout.EnumPopup("Type:", catagory[x, y], GUILayout.ExpandWidth(true), GUILayout.MaxWidth(300f));
                    if (catagory[x, y] == TypeofEditorTile.Terrain)
                    {
                        tempTileType[x, y] = (int)EditorGUILayout.Popup("Tile Type:", tempTileType[x, y], tileManager.tileNames, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(300f));
                        tempResourceType[x,y] = 0;
                        tempImprovementType[x,y] = 0;
                    }
                    else if (catagory[x, y] == TypeofEditorTile.Resource)
                    {
                        tempResourceType[x, y] = (int)EditorGUILayout.Popup("Resource Type:", tempResourceType[x, y], resourceManager.resourceNames, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(300f));
                        tempTileType[x, y] = 0;
                        tempImprovementType[x,y] = 0;
                    }
                    else if (catagory[x, y] == TypeofEditorTile.Improvement)
                    {
                        tempImprovementType[x, y] = (int)EditorGUILayout.Popup("Improvement Type:", tempImprovementType[x, y], improvementManager.improvementNames, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(300f));
                        tempTileType[x,y] = 0;
                        tempResourceType[x, y] = 0;
                    }
                    else
                    {
                        Debug.LogError("Something went wrong; send dump file!");
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();

            editedLoc = ((string)loc.Clone()).Remove(0, loc.IndexOf("Assets"));

            GUILayout.Label("Location to save:");
            GUILayout.Label(editedLoc);

            if (GUILayout.Button("Edit"))
            {
                string tempLoc = EditorUtility.OpenFolderPanel("Folder to save texture...", loc, "atlas.png"); ;
                if (tempLoc != null && tempLoc != "")
                {
                    loc = tempLoc;
                }
            }

            if (GUILayout.Button("Generate Atlas"))
            {
                GenerateAtlas();
            }
        }

        private void AssignAtlasSize()
        {
            internalAtlasDimension = terrainAtlasSize;
            textures = new Texture2D[(int)internalAtlasDimension.x, (int)internalAtlasDimension.y];
            tempTileType = new int[(int)internalAtlasDimension.x, (int)internalAtlasDimension.y];
            tempResourceType = new int[(int)internalAtlasDimension.x, (int)internalAtlasDimension.y];
            tempImprovementType = new int[(int)internalAtlasDimension.x, (int)internalAtlasDimension.y];
            catagory = new TypeofEditorTile[(int)internalAtlasDimension.x, (int)internalAtlasDimension.y];

            improvementLocations.Clear();
            resourceLocations.Clear();
            tileLocations.Clear();
        }

        private void GenerateAtlas()
        {

            Rect[] rectAreas;
            Texture2D returnTexture = TexturePacker.AtlasTextures(CivGridUtility.ToSingleArray(textures), out rectAreas);

            int lengthOfArraysX = catagory.GetLength(0); 
            int lengthOfArraysY = catagory.GetLength(1); 

            for (int x = 0; x < lengthOfArraysX; x++)
            {
                for (int y = 0; y < lengthOfArraysY; y++)
                {
                    if (catagory[x, y] == TypeofEditorTile.Terrain)
                    {
                        if (!tileLocations.ContainsKey(tileManager.tiles[tempTileType[x, y]]))
                        {
                            tileLocations.Add(tileManager.tiles[tempTileType[x, y]], rectAreas[x * lengthOfArraysY + y]);
                        }
                        else
                        {
                            Debug.LogError("Multiple textures for one terrain type, unexpected behaviour possible");
                        }
                    }
                    else if (catagory[x, y] == TypeofEditorTile.Resource)
                    {
                        if (!resourceLocations.ContainsKey(resourceManager.resources[tempResourceType[x, y]]))
                        {
                            resourceLocations.Add(resourceManager.resources[tempResourceType[x, y]], rectAreas[x * lengthOfArraysY + y]);
                        }
                        else
                        {
                            Debug.LogError("Multiple textures for one resource type, unexpected behaviour possible");
                        }
                    }
                    else
                    {
                        if (!improvementLocations.ContainsKey(improvementManager.searalizableImprovements[tempImprovementType[x, y]]))
                        {
                            improvementLocations.Add(improvementManager.searalizableImprovements[tempImprovementType[x, y]], rectAreas[x * lengthOfArraysY + y]);
                        }
                        else
                        {
                            Debug.LogError("Multiple textures for one improvement type, unexpected behaviour possible");
                        }
                    }
                }
            }

            Saver.SaveTexture(returnTexture, loc, "TerrainAtlas", false);
            returnTexture.hideFlags = HideFlags.HideAndDontSave;
            worldManager.textureAtlas.terrainAtlas = (Texture2D)AssetDatabase.LoadAssetAtPath(editedLoc + "/TerrainAtlas.png", typeof(Texture2D));
            worldManager.textureAtlas.tileLocations = (TileItem[])tileLocations.ToArray().Clone();
            worldManager.textureAtlas.resourceLocations = (ResourceItem[])resourceLocations.ToArray().Clone();
            worldManager.textureAtlas.improvementLocations = (ImprovementItem[])improvementLocations.ToArray().Clone();
            
            

            this.Close();
        }
    }
}
    