using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CivGrid;

namespace CivGrid
{

    [System.Serializable]
    public class HexInfo
    {
        //positioning
        private Vector3 gridPosition;//cube cordinates stored(x,y == axial)
        public Vector3 localPosition;
        public Vector3 worldPosition;
        public Vector2 pixelLocation;

        public bool isSelected;
        public Unit currentUnit;
        public Tile terrainType;
        public Feature terrainFeature;
        public Vector3[] vertsx;
        Vector2[] baseFeatureUv;

        public HexChunk parentChunk;

        public Vector3 hexExt;
        public Vector3 hexCenter;


        //basic hexagon mesh making
        public Vector3[] vertices;
        public Vector2[] UV;
        public Rect rectLocation;
        public Rect defaultRectLocation;
        public int[] triangles;

        //resources
        [SerializeField]
        public Resource currentResource;
        public ResourceManager resourceManager;
        public List<Vector3> resourceLocations = new List<Vector3>();

        public GameObject iObject;
        public GameObject rObject;

        //improvments
        [SerializeField]
        public Improvement currentImprovement;
        public ImprovementManager improvementManager;

        public Mesh localMesh;

        TextureAtlas worldTextureAtlas;

        //get axial grid position
        public Vector2 AxialGridPosition
        {
            get { return new Vector2(CubeGridPosition.x, CubeGridPosition.y); }
        }
        //get/set cube grid position
        public Vector3 CubeGridPosition
        {
            get { return gridPosition; }
            set { gridPosition = value; }
        }



        /// <summary>
        /// This is the setup called from HexChunk when it's ready for us to generate our meshes
        /// </summary>
        public void Start()
        {
            if (terrainFeature == Feature.Mountain) { Tile mountain = parentChunk.worldManager.tileManager.TryGetMountain(); if (mountain != null) {  terrainType = mountain; } }
            MeshSetup();
            if (parentChunk.worldManager.generateNewValues == true)
            {
                currentImprovement = improvementManager.searalizableImprovements[0];
                resourceManager.CheckForResource(this, out currentResource);
            }
            worldTextureAtlas = parentChunk.worldManager.textureAtlas;
        }

        public void ChangeTextureToResource()
        {
            if (currentResource.replaceGroundTexture == true)
            {
                if (terrainFeature == Feature.Flat)
                {
                    worldTextureAtlas.resourceLocations.TryGetValue(currentResource, out rectLocation);
                    AssignUV(rectLocation);
                }
                else if (terrainFeature == Feature.Hill || terrainFeature == Feature.Mountain)
                {
                    worldTextureAtlas.resourceLocations.TryGetValue(currentResource, out rectLocation);
                    AssignPresetUV(localMesh, rectLocation);
                }
                parentChunk.RegenerateMesh();
            }
        }

        public void ChangeTextureToImprovement()
        {
            if (currentImprovement.replaceGroundTexture == true)
            {
                if (terrainFeature == Feature.Flat)
                {
                    worldTextureAtlas.improvementLocations.TryGetValue(currentImprovement, out rectLocation);
                    AssignUV(rectLocation);
                }
                else if (terrainFeature == Feature.Hill || terrainFeature == Feature.Mountain)
                {
                    worldTextureAtlas.improvementLocations.TryGetValue(currentImprovement, out rectLocation);
                    AssignPresetUV(localMesh, rectLocation);
                }
                parentChunk.RegenerateMesh();
            }
        }

        public void ChangeTextureToNormalTile()
        {
            if (currentImprovement.replaceGroundTexture == true || currentResource.replaceGroundTexture == true)
            {
                if (terrainFeature == Feature.Flat)
                {
                    AssignUVToDefaultTile();
                }
                else if (terrainFeature == Feature.Hill || terrainFeature == Feature.Mountain)
                {
                    AssignPresetUVToDefaultTile(baseFeatureUv);
                }
                parentChunk.RegenerateMesh();
            }
        }

        public void MeshSetup()
        {
            //create new mesh to start fresh
            localMesh = new Mesh();

            #region Flat
            if (terrainFeature == Feature.Flat)
            {
                //pull mesh data from WorldManager
                localMesh.vertices = parentChunk.worldManager.flatHexagonSharedMesh.vertices;
                localMesh.triangles = parentChunk.worldManager.flatHexagonSharedMesh.triangles;
                localMesh.uv = parentChunk.worldManager.flatHexagonSharedMesh.uv;

                //recalculate normals to play nicely with lighting
                localMesh.RecalculateNormals();

                AssignUVToDefaultTile();

            }
            #endregion
            #region Feature
            else if (terrainFeature == Feature.Mountain || terrainFeature == Feature.Hill)
            {
                Texture2D localMountainTexture = new Texture2D(parentChunk.worldManager.mountainMap.width, parentChunk.worldManager.mountainMap.height);
                if (terrainFeature == Feature.Mountain)
                {
                    localMountainTexture = NoiseGenerator.RandomOverlay(parentChunk.worldManager.mountainMap, Random.Range(-100f, 100f), Random.Range(0.005f, 0.18f), Random.Range(0.2f, 0.5f), Random.Range(0.3f, 0.6f), parentChunk.worldManager.mountainScaleY, true);
                }
                else if (terrainFeature == Feature.Hill)
                {
                    localMountainTexture = NoiseGenerator.RandomOverlay(parentChunk.worldManager.mountainMap, Random.Range(-100f, 100f), Random.Range(0.005f, 0.18f), Random.Range(0.75f, 1f), Random.Range(0.4f, 0.7f), parentChunk.worldManager.mountainScaleY, true);
                }

                int width = Mathf.Min(localMountainTexture.width, 255);
                int height = Mathf.Min(localMountainTexture.height, 255);
                int y = 0;
                int x = 0;

                // Build vertices
                vertices = new Vector3[height * width];
                Vector4[] tangents = new Vector4[height * width];

                Vector2 uvScale = new Vector2(1.0f / width, 1.0f / height);
                Vector3 sizeScale = new Vector3(parentChunk.hexSize.x / (width * 0.9f), parentChunk.worldManager.mountainScaleY, parentChunk.hexSize.z / (height * 0.9f));
                Vector2[] rawUV = new Vector2[height * width];

                #region verts
                for (y = 0; y < height; y++)
                {
                    for (x = 0; x < width; x++)
                    {
                        float pixelHeight;
                        if (terrainFeature == Feature.Hill)
                        {
                            pixelHeight = localMountainTexture.GetPixel(x, y).grayscale / 2;
                        }
                        else if (terrainFeature == Feature.Mountain)
                        {
                            pixelHeight = localMountainTexture.GetPixel(x, y).grayscale * 3;
                        }
                        else
                        {
                            pixelHeight = localMountainTexture.GetPixel(x, y).grayscale;
                        }
                        Vector3 vertex = new Vector3(x, pixelHeight - (parentChunk.worldManager.mountainScaleY / 100), y);
                        vertices[y * width + x] = Vector3.Scale(sizeScale, vertex);
                        rawUV[y * width + x] = Vector2.Scale(uvScale, new Vector2(vertex.x, vertex.z));

                        // Calculate tangent vector: a vector that goes from previous vertex
                        // to next along X direction. We need tangents if we intend to
                        // use bumpmap shaders on the mesh.
                        Vector3 vertexL = new Vector3(x - 1, localMountainTexture.GetPixel(x - 1, y).grayscale, y);
                        Vector3 vertexR = new Vector3(x + 1, localMountainTexture.GetPixel(x + 1, y).grayscale, y);
                        Vector3 tan = Vector3.Scale(sizeScale, vertexR - vertexL).normalized;
                        tangents[y * width + x] = new Vector4(tan.x, tan.y, tan.z, -1.0f);
                    }
                }
                baseFeatureUv = rawUV;
                #endregion

                // Assign them to the mesh
                localMesh.vertices = vertices;
                localMesh.RecalculateBounds();

                //Move verts to compensate for mesh creation differences
                Vector3 moveVector;
                moveVector = new Vector3(localMesh.bounds.size.x / 2, 0, localMesh.bounds.size.z / 2);
                Vector3[] tempVerts = localMesh.vertices;

                for (var i = 0; i < localMesh.vertexCount; i++)
                {
                    tempVerts[i] -= moveVector;
                }

                localMesh.vertices = tempVerts;

                #region triangles
                // Build triangle indices: 3 indices into vertex array for each triangle
                triangles = new int[vertices.Length * 6];
                int index = 0;
                for (y = 0; y < height - 1; y++)
                {
                    for (x = 0; x < width - 1; x++)
                    {
                        // For each grid cell output two triangles
                        triangles[index++] = (y * width) + x;
                        triangles[index++] = ((y + 1) * width) + x;
                        triangles[index++] = (y * width) + x + 1;

                        triangles[index++] = ((y + 1) * width) + x;
                        triangles[index++] = ((y + 1) * width) + x + 1;
                        triangles[index++] = (y * width) + x + 1;
                    }
                }
                #endregion

                // And assign them to the mesh
                localMesh.triangles = triangles;

                // Auto-calculate vertex normals from the mesh
                localMesh.RecalculateNormals();

                // Assign tangents after recalculating normals
                localMesh.tangents = tangents;

                hexExt = localMesh.bounds.extents;

                AssignPresetUVToDefaultTile(rawUV);
            }
            #endregion
        }

        private void AssignUVToDefaultTile()
        {
            Vector2[] rawUV = parentChunk.worldManager.flatHexagonSharedMesh.uv;

            if (parentChunk.worldManager.generateNewValues == true)
            {
                if (defaultRectLocation == new Rect())
                {
                    parentChunk.worldManager.textureAtlas.tileLocations.TryGetValue(terrainType, out rectLocation);
                    defaultRectLocation = rectLocation;
                }
                else { rectLocation = defaultRectLocation; }
            }

            UV = new Vector2[rawUV.Length];

            for (int i = 0; i < rawUV.Length; i++)
            {
                UV[i] = new Vector2(rawUV[i].x * rectLocation.width + rectLocation.x, rawUV[i].y * rectLocation.height + rectLocation.y);
            }

            localMesh.uv = UV;
        }

        private void AssignUV(Rect rectArea)
        {

            Vector2[] rawUV = parentChunk.worldManager.flatHexagonSharedMesh.uv;

            UV = new Vector2[rawUV.Length];

            for (int i = 0; i < rawUV.Length; i++)
            {
                UV[i] = new Vector2(rawUV[i].x * rectArea.width + rectArea.x, rawUV[i].y * rectArea.height + rectArea.y);
                
                //UV[i] = new Vector2(Mathf.Clamp(UV[i].x, 0.1f, 0.9f), Mathf.Clamp(UV[i].y, 0.1f, 0.9f));
            }

            localMesh.uv = UV;
        }

        /// <summary>
        /// Assign the UV maps for a hexagon with a feature(mountain, hill, etc)
        /// </summary>
        /// <param name="rawUV">UV map locations for (0,0) sector of texture atlas</param>
        private void AssignPresetUVToDefaultTile(Vector2[] rawUV)
        {
            if (parentChunk.worldManager.generateNewValues == true)
            {
                if (defaultRectLocation == new Rect())
                {
                    parentChunk.worldManager.textureAtlas.tileLocations.TryGetValue(terrainType, out rectLocation);
                    defaultRectLocation = rectLocation;
                }
                else { rectLocation = defaultRectLocation; }
            }

            UV = new Vector2[localMesh.vertexCount];

            for (int i = 0; i < localMesh.vertexCount; i++)
            {
                UV[i] = new Vector2(rawUV[i].x * rectLocation.width + rectLocation.x, rawUV[i].y * rectLocation.height + rectLocation.y);
            }

            localMesh.uv = UV;
        }

        private void AssignPresetUV(Mesh mesh, Rect rectArea)
        {
            UV = new Vector2[mesh.vertexCount];

            for (int i = 0; i < mesh.vertexCount; i++)
            {
                UV[i] = new Vector2(mesh.uv[i].x * rectArea.width + rectArea.x, mesh.uv[i].y * rectArea.height + rectArea.y);
            }

            localMesh.uv = UV;
        }

        private void AssignPresetUV(Vector2[] uv, Rect rectArea)
        {
            UV = new Vector2[uv.Length];

            for (int i = 0; i < uv.Length; i++)
            {
                UV[i] = new Vector2(uv[i].x * rectArea.width + rectArea.x, uv[i].y * rectArea.height + rectArea.y);
            }

            localMesh.uv = uv;
        }
    }
}