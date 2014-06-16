﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#region namespace defines
using CivGrid;
#endregion

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

        public string name;
        public bool isSelected;
        public Unit currentUnit;
        public bool isWater;
        public Tile terrainType;
        public Feature terrainFeature;
        public Vector3[] vertsx;

        public HexChunk parentChunk;

        public Vector3 hexExt;
        public Vector3 hexCenter;


        //basic hexagon mesh making
        public Vector3[] vertices;
        public Vector2[] UV;
        public int[] triangles;

        //resources
        [SerializeField]
        public Resource currentResource;
        public ResourceManager rM;
        public bool hideResources;
        public List<Vector3> resourceLocations = new List<Vector3>();

        //improvments
        [SerializeField]
        public Improvement currentImprovement;
        public List<GameObject> improvementGameObjects = new List<GameObject>();

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



        public void Start()
        {
            MeshSetup();
            rM.CheckForResource(this, out currentResource);
            worldTextureAtlas = parentChunk.worldManager.textureAtlas;

            if (terrainType == Tile.Ocean) { isWater = true; }
        }

        public void ChangeTextureToResource()
        {
            if (terrainFeature == Feature.Flat)
            {
                AssignUV(worldTextureAtlas.resourceLocations.TryGetValue(currentResource));
            }
            else if (terrainFeature == Feature.Hill || terrainFeature == Feature.Mountain)
            {
                AssignPresetUV(localMesh, worldTextureAtlas.resourceLocations.TryGetValue(currentResource));
            }
            parentChunk.RegenerateMesh();
        }

        public void ChangeTextureToImprovement()
        {
            if (terrainFeature == Feature.Flat)
            {
                AssignUV(worldTextureAtlas.improvementLocations.TryGetValue(currentImprovement));
            }
            else if (terrainFeature == Feature.Hill || terrainFeature == Feature.Mountain)
            {
                AssignPresetUV(localMesh, worldTextureAtlas.improvementLocations.TryGetValue(currentImprovement));
            }
            parentChunk.RegenerateMesh();
        }

        public void ChangeTextureToNormalTile()
        {
            if (terrainFeature == Feature.Flat)
            {
                AssignUVToDefaultTile();
            }
            else if (terrainFeature == Feature.Hill || terrainFeature == Feature.Mountain)
            {
                AssignPresetUV(localMesh, worldTextureAtlas.tileLocations.TryGetValue(terrainType));
            }
            parentChunk.RegenerateMesh();
        }

        public void MeshSetup()
        {
            localMesh = new Mesh();

            #region Flat
            if (terrainFeature == Feature.Flat || terrainFeature == Feature.NextToWater)
            {
                localMesh.vertices = parentChunk.worldManager.flatHexagonSharedMesh.vertices;
                localMesh.triangles = parentChunk.worldManager.flatHexagonSharedMesh.triangles;
                localMesh.RecalculateBounds();

                AssignUVToDefaultTile();

            }
            #endregion
            #region Feature
            else if (terrainFeature == Feature.Mountain || terrainFeature == Feature.Hill)
            {
                Texture2D localMountainTexture = new Texture2D(parentChunk.mountainTexture.width, parentChunk.mountainTexture.height);
                if (terrainFeature == Feature.Mountain)
                {
                    localMountainTexture = NoiseGenerator.RandomOverlay(parentChunk.mountainTexture, Random.Range(-100f, 100f), Random.Range(0.005f, 0.18f), Random.Range(0.2f, 0.5f), Random.Range(0.3f, 0.6f), parentChunk.maxHeight, true);
                }
                else if (terrainFeature == Feature.Hill)
                {
                    localMountainTexture = NoiseGenerator.RandomOverlay(parentChunk.mountainTexture, Random.Range(-100f, 100f), Random.Range(0.005f, 0.18f), Random.Range(0.75f, 1f), Random.Range(0.4f, 0.7f), parentChunk.maxHeight, true);
                }

                int width = Mathf.Min(localMountainTexture.width, 255);
                int height = Mathf.Min(localMountainTexture.height, 255);
                int y = 0;
                int x = 0;

                // Build vertices
                vertices = new Vector3[height * width];
                Vector4[] tangents = new Vector4[height * width];

                Vector2 uvScale = new Vector2(1.0f / width, 1.0f / height);
                Vector3 sizeScale = new Vector3(parentChunk.hexSize.x / (width * 0.9f), parentChunk.maxHeight, parentChunk.hexSize.z / (height * 0.9f));
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
                        Vector3 vertex = new Vector3(x, pixelHeight - (parentChunk.maxHeight / 100), y);
                        vertices[y * width + x] = Vector3.Scale(sizeScale, vertex);
                        rawUV[y * width + x] = Vector2.Scale(uvScale, new Vector2(vertex.x, vertex.z));

                        //rawUV[y * width + x].x /= parentChunk.worldManager.textureAtlas.texturesInAtlas.x;
                        //rawUV[y * width + x].y /= parentChunk.worldManager.textureAtlas.texturesInAtlas.y;

                        // Calculate tangent vector: a vector that goes from previous vertex
                        // to next along X direction. We need tangents if we intend to
                        // use bumpmap shaders on the mesh.
                        Vector3 vertexL = new Vector3(x - 1, localMountainTexture.GetPixel(x - 1, y).grayscale, y);
                        Vector3 vertexR = new Vector3(x + 1, localMountainTexture.GetPixel(x + 1, y).grayscale, y);
                        Vector3 tan = Vector3.Scale(sizeScale, vertexR - vertexL).normalized;
                        tangents[y * width + x] = new Vector4(tan.x, tan.y, tan.z, -1.0f);
                    }
                }
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

                AssignPresetUVToDefaultTile(rawUV);
            }
            #endregion
        }

        public void RemoveResources()
        {
            resourceLocations = new List<Vector3>();
            MeshSetup();
        }

        public void CombineWithOthers(int size, Vector3[] positions)
        {
            //AssignPresetUV(currentResource.meshToSpawn, 0, 0);

            //number of resources to combine
            if (size > 0)
            {
                //combine instances
                CombineInstance[] combine = new CombineInstance[size + 1];
                //set first mesh to the hexagon
                combine[0].mesh = localMesh;
                Matrix4x4 matrix = new Matrix4x4();
                matrix.SetTRS(Vector3.zero, Quaternion.identity, Vector3.one);
                combine[0].transform = matrix;
                localMesh = new Mesh();


                //skip first combine instance due to presetting
                for (int i = 0; i < size; i++)
                {
                    combine[i + 1].mesh = currentResource.meshToSpawn;
                    matrix = new Matrix4x4();
                    matrix.SetTRS(positions[i], Quaternion.identity, Vector3.one);
                    combine[i + 1].transform = matrix;
                }

                localMesh.CombineMeshes(combine);
                //AssignFeatureUV(terrainType, GetRawUV(), (1f / 3f));
                localMesh.RecalculateNormals();
                localMesh.RecalculateBounds();
            }
        }

        private void AssignUVToDefaultTile()
        {
            Vector2[] rawUV = parentChunk.worldManager.flatHexagonSharedMesh.uv;

            Rect rectArea = parentChunk.worldManager.textureAtlas.tileLocations.TryGetValue(terrainType);

            UV = new Vector2[rawUV.Length];

            for (int i = 0; i < rawUV.Length; i++)
            {
                UV[i] = new Vector2(rawUV[i].x * rectArea.width + rectArea.x, rawUV[i].y * rectArea.height + rectArea.y);

                UV[i] = new Vector2(Mathf.Clamp(UV[i].x, 0.1f, 0.9f), Mathf.Clamp(UV[i].y, 0.1f, 0.9f));
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

                UV[i] = new Vector2(Mathf.Clamp(UV[i].x, 0.1f, 0.9f), Mathf.Clamp(UV[i].y, 0.1f, 0.9f));
            }

            localMesh.uv = UV;
        }

        /// <summary>
        /// Assign the UV maps for a hexagon with a feature(mountain, hill, etc)
        /// </summary>
        /// <param name="rawUV">UV map locations for (0,0) sector of texture atlas</param>
        /// <param name="sectorPercentage">Percent each sector in the atlas takes up in one direction (1/numberOfSectorsInOneDirection)</param>
        private void AssignPresetUVToDefaultTile(Vector2[] rawUV)
        {
            Rect rectArea = parentChunk.worldManager.textureAtlas.tileLocations.TryGetValue(terrainType);

            UV = new Vector2[localMesh.vertexCount];

            for (int i = 0; i < localMesh.vertexCount; i++)
            {
                UV[i] = new Vector2(rawUV[i].x * rectArea.width + rectArea.x, rawUV[i].y  * rectArea.height + rectArea.y);

                UV[i] = new Vector2(Mathf.Clamp(UV[i].x, 0.1f, 0.9f), Mathf.Clamp(UV[i].y, 0.1f, 0.9f));
            }

            localMesh.uv = UV;
        }

        private void AssignPresetUV(Mesh mesh, Rect rectArea)
        {
            UV = new Vector2[localMesh.vertexCount];

            for (int i = 0; i < localMesh.vertexCount; i++)
            {
                UV[i] = new Vector2(mesh.uv[i].x * rectArea.width + rectArea.x, mesh.uv[i].y * rectArea.height + rectArea.y);

                UV[i] = new Vector2(Mathf.Clamp(UV[i].x, 0.1f, 0.9f), Mathf.Clamp(UV[i].y, 0.1f, 0.9f));
            }

            mesh.uv = UV;
        }
    }
}