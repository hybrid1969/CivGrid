using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CivGrid;

namespace CivGrid
{
    public enum Feature { Flat = 0, Hill = 1, Mountain = 3 }

    public enum WorldType { Diced, Continents }

    [RequireComponent(typeof(TileManager), typeof(ResourceManager), typeof(ImprovementManager))]
    public class WorldManager : MonoBehaviour
    {
        #region fields

        public CivGridCamera civGridCamera;
        public bool useCivGridCamera;

        public bool generateOnStart;
        private bool doneGenerating;

        //Moving
        public Vector3 currentHex;
        public Vector3 goToHex;
        public int distance;

        //hexInstances
        public Vector3 hexExt;
        public Vector3 hexSize;
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
        public float mountainScaleY;


        //managers
        [HideInInspector]
        public ResourceManager resourceManager;
        [HideInInspector]
        public ImprovementManager improvementManager;
        [HideInInspector]
        public TileManager tileManager;
        #endregion

        /// <summary>
        /// Sets up values for world generation
        /// </summary>
        void Awake()
        {
            resourceManager = GetComponent<ResourceManager>();
            improvementManager = GetComponent<ImprovementManager>();
            tileManager = GetComponent<TileManager>();
            civGridCamera = GameObject.FindObjectOfType<CivGridCamera>();
            if (generateOnStart == true)
            {
                //LoadAndGenerateMap("C:/Users/Landon/Desktop/CivGridRepository/TestProject/terrainTest");
                GenerateNewMap();
                //System.Diagnostics.T
                CivGridSaver.SaveTerrain("terrainTest", this);
            }
            else { civGridCamera.enabled = false; }
        }

        public void GenerateNewMap()
        {
            StartGeneration();
        }

        public void LoadAndGenerateMap(string savedMapLocation)
        {
            CivGridSaver.LoadTerrain(savedMapLocation, this);
            StartGeneration();
        }

        private void StartGeneration()
        {
            resourceManager.SetUp();
            improvementManager.SetUp();
            tileManager.SetUp();
            if (useCivGridCamera)
            {
                civGridCamera.enabled = true;
                if (civGridCamera == null) { Debug.LogError("Please add the 'CivGridCamera' to the mainCamera"); }
                else { civGridCamera.SetupCameras(this); }
            }
            DetermineWorldType();
            GetHexProperties();
            GenerateMap();

            resourceManager.InitResourceTexturesOnHexs();

            doneGenerating = true;
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
            noiseScale = UnityEngine.Random.Range(noiseScale / 1.35f, noiseScale * 1.35f);
            tileMap = NoiseGenerator.SmoothPerlinNoise((int)mapSize.x, (int)mapSize.y, noiseScale);
        }

        /// <summary>
        /// Generates and caches a flat hexagon mesh for all the hexagon's to pull down into their localMesh, if they are flat
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
            //add the hexChunk script and cache it
            HexChunk hexChunk = chunkObj.AddComponent<HexChunk>();
            //assign the size of the chunk
            hexChunk.SetSize(chunkSize, chunkSize);
            //setup HexInfo array
            hexChunk.AllocateHexArray();
            //assign mountain texture to the chunk
            hexChunk.mountainTexture = this.mountainMap;
            //pass down our mountainScaleY
            hexChunk.maxHeight = mountainScaleY;
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
            //GameManager.worldEvent.Invoke("World Done", null);
        }

        /// <summary>
        /// Use lattitude to determine the biome the tile is in
        /// </summary>
        /// <param name="x">The x cords of the tile</param>
        /// <param name="h">The h(height) cord of the tile</param>
        /// <returns>An int corresponding to the biome it should be within</returns>
        public Tile PickHexType(int x, int h)
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

        public Feature PickFeatureType(int xArrayPosition, int yArrayPosition, bool edge)
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
                MouseInput();
            }
        }


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
                        ImprovementManager.TestedAddImprovementToTile(hex, 0);
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
                            ImprovementManager.TestedAddImprovementToTile(hex, 0);
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
        /// Get a hexagon from a world position; This is faster than not giving a chunk
        /// </summary>
        /// <param name="worldPosition">The position of the needed hexagon</param>
        /// <param name="chunk">The chunk that contains the hexagon</param>
        /// <returns>The hex at the nearest position within the provided chunk</returns>
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
                CivGridUtility.ToSingleArray<HexChunk>(hexChunks, out chunkArray); return chunkArray;
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

    [System.Serializable]
    public class TextureAtlas
    {
        [SerializeField]
        public Texture2D terrainAtlas;
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