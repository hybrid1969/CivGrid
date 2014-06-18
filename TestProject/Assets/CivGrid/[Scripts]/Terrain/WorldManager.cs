using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CivGrid;

namespace CivGrid
{
    //public enum Tile { None = 0, Desert = 1, Grass = 2, Grasslands = 3, Ocean = 4, Shore = 5, Snow = 6, Tundra = 7 }
    public enum Feature { Flat = 0, Hill = 1, Mountain = 3, NextToWater = 4 }

    public enum TextureAtlasType { Terrain = 0, Resource = 1, Improvement = 2 }

    public enum WorldType { Diced, Continents }

    [RequireComponent(typeof(TileManager), typeof(ResourceManager), typeof(ImprovementManager))]
    public class WorldManager : MonoBehaviour
    {
        #region fields

        [HideInInspector]
        public CivGridCamera civGridCamera;

        public bool keepSymmetrical;

        //Moving
        [HideInInspector]
        public Vector3 currentHex;
        [HideInInspector]
        public Vector3 goToHex;
        [HideInInspector]
        public int distance;

        //hexInstances
        [HideInInspector]
        public Vector3 hexExt;
        [HideInInspector]
        public Vector3 hexSize;
        [HideInInspector]
        public Vector3 hexCenter;

        //internals
        private Vector3 moveVector;
        private RaycastHit chunkHit;
        private GameObject selectedHex;
        [HideInInspector]
        public Vector2 mousePos;
        private GameObject chunkHolder;
        public HexChunk[,] hexChunks;
        private int xSectors;
        private int zSectors;
        [HideInInspector]
        public Mesh flatHexagonSharedMesh;

        public bool debugMode;
        public bool useWorldTypeValues;

        //World Values
        private Texture2D tileMap;
        public float noiseScale;

        [SerializeField]
        public TextureAtlas textureAtlas;

        //World Settings
        public WorldType worldType;
        public Vector2 mapSize;
        public int chunkSize;
        public float hexRadiusSize;

        //Hill and mountains
        [SerializeField]
        public Texture2D mountainMap;


        //managers
        [HideInInspector]
        public ResourceManager rM;
        [HideInInspector]
        public ImprovementManager iM;
        [HideInInspector]
        public TileManager tM;
        #endregion

        /// <summary>
        /// Sets up values for world generation
        /// </summary>
        void Awake()
        {
            rM = GetComponent<ResourceManager>();
            iM = GetComponent<ImprovementManager>();
            tM = GetComponent<TileManager>();
            rM.SetUp();
            iM.SetUp();
            civGridCamera = Camera.main.GetComponent<CivGridCamera>();
            if (civGridCamera == null) { Debug.LogError("Please add the 'CivGridCamera' to the mainCamera"); }
            DetermineWorldType();
            GetHexProperties();
            GenerateMap();

            rM.InitResourceTexturesOnHexs();
        }

        void SetNoiseScaleToTrueValue()
        {
            noiseScale /= mapSize.magnitude;
        }

        //sets the tileMap to the appripriet map
        void DetermineWorldType()
        {
            if (useWorldTypeValues)
            {
                if (worldType == WorldType.Continents) { noiseScale = 5f; }
                else if (worldType == WorldType.Diced) { noiseScale = 10f; }
            }

            SetNoiseScaleToTrueValue();
            if (noiseScale == 0) { Debug.LogException(new UnityException("Noise scale is zero, this produces artifacts.")); }
            noiseScale = Random.Range(noiseScale / 1.35f, noiseScale * 1.35f);
            tileMap = NoiseGenerator.SmoothPerlinNoise((int)mapSize.x, (int)mapSize.y, noiseScale);
        }

        void Start()
        {
            civGridCamera.SetupCameras(this);
            Saver.SaveTexture(tileMap, "terrian", false);
            //Saver.SaveTerrain("terrainTest", this);
            //Saver.LoadTerrain("terrainTest", this);
        }

        /// <summary>
        /// Set up dimensions of the hexagons; used for spacing and other algorithms
        /// </summary>
        void GetHexProperties()
        {

            //creates GameObject that holds our test mesh to calculate bounds/dimensions
            GameObject inst = new GameObject("Bounds Set Up: Flat");
            //creates a MeshFilter to hold our mesh data
            inst.AddComponent<MeshFilter>();
            //creates a MeshCollider that we can use to get size dimesnsions easily from
            inst.AddComponent<MeshCollider>();
            
            //set to zero position/rotation to eliminate local/world confusion
            inst.transform.position = Vector3.zero;
            inst.transform.rotation = Quaternion.identity;

            //array of vertices we will generate for our test mesh
            Vector3[] vertices;
            //array of triangles we will generate for our test mesh
            int[] triangles;

            #region verts

            //y position of the vertices; consistant to create flat hexagons
            float floorLevel = 0;
            //setting our vertex position using hexRadiusSize to determine radius size that generates a geometric regular hexagon
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

            //setting triangles for our hexagon
            triangles = new int[] 
            {
                1,5,0,
                1,4,5,
                1,2,4,
                2,3,4
            };

            #endregion

            #region uv

            Vector2[] uv = new Vector2[]
            {
                new Vector2(0,0.25f),
                new Vector2(0,0.75f),
                new Vector2(0.5f,1),
                new Vector2(1,0.75f),
                new Vector2(1,0.25f),
                new Vector2(0.5f,0),
            };

            #endregion

            #region finalize
            flatHexagonSharedMesh = new Mesh();
            flatHexagonSharedMesh.vertices = vertices;
            flatHexagonSharedMesh.triangles = triangles;
            flatHexagonSharedMesh.uv = uv;
            inst.GetComponent<MeshFilter>().mesh = flatHexagonSharedMesh;
            inst.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            inst.GetComponent<MeshCollider>().sharedMesh = flatHexagonSharedMesh;
            #endregion

            hexExt = new Vector3(inst.gameObject.collider.bounds.extents.x, inst.gameObject.collider.bounds.extents.y, inst.gameObject.collider.bounds.extents.z);
            hexSize = new Vector3(inst.gameObject.collider.bounds.size.x, inst.gameObject.collider.bounds.size.y, inst.gameObject.collider.bounds.size.z);
            hexCenter = new Vector3(inst.gameObject.collider.bounds.center.x, inst.gameObject.collider.bounds.center.y, inst.gameObject.collider.bounds.center.z);
            Destroy(inst);
        }

        /// <summary>
        /// Creates a new chunk
        /// </summary>
        /// <param name="x">The width interval of the chunks</param>
        /// <param name="y">The height interval of the chunks</param>
        /// <returns>The new chunk's script</returns>
        public HexChunk NewChunk(int x, int y)
        {
            //if this the first chunk made?
            if (x == 0 && y == 0)
            {
                chunkHolder = new GameObject("ChunkHolder");
            }
            //create the chunk object
            GameObject chunkObj = new GameObject("Chunk[" + x + "," + y + "]");
            //add the hexChunk script and set it's size
            chunkObj.AddComponent<HexChunk>().SetSize(chunkSize, chunkSize);
            //allocate the hexagon array
            chunkObj.GetComponent<HexChunk>().AllocateHexArray();
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
        /// Generate Chunks to make the map
        /// </summary>
        void GenerateMap()
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
                    //the width interval of the chunk
                    hexChunks[x, z].xSector = x;
                    //set the height interval of the chunk
                    hexChunks[x, z].ySector = z;
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

        }

        /// <summary>
        /// Use lattitude to determine the biome the tile is in
        /// </summary>
        /// <param name="x">The x cords of the tile</param>
        /// <param name="h">The h(height) cord of the tile</param>
        /// <returns>An int corresponding to the biome it should be within</returns>
        public Tile PickHex(int x, int h)
        {
            //temp no influence from rainfall values
            float latitude = Mathf.Abs((mapSize.y / 2) - h) / (mapSize.y / 2);//1 == snow (top) 0 == eqautor
            //add more results
            latitude *= (1 + Random.Range(-0.2f, 0.2f));
            Tile tile;

            if (tileMap.GetPixel(x, h).r == 0)
            {
                tile = tM.GetOcean();
            }
            else
            {
                tile = tM.GetTileFromLattitude(latitude);
                /*
                if (latitude > 0.9f)
                {
                    tile = Tile.Snow;
                }
                else if (latitude > 0.8 && latitude < 0.9)
                {
                    tile = (Tile)Random.Range(5, 7);
                }
                else if (latitude > 0.6f && latitude < 0.8f)
                {
                    tile = Tile.Tundra;
                }
                else if (latitude >= 0.5 && latitude < 0.61)
                {
                    int index = Random.Range(0, 2);
                    tile = (Tile)((index < 0.5f) ? Tile.Tundra : Tile.Grass);
                }
                else if (latitude < 0.5f && latitude > 0.35f)
                {
                    tile = Tile.Grass;
                }
                else if (latitude > 0.3 && latitude < 0.35)
                {
                    int index = Random.Range(0, 2);
                    tile = (Tile)((index < 0.5f) ? Tile.Grass : Tile.Grasslands);
                }
                else if (latitude < 0.3f && latitude >= 0.15f)
                {
                    tile = Tile.Grasslands;
                }
                else if (latitude >= 0.1f && latitude < 0.15f)
                {
                    int index = Random.Range(0, 2);
                    tile = (Tile)((index < 0.5f) ? Tile.Grasslands : Tile.Desert);
                }
                else
                {
                    tile = Tile.Desert;
                    if (latitude < 0.1f == false)
                        print("error incorrrect lattitude: " + latitude);
                }
                 */
            }

            return (tile);
        }

        public Feature PickFeature(int xArrayPosition, int yArrayPosition, bool edge)
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
            if (edge)
            {
                returnVal = Feature.Flat;
            }
            return returnVal;
        }

        #region Updates

        void Update()
        {
            mousePos = Input.mousePosition;
            MouseInput();
        }
        #endregion


        void MouseInput()
        {

            Ray ray1 = civGridCamera.cam1.ScreenPointToRay(mousePos);
            Vector3 mouseWorldPosition = new Vector3();

            if (Physics.Raycast(ray1, out chunkHit, 100f))
            {
                HexChunk chunkHexIsLocatedIn = chunkHit.collider.gameObject.GetComponent<HexChunk>();
                if (chunkHit.collider != null)
                {
                    mouseWorldPosition = chunkHit.point;
                    HexInfo hex = GetHexFromWorldPosition(mouseWorldPosition, chunkHexIsLocatedIn);
                    if (Input.GetMouseButtonDown(0))
                    {
                        ImprovementManager.TestedAddImprovementToTile(hex, "Farm");
                        return;
                    }
                    if (Input.GetMouseButtonDown(1))
                    {
                        ImprovementManager.RemoveImprovementFromTile(hex);
                        return;
                    }
                }
            }
            if (civGridCamera.enableWrapping == true)
            {
                var ray2 = civGridCamera.cam2.ScreenPointToRay(mousePos);
                if (Physics.Raycast(ray2, out chunkHit, 100f))
                {
                    HexChunk chunkHexIsLocatedIn = chunkHit.collider.gameObject.GetComponent<HexChunk>();
                    if (chunkHit.collider != null)
                    {
                        mouseWorldPosition = chunkHit.point;
                        HexInfo hex = GetHexFromWorldPosition(mouseWorldPosition, chunkHexIsLocatedIn);
                        if (Input.GetMouseButtonDown(0))
                        {
                            ImprovementManager.TestedAddImprovementToTile(hex, "Farm");
                            return;
                        }
                        if (Input.GetMouseButtonDown(1))
                        {
                            ImprovementManager.RemoveImprovementFromTile(hex);
                            return;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Get a hexagon from a world position
        /// </summary>
        /// <param name="worldPosition">The position of the needed hexagon</param>
        /// <returns>The hex at the nearest position</returns>
        HexInfo GetHexFromWorldPosition(Vector3 worldPosition)
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
        /// Get a hexagon from a world position; This is faster than not giving a chunk
        /// </summary>
        /// <param name="worldPosition">The position of the needed hexagon</param>
        /// <param name="chunk">The chunk that contains the hexagon</param>
        /// <returns>The hex at the nearest position within the provided chunk</returns>
        HexInfo GetHexFromWorldPosition(Vector3 worldPosition, HexChunk originalchunk)
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

        private HexChunk[] FindPossibleChunks(HexChunk chunk)
        {
            HexChunk[] chunkArray;
            if (DetermineWorldEdge(chunk) == false)
            {
                chunkArray = new HexChunk[9];
                chunkArray[0] = hexChunks[chunk.xSector + 1, chunk.ySector];
                chunkArray[1] = hexChunks[chunk.xSector + 1, chunk.ySector + 1];
                chunkArray[2] = hexChunks[chunk.xSector, chunk.ySector + 1];
                chunkArray[3] = hexChunks[chunk.xSector - 1, chunk.ySector + 1];
                chunkArray[4] = hexChunks[chunk.xSector - 1, chunk.ySector];
                chunkArray[5] = hexChunks[chunk.xSector - 1, chunk.ySector - 1];
                chunkArray[6] = hexChunks[chunk.xSector, chunk.ySector - 1];
                chunkArray[7] = hexChunks[chunk.xSector + 1, chunk.ySector - 1];
                chunkArray[8] = hexChunks[chunk.xSector, chunk.ySector];
                return chunkArray;
            }
            else
            {
                CivGridUtility.ToSingleArray(hexChunks, out chunkArray); return chunkArray;
            }
        }

        private bool DetermineWorldEdge(HexChunk chunk)
        {
            if (chunk.xSector == 0 || chunk.xSector == ((mapSize.x / chunkSize) - 1))
            {
                return true;
            }

            if (chunk.ySector == 0 || chunk.ySector == ((mapSize.y / chunkSize) - 1))
            {
                return true;
            }

            return false;
        }

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
            GUI.Label(new Rect(20, 0, 100, 20), goToHex.ToString());
            GUI.Label(new Rect(20, 30, 100, 20), distance.ToString("Distance: #."));
        }
    }
}