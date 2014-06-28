using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CivGrid;

namespace CivGrid.Editors
{

    public sealed class CivGridEditor
    {

    }

    public sealed class ResourceEditorWindow : EditorWindow
    {
        public bool editMode;
        public int resourceIndexToEdit;

        //adding fields
        string rName = "None";
        float rRariety;
        Mesh rMesh;
        Texture2D rTexture;
        ResourceManager resourceManager;
        TileManager tileManager;

        List<int> rPossibleTiles = new List<int>();
        List<Feature> rPossibleFeatures = new List<Feature>();

        [MenuItem("CivGrid/New Resource", priority = 3)]
        static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(ResourceEditorWindow));
        }

        void OnEnable()
        {
            resourceManager = GameObject.FindObjectOfType<ResourceManager>();
            tileManager = GameObject.FindObjectOfType<TileManager>();
        }


        Vector2 scrollPosition = new Vector2();
        bool doneAddingResources = false;
        void OnGUI()
        {
            if (editMode == false)
            {
                this.title = "Add Resource";

                GUILayout.BeginVertical();

                scrollPosition = GUILayout.BeginScrollView(scrollPosition);

                GUILayout.Label("Add Resource:", EditorStyles.boldLabel);
                rName = EditorGUILayout.TextField("Resource Name", rName);
                rRariety = EditorGUILayout.FloatField("Rariety", rRariety);
                rMesh = (Mesh)EditorGUILayout.ObjectField("Resource Mesh:", (Object)rMesh, typeof(Mesh), false);
                rTexture = (Texture2D)EditorGUILayout.ObjectField("Resource Mesh Texture:", (Object)rTexture, typeof(Texture2D), false);
                GUILayout.Label("Possible Tiles", EditorStyles.boldLabel);

                if (GUILayout.Button("+"))
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
                    CreateResource(rName, rRariety, rMesh, rTexture);
                    this.Close();
                }

                GUILayout.EndVertical();
            }
            else
            {
                this.title = "Edit Resource";

                Resource r = resourceManager.resources[resourceIndexToEdit];
                GUILayout.Label("Edit Resource: " + r.name);

                GUILayout.BeginVertical();

                scrollPosition = GUILayout.BeginScrollView(scrollPosition);

                r.name = EditorGUILayout.TextField("Resource Name", r.name);
                r.rarity = EditorGUILayout.FloatField("Rariety", r.rarity);
                r.meshToSpawn = (Mesh)EditorGUILayout.ObjectField("Resource Mesh:", (Object)r.meshToSpawn, typeof(Mesh), false);
                r.resourceMeshTexture = (Texture2D)EditorGUILayout.ObjectField("Resource Mesh Texture:", (Object)r.resourceMeshTexture, typeof(Texture2D), false);
                GUILayout.Label("Possible Tiles", EditorStyles.boldLabel);

                if (doneAddingResources == false)
                {
                    foreach (int t in resourceManager.resources[resourceIndexToEdit].rule.possibleTiles)
                    {
                        rPossibleTiles.Add(t);
                    }

                    foreach (Feature f in resourceManager.resources[resourceIndexToEdit].rule.possibleFeatures)
                    {
                        rPossibleFeatures.Add(f);
                    }
                    doneAddingResources = true;
                }

                if (GUILayout.Button("+"))
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



                if (GUILayout.Button("Close"))
                {
                    EditResource(r.name, r.rarity, r.meshToSpawn, r.resourceMeshTexture, resourceIndexToEdit);
                    resourceManager.resources.RemoveAt(resourceIndexToEdit+1);
                    this.Close();
                }

                GUILayout.EndVertical();
            }
        }

        void CreateResource(string name, float rariety, Mesh mesh, Texture2D texture)
        {
            int[] finalTiles = new int[rPossibleTiles.Count];
            List<Feature> finalFeatures = new List<Feature>();

            finalTiles = rPossibleTiles.ToArray();

            //fill feature list with data from rule.possibleFeatures
            for(int i = 0; i < rPossibleFeatures.Count; i++)
            {
                finalFeatures.Add(rPossibleFeatures[i]);
            }


            //nullify duplicate tiles
            for (int i = 0; i < rPossibleTiles.Count; i++)
            {
                for (int z = 0; z < rPossibleTiles.Count; z++)
                {
                    if (rPossibleTiles[i] == rPossibleTiles[z] && i != z)
                    {
                        rPossibleTiles.RemoveAt(z);
                    }
                }
            }
            finalTiles = rPossibleTiles.ToArray();

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

            resourceManager.AddResource(new Resource(name, rariety, mesh, texture, new ResourceRule(finalTiles, finalFeatures.ToArray())));
        }

        void EditResource(string name, float rariety, Mesh mesh, Texture2D texture, int index)
        {
            int[] finalTiles = new int[rPossibleTiles.Count];
            List<Feature> finalFeatures = new List<Feature>();

            finalTiles = rPossibleTiles.ToArray();

            //fill feature list with data from rule.possibleFeatures
            for (int i = 0; i < rPossibleFeatures.Count; i++)
            {
                finalFeatures.Add(rPossibleFeatures[i]);
            }


            //nullify duplicate tiles
            for (int i = 0; i < rPossibleTiles.Count; i++)
            {
                for (int z = 0; z < rPossibleTiles.Count; z++)
                {
                    if (rPossibleTiles[i] == rPossibleTiles[z] && i != z)
                    {
                        rPossibleTiles.RemoveAt(z);
                    }
                }
            }
            finalTiles = rPossibleTiles.ToArray();

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

            resourceManager.AddResourceAt(new Resource(name, rariety, mesh, texture, new ResourceRule(finalTiles, finalFeatures.ToArray())), index);
        }
    }

    public sealed class ImprovementEditorWindow : EditorWindow
    {
        public bool editMode;
        public int improvementIndexToEdit;

        //adding fields
        string iName = "None";
        float iRariety;
        Mesh iMesh;
        Texture2D iTexture;
        ImprovementManager improvementManager;
        TileManager tileManager;

        List<int> iPossibleTiles = new List<int>();
        List<Feature> iPossibleFeatures = new List<Feature>();

        [MenuItem("CivGrid/New Improvement", priority = 4)]
        static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(ImprovementEditorWindow));
        }

        void OnEnable()
        {
            improvementManager = GameObject.FindObjectOfType<ImprovementManager>();
            tileManager = GameObject.FindObjectOfType<TileManager>();
        }


        Vector2 scrollPosition = new Vector2();
        bool doneAddingImprovements = false;
        void OnGUI()
        {
            if (editMode == false)
            {
                this.title = "Add Improvement";
                GUILayout.BeginVertical();

                scrollPosition = GUILayout.BeginScrollView(scrollPosition);

                GUILayout.Label("Add Improvement:", EditorStyles.boldLabel);
                iName = EditorGUILayout.TextField("Improvement Name", iName);
                iRariety = EditorGUILayout.FloatField("Rariety", iRariety);
                iMesh = (Mesh)EditorGUILayout.ObjectField("Improvement Mesh:", (Object)iMesh, typeof(Mesh), false);
                iTexture = (Texture2D)EditorGUILayout.ObjectField("Improvement Mesh Texture:", (Object)iTexture, typeof(Texture2D), false);
                GUILayout.Label("Possible Tiles", EditorStyles.boldLabel);

                if (GUILayout.Button("+"))
                {
                    iPossibleTiles.Add(0);
                }

                for (int i = 0; i < iPossibleTiles.Count; i++)
                {
                    GUILayout.BeginHorizontal();

                    iPossibleTiles[i] = (int)EditorGUILayout.Popup(iPossibleTiles[i], tileManager.tileNames);

                    if (GUILayout.Button("-"))
                    {
                        iPossibleTiles.RemoveAt(i);
                    }

                    GUILayout.EndHorizontal();
                }

                GUILayout.Label("Possible Features", EditorStyles.boldLabel);

                if (GUILayout.Button("+"))
                {
                    iPossibleFeatures.Add(Feature.Flat);
                }

                for (int i = 0; i < iPossibleFeatures.Count; i++)
                {
                    GUILayout.BeginHorizontal();

                    iPossibleFeatures[i] = (Feature)EditorGUILayout.EnumPopup(iPossibleFeatures[i]);

                    if (GUILayout.Button("-"))
                    {
                        iPossibleFeatures.RemoveAt(i);
                    }

                    GUILayout.EndHorizontal();
                }

                GUILayout.EndScrollView();



                if (GUILayout.Button("Create"))
                {
                    CreateImprovement(iName, iRariety, iMesh, iTexture);
                    this.Close();
                }

                GUILayout.EndVertical();
            }
            else
            {
                this.title = "Edit Improvement";

                Improvement improvement = improvementManager.searalizableImprovements[improvementIndexToEdit];
                GUILayout.Label("Edit Improvement: " + improvement.name);

                GUILayout.BeginVertical();

                scrollPosition = GUILayout.BeginScrollView(scrollPosition);

                improvement.name = EditorGUILayout.TextField("Resource Name", improvement.name);
                improvement.rarity = EditorGUILayout.FloatField("Rariety", improvement.rarity);
                improvement.meshToSpawn = (Mesh)EditorGUILayout.ObjectField("Resource Mesh:", (Object)improvement.meshToSpawn, typeof(Mesh), false);
                improvement.improvementMeshTexture = (Texture2D)EditorGUILayout.ObjectField("Resource Mesh Texture:", (Object)improvement.improvementMeshTexture, typeof(Texture2D), false);
                GUILayout.Label("Possible Tiles", EditorStyles.boldLabel);

                if (doneAddingImprovements == false)
                {
                    foreach (int t in improvementManager.searalizableImprovements[improvementIndexToEdit].rule.possibleTiles)
                    {
                        iPossibleTiles.Add(t);
                    }

                    foreach (Feature f in improvementManager.searalizableImprovements[improvementIndexToEdit].rule.possibleFeatures)
                    {
                        iPossibleFeatures.Add(f);
                    }
                    doneAddingImprovements = true;
                }

                if (GUILayout.Button("+"))
                {
                    iPossibleTiles.Add(0);
                }

                for (int i = 0; i < iPossibleTiles.Count; i++)
                {
                    GUILayout.BeginHorizontal();

                    iPossibleTiles[i] = (int)EditorGUILayout.Popup(iPossibleTiles[i], tileManager.tileNames);

                    if (GUILayout.Button("-"))
                    {
                        iPossibleTiles.RemoveAt(i);
                    }

                    GUILayout.EndHorizontal();
                }

                GUILayout.Label("Possible Features", EditorStyles.boldLabel);

                if (GUILayout.Button("+"))
                {
                    iPossibleFeatures.Add(Feature.Flat);
                }

                for (int i = 0; i < iPossibleFeatures.Count; i++)
                {
                    GUILayout.BeginHorizontal();

                    iPossibleFeatures[i] = (Feature)EditorGUILayout.EnumPopup(iPossibleFeatures[i]);

                    if (GUILayout.Button("-"))
                    {
                        iPossibleFeatures.RemoveAt(i);
                    }

                    GUILayout.EndHorizontal();
                }

                GUILayout.EndScrollView();



                if (GUILayout.Button("Close"))
                {
                    EditImprovement(improvement.name, improvement.rarity, improvement.meshToSpawn, improvement.improvementMeshTexture, improvementIndexToEdit);
                    improvementManager.searalizableImprovements.RemoveAt(improvementIndexToEdit+1);
                    this.Close();
                }

                GUILayout.EndVertical();
            }
        }

        void CreateImprovement(string name, float rariety, Mesh mesh, Texture2D texture)
        {
            int[] finalTiles = new int[iPossibleTiles.Count];
            List<Feature> finalFeatures = new List<Feature>();

            finalTiles = iPossibleTiles.ToArray();

            //fill feature list with data from rule.possibleFeatures
            for(int i = 0; i < iPossibleFeatures.Count; i++)
            {
                finalFeatures.Add(iPossibleFeatures[i]);
            }


            //nullify duplicate tiles
            for (int i = 0; i < iPossibleTiles.Count; i++)
            {
                for (int z = 0; z < iPossibleTiles.Count; z++)
                {
                    if (iPossibleTiles[i] == iPossibleTiles[z] && i != z)
                    {
                        iPossibleTiles.RemoveAt(z);
                    }
                }
            }
            finalTiles = iPossibleTiles.ToArray();

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

            improvementManager.AddImprovement(new Improvement(name, rariety, mesh, texture, new ImprovementRule(finalTiles, finalFeatures.ToArray())));
        }

        void EditImprovement(string name, float rariety, Mesh mesh, Texture2D texture, int index)
        {
            int[] finalTiles = new int[iPossibleTiles.Count];
            List<Feature> finalFeatures = new List<Feature>();

            finalTiles = iPossibleTiles.ToArray();

            //fill feature list with data from rule.possibleFeatures
            for (int i = 0; i < iPossibleFeatures.Count; i++)
            {
                finalFeatures.Add(iPossibleFeatures[i]);
            }


            //nullify duplicate tiles
            for (int i = 0; i < iPossibleTiles.Count; i++)
            {
                for (int z = 0; z < iPossibleTiles.Count; z++)
                {
                    if (iPossibleTiles[i] == iPossibleTiles[z] && i != z)
                    {
                        iPossibleTiles.RemoveAt(z);
                    }
                }
            }
            finalTiles = iPossibleTiles.ToArray();

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

            improvementManager.AddImprovementAtIndex(new Improvement(name, rariety, mesh, texture, new ImprovementRule(finalTiles, finalFeatures.ToArray())), index);
        }
    }

    public sealed class TileEditorWindow : EditorWindow
    {
        public bool editMode;
        public int tileIndexToEdit;

        public string tName;
        public bool tIsWater;
        public float tTopLat;
        public float tBottomLat;

        TileManager tileManager;

        [MenuItem("CivGrid/New Tile", priority = 2)]
        static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(TileEditorWindow));
        }

        public void OnEnable()
        {
            tileManager = GameObject.FindObjectOfType<TileManager>();
        }

        Vector2 scrollPosition = new Vector2();
        void OnGUI()
        {
            if (editMode == false)
            {
                this.title = "Add Tile";

                GUILayout.BeginVertical();

                scrollPosition = GUILayout.BeginScrollView(scrollPosition);

                GUILayout.Label("Add Tile:", EditorStyles.boldLabel);

                tName = EditorGUILayout.TextField("Name:", tName);
                tIsWater = EditorGUILayout.Toggle("Is Water:", tIsWater);
                tTopLat = EditorGUILayout.FloatField("Top Lattitude:", tTopLat);
                tBottomLat = EditorGUILayout.FloatField("Bottom Lattitude:", tBottomLat);

                GUILayout.EndScrollView();

                GUILayout.EndVertical();

                if (GUILayout.Button("Create"))
                {
                    CreateTile(tName, tIsWater, tTopLat, tBottomLat);
                    this.Close();
                }
            }
            else
            {
                this.title = "Edit Tile";

                GUILayout.BeginVertical();

                scrollPosition = GUILayout.BeginScrollView(scrollPosition);

                GUILayout.Label("Edit Tile:", EditorStyles.boldLabel);

                Tile tile = tileManager.tiles[tileIndexToEdit];

                tile.name = EditorGUILayout.TextField("Name:", tile.name);
                tile.isWater = EditorGUILayout.Toggle("Is Water:", tile.isWater);
                tile.topLat = EditorGUILayout.FloatField("Top Lattitude:", tile.topLat);
                tile.bottomLat = EditorGUILayout.FloatField("Bottom Lattitude:", tile.bottomLat);

                GUILayout.EndScrollView();

                GUILayout.EndVertical();

                if (GUILayout.Button("Close"))
                {
                    this.Close();
                }
            }
        }

        private void CreateTile(string name, bool isWater, float topLat, float bottomLat)
        {
            tileManager.AddTile(new Tile(name, bottomLat, topLat, isWater));
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

        [MenuItem("CivGrid/Terrain Manager", priority = 1)]
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
    