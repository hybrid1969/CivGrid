using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CivGrid;

namespace CivGrid
{
    /// <summary>
    /// Enum for the feature on a tile.<br />
    /// <br />
    /// Contains three basic types of features. See remarks for descriptions of each.
    /// </summary>
    /// <remarks>
    /// <list type="definition">
    /// <item>
    /// <term>Flat</term>
    /// <description>A completly flat hexagon with no change in the vertical axis.</description>
    /// </item>
    /// <item>
    /// <term>Hill</term>
    /// <description>A hill with vertical noise.</description>
    /// </item>
    /// <item>
    /// <term>Mountain</term>
    /// <description>A large pointed mountain with vertical noise.</description>
    /// </item>
    /// </list>
    /// </remarks>
    public enum Feature 
    { 
        /// <summary>
        /// A completly flat hexagon with no change in the vertical axis.
        /// </summary>
        Flat = 0, 
        /// <summary>
        /// A hill with vertical noise.
        /// </summary>
        Hill = 1, 
        /// <summary>
        /// A large pointed mountain with vertical noise.
        /// </summary>
        Mountain = 3 
    }

    /// <summary>
    /// Preset world generator values that create numerous world types.<br />
    /// <br />
    /// Contains six basic types of worlds. See remarks for description of each.<br />
    /// </summary>
    /// <remarks>
    /// Description for each world type.
    /// <list type="definition">
    /// <item>
    /// <term>Diced</term>
    /// <description>A very random map with many very small noisy island. No large landmasses, with a high ratio of water.</description>
    /// </item>
    /// <item>
    /// <term>Continents</term>
    /// <description>A world like ours. A few large land masses with numerous smaller islands. Fair amount of both water and land.</description>
    /// </item>
    /// <item>
    /// <term>Pangaea</term>
    /// <description>An extremely large landmass with a few smaller islands offshore. A large amount of land.</description>
    /// </item>
    /// <item>
    /// <term>Strings</term>
    /// <description>Long snakey islands throughout. No large landmasses, with a high ratio of water.</description>
    /// </item>
    /// <item>
    /// <term>Small Islands</term>
    /// <description>Many small islands. Islands are larger and more regular than with Diced. No large landmasses, with a high ratio of water.</description>
    /// </item>
    /// <item>
    /// <term>Large Islands</term>
    /// <description>A fair amount of medium sized landmasses. Medium landmasses, with a somewhat high ratio of water.</description>
    /// </item>
    /// </list>
    /// </remarks>
    public enum WorldType 
    { 
        /// <summary>
        /// A very random map with many very small noisy island. No large landmasses, with a high ratio of water.
        /// </summary>
        Diced, 
        /// <summary>
        /// A world like ours. A few large land masses with numerous smaller islands. Fair amount of both water and land.
        /// </summary>
        Continents, 
        /// <summary>
        /// An extremely large landmass with a few smaller islands offshore. A large amount of land.
        /// </summary>
        Pangaea, 
        /// <summary>
        /// Long snakey islands throughout. No large landmasses, with a high ratio of water.
        /// </summary>
        Strings, 
        /// <summary>
        /// Many small islands. Islands are larger and more regular than with Diced. No large landmasses, with a high ratio of water.
        /// </summary>
        SmallIslands, 
        /// <summary>
        /// A fair amount of medium sized landmasses. Medium landmasses, with a somewhat high ratio of water.
        /// </summary>
        LargeIslands 
    }

    /// <summary>
    /// This script runs the entire CivGrid system. <br />
    /// <br />
    /// Holds all chunks, and in turn each hexagon, in memory and runs all the operations throughout them when needed. Contains the methods to generate worlds, load worlds, and save worlds.
    /// While some generation methods are exposed for use, it is best to not try and use the lower level methods.
    /// </summary>
    [RequireComponent(typeof(TileManager), typeof(ResourceManager), typeof(ImprovementManager))]
    public class WorldManager : MonoBehaviour
    {
        #region fields

        //Moving
        //internal Vector3 currentHex;
        //internal Vector3 goToHex;
        //sinternal int distance;

        /// <summary>
        /// The extents of a hexagon from the origin
        /// </summary>
        public Vector3 hexExt;
        /// <summary>
        /// The size of a hexagon from side to side
        /// </summary>
        public Vector3 hexSize;
        /// <summary>
        /// The center of a hexagon
        /// </summary>
        public Vector3 hexCenter;

        //internals
        private Vector3 moveVector;
        private RaycastHit chunkHit;
        private GameObject selectedHex;
        /// <summary>
        /// The position of the mouse in screen coordinates
        /// </summary>
        public Vector2 mousePos;
        private GameObject chunkHolder;
        /// <summary>
        /// The chunks in the generated world, <see cref="HexChunk"/>
        /// </summary>
        public HexChunk[,] hexChunks;
        private int xSectors;
        private int zSectors;
        /// <summary>
        /// A cached flat hexagon mesh
        /// </summary>
        public Mesh flatHexagonSharedMesh;
        private bool doneGenerating;
        internal CivGridCamera civGridCamera;
        //public Color borderColor;

        //World Values
        private Texture2D tileMap;
        /// <summary>
        /// Scale of the noise map, and in turn the world
        /// </summary>
        public float noiseScale;
        /// <summary>
        /// The world texture atlas
        /// </summary>
        [SerializeField]
        public TextureAtlas textureAtlas;
        /// <summary>
        /// The type of the world, if one selected
        /// </summary>
        public WorldType worldType;
        /// <summary>
        /// The size of the map in hexagons
        /// </summary>
        public Vector2 mapSize;
        /// <summary>
        /// The number of hexagons in one chunk, in one axis
        /// </summary>
        /// <remarks>
        /// The real amount of hexagons in the chunk is represented as: <b>(chunkSize)^2</b>
        /// </remarks>
        public int chunkSize;
        /// <summary>
        /// The radius of the hexagon
        /// </summary>
        public float hexRadiusSize;

        //world setup
        /// <summary>
        /// Whether or not to use the built in <see cref="CivGridCamera"/> 
        /// </summary>
        public bool useCivGridCamera;
        /// <summary>
        /// Whether or not to generate the world on start up
        /// </summary>
        public bool generateOnStart;
        internal bool generateNewValues;
        /// <summary>
        /// Whether or not to use the built in world type values or custom user ones
        /// </summary>
        public bool useWorldTypeValues;
        //public bool showBorder;

        //Hill and mountains
        /// <summary>
        /// The base heightmap for mountains
        /// </summary>
        [SerializeField]
        public Texture2D mountainMap;
        /// <summary>
        /// Amount to scale the mountain heightmap upon
        /// </summary>
        public float mountainScaleY;


        //managers
        internal ResourceManager resourceManager;
        internal ImprovementManager improvementManager;
        internal TileManager tileManager;

        //delegates
        public delegate void OnHexClick(HexInfo hex, int mouseButton);
        public static OnHexClick onHexClick;

        public delegate void OnMouseOverHex(HexInfo hex);
        public static OnMouseOverHex onMouseOverHex;
        #endregion

        /// <summary>
        /// Sets up values for world generation.
        /// </summary>
        void Awake()
        {
            resourceManager = GetComponent<ResourceManager>();
            improvementManager = GetComponent<ImprovementManager>();
            tileManager = GetComponent<TileManager>();
            civGridCamera = GameObject.FindObjectOfType<CivGridCamera>();
            if (generateOnStart == true)
            {
                //LoadAndGenerateMap("terrainTest");
                GenerateNewMap(true);
                CivGridFileUtility.SaveTerrain("terrainTest");
            }
            else { civGridCamera.enabled = false; }
        }

        /// <summary>
        /// Starts world generation.
        /// </summary>
        /// <param name="assignTypes">If it should assign values to hexagons</param>
        /// <remarks>
        /// For generating a new map, and not loading values, set the parameter to true.
        /// </remarks>
        public void GenerateNewMap(bool assignTypes)
        {
            this.generateNewValues = assignTypes;
            StartGeneration(true);
        }

        /// <summary>
        /// Starts world generation.
        /// </summary>
        public void GenerateNewMap()
        {
            this.generateNewValues = true;
            StartGeneration(true);
        }

        /// <summary>
        /// Handles destruction of world dependencies and generates a brand new world.
        /// </summary>
        public void RegenerateNewMap()
        {
            Destroy(GameObject.Find("ChunkHolder"));
            if (useCivGridCamera)
            {
                civGridCamera.enabled = false;
                Destroy(GameObject.Find("Cam2"));
            }
            StartGeneration(false);
        }

        /// <summary>
        /// Loads a map from a file name.
        /// </summary>
        /// <param name="name">Name of the saved map</param>
        /// <remarks>
        /// The file name should not be a complete file path, only the name given to the saved map.
        /// </remarks>
        public void LoadAndGenerateMap(string name)
        {
            generateNewValues = false;
            string savedMapLocation = Application.dataPath + "/../" +  name;
            CivGridFileUtility.LoadTerrain(savedMapLocation);
            resourceManager.InitiateResourceTexturesOnHexs();
        }

        /// <summary>
        /// Saves a map under the given name.
        /// </summary>
        /// <param name="name">Name of the save</param>
        public void SaveMap(string name)
        {
            CivGridFileUtility.SaveTerrain(name);
        }

        /// <summary>
        /// Disbatches generation work.
        /// </summary>
        /// <param name="setUpManagers">If the manager need setup</param>
        private void StartGeneration(bool setUpManagers)
        {
            if (setUpManagers)
            {
                resourceManager.SetUp();
                improvementManager.SetUp();
                tileManager.SetUp();
            }


            DetermineWorldType();
            GetHexProperties();
            GenerateMap();

            if (useCivGridCamera)
            {
                civGridCamera.enabled = true;
                if (civGridCamera == null) { Debug.LogError("Please add the 'CivGridCamera' to the mainCamera"); }
                else { civGridCamera.SetupCameras(); }
            }

            if (generateNewValues == true)
            {
                resourceManager.InitiateResourceTexturesOnHexs();
            }

            doneGenerating = true;
        }

        /// <summary>
        /// Scales noise to be consistant between world sizes.
        /// </summary>
        private void SetNoiseScaleToTrueValue()
        {
            noiseScale /= mapSize.magnitude;
        }

        /// <summary>
        /// Sets the tileMap to the correct mapping settings.
        /// </summary>
        private void DetermineWorldType()
        {
            int smoothingCutoff = 0;
            if (useWorldTypeValues)
            {
                if (worldType == WorldType.Continents) { noiseScale = 5f; smoothingCutoff = 3; }
                else if (worldType == WorldType.Pangaea) { noiseScale = 3f; smoothingCutoff = 2; }
                else if (worldType == WorldType.Strings) { noiseScale = 25f; smoothingCutoff = 3; }
                else if (worldType == WorldType.Diced) { noiseScale = 25f; smoothingCutoff = 7; }
                else if (worldType == WorldType.LargeIslands) { noiseScale = 25f; smoothingCutoff = 5; }
                else if (worldType == WorldType.SmallIslands) { noiseScale = 25f; smoothingCutoff = 6; }
            }

            SetNoiseScaleToTrueValue();
            if (noiseScale == 0) { Debug.LogException(new UnityException("Noise scale is zero, this produces artifacts.")); }
            noiseScale = UnityEngine.Random.Range(noiseScale / 1.05f, noiseScale * 1.05f);

            tileMap = NoiseGenerator.SmoothPerlinNoise((int)mapSize.x, (int)mapSize.y, noiseScale, smoothingCutoff);
        }

        /// <summary>
        /// Generates and caches a flat hexagon mesh for all the hexagon's to pull down into their localMesh, if they are flat.
        /// </summary>
        private void GetHexProperties()
        {
            //Creates mesh to calculate bounds
            GameObject inst = new GameObject("Bounds Set Up: Flat");
            //add mesh filter to our temp object
            inst.AddComponent<MeshFilter>();
            //add a renderer to our temp object
            inst.AddComponent<MeshRenderer>();
            //add a mesh collider to our temp object; this is for determining dimensions cheaply and easily
            inst.AddComponent<MeshCollider>();
            //reset the position to global zero
            inst.transform.position = Vector3.zero;
            //reset all rotation
            inst.transform.rotation = Quaternion.identity;


            Vector3[] vertices;
            int[] triangles;
            Vector2[] uv;

            #region verts

            float floorLevel = 0;
            //positions vertices of the hexagon to make a normal hexagon
            vertices = new Vector3[]
            {
                /*0*/new Vector3((hexRadiusSize * Mathf.Cos((float)(2*Mathf.PI*(3+0.5)/6))), floorLevel, (hexRadiusSize * Mathf.Sin((float)(2*Mathf.PI*(3+0.5)/6)))),
                /*1*/new Vector3((hexRadiusSize * Mathf.Cos((float)(2*Mathf.PI*(2+0.5)/6))), floorLevel, (hexRadiusSize * Mathf.Sin((float)(2*Mathf.PI*(2+0.5)/6)))),
                /*2*/new Vector3((hexRadiusSize * Mathf.Cos((float)(2*Mathf.PI*(1+0.5)/6))), floorLevel, (hexRadiusSize * Mathf.Sin((float)(2*Mathf.PI*(1+0.5)/6)))),
                /*3*/new Vector3((hexRadiusSize * Mathf.Cos((float)(2*Mathf.PI*(0+0.5)/6))), floorLevel, (hexRadiusSize * Mathf.Sin((float)(2*Mathf.PI*(0+0.5)/6)))),
                /*4*/new Vector3((hexRadiusSize * Mathf.Cos((float)(2*Mathf.PI*(5+0.5)/6))), floorLevel, (hexRadiusSize * Mathf.Sin((float)(2*Mathf.PI*(5+0.5)/6)))),
                /*5*/new Vector3((hexRadiusSize * Mathf.Cos((float)(2*Mathf.PI*(4+0.5)/6))), floorLevel, (hexRadiusSize * Mathf.Sin((float)(2*Mathf.PI*(4+0.5)/6))))
            };

            #endregion

            #region triangles

            //triangles connecting the verts
            triangles = new int[] 
            {
                1,5,0,
                1,4,5,
                1,2,4,
                2,3,4
            };

            #endregion

            #region uv
            //uv mappping
            uv = new Vector2[]
            {
                new Vector2(0.05f,0.25f),
                new Vector2(0.05f,0.75f),
                new Vector2(0.5f,0.95f),
                new Vector2(0.95f,0.75f),
                new Vector2(0.95f,0.25f),
                new Vector2(0.5f,0.05f),
            };
            #endregion

            #region finalize
            //create new mesh to hold the data for the flat hexagon
            flatHexagonSharedMesh = new Mesh();
            //assign verts
            flatHexagonSharedMesh.vertices = vertices;
            //assign triangles
            flatHexagonSharedMesh.triangles = triangles;
            //assign uv
            flatHexagonSharedMesh.uv = uv;
            //set temp gameObject's mesh to the flat hexagon mesh
            inst.GetComponent<MeshFilter>().mesh = flatHexagonSharedMesh;
            //make object play nicely with lighting
            inst.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            //set mesh collider's mesh to the flat hexagon
            inst.GetComponent<MeshCollider>().sharedMesh = flatHexagonSharedMesh;
            #endregion

            //calculate the extents of the flat hexagon
            hexExt = new Vector3(inst.gameObject.collider.bounds.extents.x, inst.gameObject.collider.bounds.extents.y, inst.gameObject.collider.bounds.extents.z);
            //calculate the size of the flat hexagon
            hexSize = new Vector3(inst.gameObject.collider.bounds.size.x, inst.gameObject.collider.bounds.size.y, inst.gameObject.collider.bounds.size.z);
            //calculate the center of the flat hexagon
            hexCenter = new Vector3(inst.gameObject.collider.bounds.center.x, inst.gameObject.collider.bounds.center.y, inst.gameObject.collider.bounds.center.z);
            //destroy the temp object that we used to calculate the flat hexagon's size
            Destroy(inst);
        }

        /// <summary>
        /// Creates a new chunk.
        /// </summary>
        /// <param name="x">The width interval of the chunks</param>
        /// <param name="y">The height interval of the chunks</param>
        /// <returns>The new chunk's script</returns>
        private HexChunk NewChunk(int x, int y)
        {
            //if this the first chunk made?
            if (x == 0 && y == 0)
            {
                chunkHolder = new GameObject("ChunkHolder");
            }
            //create the chunk object
            GameObject chunkObj = new GameObject("Chunk[" + x + "," + y + "]");
            //add the hexChunk script and cache it
            HexChunk hexChunk = chunkObj.AddComponent<HexChunk>();
            //assign the size of the chunk
            hexChunk.SetSize(chunkSize, chunkSize);
            //setup HexInfo array
            hexChunk.AllocateHexArray();
            //set the texture map for this chunk and add the mesh renderer
            chunkObj.AddComponent<MeshRenderer>().material.mainTexture = textureAtlas.terrainAtlas;
            //add the mesh filter
            chunkObj.AddComponent<MeshFilter>();
            //make this chunk a child of "ChunkHolder"s
            chunkObj.transform.parent = chunkHolder.transform;

            //return the script on the new chunk 
            return chunkObj.GetComponent<HexChunk>();
        }

        /// <summary>
        /// Generate Chunks to make the map.
        /// </summary>
        private void GenerateMap()
        {

            //determine number of chunks
            xSectors = Mathf.CeilToInt(mapSize.x / chunkSize);
            zSectors = Mathf.CeilToInt(mapSize.y / chunkSize);

            //allocate chunk array
            hexChunks = new HexChunk[xSectors, zSectors];

            //cycle through all chunks
            for (int x = 0; x < xSectors; x++)
            {
                for (int z = 0; z < zSectors; z++)
                {
                    //create the new chunk
                    hexChunks[x, z] = NewChunk(x, z);
                    //set the position of the new chunk
                    hexChunks[x, z].gameObject.transform.position = new Vector3(x * (chunkSize * hexSize.x), 0f, (z * (chunkSize * hexSize.z) * (.75f)));
                    //set hex size for hexagon positioning
                    hexChunks[x, z].hexSize = hexSize;
                    //set the number of hexagons for the chunk to generate
                    hexChunks[x, z].SetSize(chunkSize, chunkSize);
                    //the sector location of the chunk
                    hexChunks[x, z].chunkLocation = new Vector2(x,z);
                    //assign the world manager(this)
                    hexChunks[x, z].worldManager = this;
                }
            }

            //cycle through all chunks
            foreach (HexChunk chunk in hexChunks)
            {
                //begin chunk operations since we are done with value generation
                chunk.Begin();
            }
            //GameManager.worldEvent.Invoke("World Done", null);
        }

        /// <summary>
        /// Use lattitude to determine the biome the tile is in.
        /// </summary>
        /// <param name="x">The x cords of the tile</param>
        /// <param name="h">The h(height) cord of the tile</param>
        /// <returns>An int corresponding to the biome it should be within</returns>
        internal Tile PickTileType(int x, int h)
        {
            //temp no influence from rainfall values
            float latitude = Mathf.Abs((mapSize.y / 2) - h) / (mapSize.y / 2);//1 == snow (top) 0 == eqautor
            //add more results
            latitude *= (1 + UnityEngine.Random.Range(-0.2f, 0.2f));
            latitude = Mathf.Clamp(latitude, 0f, 1f);
            Tile tile;

            if (tileMap.GetPixel(x, h).r == 0)
            {
                if (CheckIfCoast(x, h))
                {
                    tile = tileManager.TryGetShore();
                }
                else
                {
                    tile = tileManager.TryGetOcean();
                }
            }
            else
            {
                tile = tileManager.GetTileFromLattitude(latitude);
            }

            return (tile);
        }

        private bool CheckIfCoast(int x, int y)
        {
            float[] surrondingPixels = CivGridUtility.GetSurrondingPixels(tileMap, x, y);

            int numberWater = 0;
            for (int i = 0; i < 8; i++)
            {
                if (surrondingPixels[i] < 0.5f)
                {
                    numberWater++;
                }
            }

            if (numberWater < 8)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines the tile's <see cref="Feature"/> type from the world map.
        /// </summary>
        /// <param name="xArrayPosition">X array position</param>
        /// <param name="yArrayPosition">Y array position</param>
        /// <param name="edge">If the world is an edge of a chunk</param>
        /// <returns>The correct <see cref="Feature"/> for this tile</returns>
        internal Feature PickFeatureType(int xArrayPosition, int yArrayPosition, bool edge)
        {
            float value = tileMap.GetPixel(xArrayPosition, yArrayPosition).r;
            Feature returnVal = 0;

            if (value == 0.8f)
            {
                returnVal = Feature.Hill;
            }
            else if (value == 1f)
            {
                returnVal = Feature.Mountain;
            }
            else
            {
                returnVal = Feature.Flat;
            }
            if (edge)
            {
                returnVal = Feature.Flat;
            }
            return returnVal;
        }

        void Update()
        {
            if (doneGenerating)
            {
                mousePos = Input.mousePosition;
            }
            RegisterDelegates();
        }

        private void RegisterDelegates()
        {
            HexInfo hex = GetHexFromMouse();

            //OnHexClick
            if (onHexClick != null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (hex != null) { onHexClick.Invoke(hex, 0); }
                }
                if (Input.GetMouseButtonDown(1))
                {
                    if (hex != null) { onHexClick.Invoke(hex, 1); }
                }
            }

            //OnMouseOverHex
            if (onMouseOverHex != null)
            {
                if (hex != null) { onMouseOverHex.Invoke(hex); }
            }
        }


        /// <summary>
        /// Get a hexagon from a world position.
        /// </summary>
        /// <param name="worldPosition">The position of the needed hexagon</param>
        /// <returns>The hex at the nearest position</returns>
        public HexInfo GetHexFromWorldPosition(Vector3 worldPosition)
        {
            HexInfo hexToReturn = null;

            float minDistance = 100;
            foreach (HexChunk chunk in hexChunks)
            {
                foreach (HexInfo hex in chunk.hexArray)
                {
                    //find lowest distance to point
                    float distance = Vector3.Distance(hex.worldPosition, worldPosition);
                    if (distance < minDistance)
                    {
                        hexToReturn = hex;
                        minDistance = distance;
                    }
                }
            }

            return hexToReturn;
        }


        /// <summary>
        /// Get a hexagon from a world position; This is faster than not giving a chunk.
        /// </summary>
        /// <param name="worldPosition">The position of the needed hexagon</param>
        /// <param name="originalchunk">The chunk that contains the hexagon</param>
        /// <returns>The hexagon at the nearest position within the provided chunk</returns>
        public HexInfo GetHexFromWorldPosition(Vector3 worldPosition, HexChunk originalchunk)
        {
            //print(worldPosition);
            HexInfo hexToReturn = null;

            HexChunk[] possibleChunks = FindPossibleChunks(originalchunk);

            float minDistance = 100;

            foreach (HexChunk chunk in possibleChunks)
            {
                foreach (HexInfo hex in chunk.hexArray)
                {
                    //find lowest distance to point
                    float distance = Vector3.Distance(hex.worldPosition, worldPosition);
                    if (distance < minDistance)
                    {
                        hexToReturn = hex;
                        minDistance = distance;
                    }
                }
            }

            return hexToReturn;
        }

        /// <summary>
        /// Get a hexagon from a axial position.
        /// </summary>
        /// <param name="position">Axial position to look for</param>
        /// <returns>The hexagon at the nearest position</returns>
        public HexInfo GetHexFromAxialPosition(Vector2 position)
        {
            foreach (HexChunk chunk in hexChunks)
            {
                foreach (HexInfo hex in chunk.hexArray)
                {
                    if (hex.AxialGridPosition == position) { return hex; }
                }
            }
            return null;
        }


        //testing
        Vector3 hitLocation;
        void OnSceneGUI()
        {
            Debug.Log("lol");
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(hitLocation, 5f);
        }


        /// <summary>
        /// Gets a hexagon from the mouse posion.
        /// </summary>
        /// <returns>The hexagon closest to the mouse position</returns>
        public HexInfo GetHexFromMouse()
        {
            Ray ray1 = civGridCamera.GetCamera(0).ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray1, out chunkHit, 100f))
            {
                hitLocation = chunkHit.point;
                HexChunk chunkHexIsLocatedIn = chunkHit.collider.gameObject.GetComponent<HexChunk>();
                if (chunkHit.collider != null)
                {
                    return GetHexFromWorldPosition(chunkHit.point, chunkHexIsLocatedIn);
                }
            }
            if (civGridCamera.enableWrapping == true)
            {
                Ray ray2 = civGridCamera.GetCamera(1).ScreenPointToRay(mousePos);
                if (Physics.Raycast(ray2, out chunkHit, 100f))
                {
                    hitLocation = chunkHit.point;
                    HexChunk chunkHexIsLocatedIn = chunkHit.collider.gameObject.GetComponent<HexChunk>();
                    if (chunkHit.collider != null)
                    {
                        return GetHexFromWorldPosition(chunkHit.point, chunkHexIsLocatedIn);
                    }
                }
            }

            return null;
        }

        private HexChunk[] FindPossibleChunks(HexChunk chunk)
        {
            HexChunk[] chunkArray;
            if (DetermineWorldEdge(chunk) == false)
            {
                chunkArray = new HexChunk[9];
                chunkArray[0] = hexChunks[(int)chunk.chunkLocation.x + 1, (int)chunk.chunkLocation.y];
                chunkArray[1] = hexChunks[(int)chunk.chunkLocation.x + 1, (int)chunk.chunkLocation.y + 1];
                chunkArray[2] = hexChunks[(int)chunk.chunkLocation.x, (int)chunk.chunkLocation.y + 1];
                chunkArray[3] = hexChunks[(int)chunk.chunkLocation.x - 1, (int)chunk.chunkLocation.y + 1];
                chunkArray[4] = hexChunks[(int)chunk.chunkLocation.x - 1, (int)chunk.chunkLocation.y];
                chunkArray[5] = hexChunks[(int)chunk.chunkLocation.x - 1, (int)chunk.chunkLocation.y - 1];
                chunkArray[6] = hexChunks[(int)chunk.chunkLocation.x, (int)chunk.chunkLocation.y - 1];
                chunkArray[7] = hexChunks[(int)chunk.chunkLocation.x + 1, (int)chunk.chunkLocation.y - 1];
                chunkArray[8] = hexChunks[(int)chunk.chunkLocation.x, (int)chunk.chunkLocation.y];
                return chunkArray;
            }
            else
            {
                CivGridUtility.ToSingleArray<HexChunk>(hexChunks, out chunkArray); return chunkArray;
            }
        }

        private bool DetermineWorldEdge(HexChunk chunk)
        {
            if (chunk.chunkLocation.x == 0 || chunk.chunkLocation.x == ((mapSize.x / chunkSize) - 1))
            {
                return true;
            }

            if (chunk.chunkLocation.y == 0 || chunk.chunkLocation.y == ((mapSize.y / chunkSize) - 1))
            {
                return true;
            }

            return false;
        }

        /*
        void CalculateDistance()
        {
            int dx = Mathf.Abs(Mathf.RoundToInt(goToHex.x - currentHex.x));
            int dy = Mathf.Abs(Mathf.RoundToInt(goToHex.y - currentHex.y));
            int dz = Mathf.Abs(Mathf.RoundToInt(goToHex.z - currentHex.z));

            int distanceA = Mathf.Max(dx, dy, dz);
            int distanceB = Mathf.Abs(distanceA - Mathf.Abs(Mathf.RoundToInt(mapSize.x + dx)));

            if (distanceA == distanceB)
            {
                distance = distanceA;
            }
            else
            {
                distance = Mathf.Min(distanceA, distanceB);
            }
        }

        int CalculateDistance(HexInfo start, HexInfo end)
        {
            int dx = Mathf.Abs(Mathf.RoundToInt(start.CubeGridPosition.x - end.CubeGridPosition.x));
            int dy = Mathf.Abs(Mathf.RoundToInt(start.CubeGridPosition.y - end.CubeGridPosition.y));
            int dz = Mathf.Abs(Mathf.RoundToInt(start.CubeGridPosition.z - end.CubeGridPosition.z));

            int distanceA = Mathf.Max(dx, dy, dz);
            int distanceB = Mathf.Abs(distanceA - Mathf.Abs(Mathf.RoundToInt(mapSize.x + dx)));

            if (distanceA == distanceB)
            {
                return distanceA;
            }
            else
            {
                return Mathf.Min(distanceA, distanceB);
            }
        }

        void OnGUI()
        {
            //GUI.Label(new Rect(20, 0, 100, 20), goToHex.ToString());
            //GUI.Label(new Rect(20, 30, 100, 20), distance.ToString("Distance: #."));
        }
         */
    }

    /// <summary>
    /// The world texture atlas.
    /// </summary>
    /// <remarks>
    /// Contains the locations of each element within the texture.
    /// </remarks>
    [System.Serializable]
    public class TextureAtlas
    {
        /// <summary>
        /// The terrain texture
        /// </summary>
        [SerializeField]
        public Texture2D terrainAtlas;
        /// <summary>
        /// The location of each 
        /// </summary>
        [SerializeField]
        public TileItem[] tileLocations;
        [SerializeField]
        public ResourceItem[] resourceLocations;
        [SerializeField]
        public ImprovementItem[] improvementLocations;
    }

    [System.Serializable]
    public class TileItem
    {
        [SerializeField]
        private Tile key;

        [SerializeField]
        public Tile Key
        {
            get
            {
                return key;
            }
            set
            {
                key = value;
            }
        }

        [SerializeField]
        private Rect value;

        [SerializeField]
        public Rect Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }

        [SerializeField]
        public TileItem(Tile key, Rect value)
        {
            this.key = key;
            this.value = value;
        }

    }

    [System.Serializable]
    public class ResourceItem
    {
        [SerializeField]
        private Resource key;

        [SerializeField]
        public Resource Key
        {
            get
            {
                return key;
            }
            set
            {
                key = value;
            }
        }

        [SerializeField]
        private Rect value;

        [SerializeField]
        public Rect Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }

        [SerializeField]
        public ResourceItem(Resource key, Rect value)
        {
            this.key = key;
            this.value = value;
        }
    }

    [System.Serializable]
    public class ImprovementItem
    {
        [SerializeField]
        private Improvement key;

        [SerializeField]
        public Improvement Key
        {
            get
            {
                return key;
            }
            set
            {
                key = value;
            }
        }

        [SerializeField]
        private Rect value;

        [SerializeField]
        public Rect Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }

        [SerializeField]
        public ImprovementItem(Improvement key, Rect value)
        {
            this.key = key;
            this.value = value;
        }
    }
}