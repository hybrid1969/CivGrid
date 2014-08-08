using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CivGrid;
using CivGrid.SampleResources;

namespace CivGrid
{
    /// <summary>
    /// Contains all hexagon data and methods.
    /// Generates it's localMesh and uploads this to the chunk.
    /// Generates it's UV data depending on constraints.
    /// </summary>
    [System.Serializable]
    public class HexInfo
    {
        //positioning
        private Vector3 gridPosition;//cube cordinates stored(x,y == axial)
        /// <summary>
        /// The position of this hexagon local to the parent chunk.
        /// </summary>
        /// <remarks>
        /// This is the local position with the origin being the chunk. Therefore, if the chunk is located
        /// at (10,10,10) in world space and the hexagon is located at (12,12,12) in world space. This values would
        /// contain (2,2,2).
        /// </remarks>
        public Vector3 localPosition;
        /// <summary>
        /// The position of the hexagon in world space.
        /// </summary>
        public Vector3 worldPosition;

        /// <summary>
        /// Ignore this field block; used for testing a sample game.
        /// </summary>
        public bool isSelected;
        internal Unit currentUnit;
        
        /// <summary>
        /// The type of terrain that this hexagon represents.
        /// </summary>
        /// <remarks>
        /// Since this is represented by a class that is generated from user data, differation is possible.
        /// </remarks>
        public Tile terrainType;
        /// <summary>
        /// The type of terrain feature that this hexagon includes.
        /// </summary>
        /// <remarks>
        /// Since this is represented by an <see cref="System.Enum"/>, it is considered failsafe.
        /// </remarks>
        public Feature terrainFeature;
       
        /// <summary>
        /// The chunk that this hexagon is within.
        /// </summary>
        public HexChunk parentChunk;

        private TextureAtlas worldTextureAtlas;

        /// <summary>
        /// The current location of the texture that the hexagon is using.
        /// </summary>
        public Rect currentRectLocation;
        /// <summary>
        /// The location of the base terrain texture.
        /// </summary>
        /// <remarks>
        /// This value should not be changed, as it holds the location to pull the default terrain
        /// texture from.
        /// </remarks>
        /// <example>
        /// If the <see cref="terrainType"/> is set to a <see cref="Tile"/> with the name of "Grass", this value
        /// will hold the location of the selected grass texture in the terrain atlas.
        /// </example>
        public Rect defaultRectLocation;
        private Vector2[] baseFeatureUV;
        internal Vector3 hexExt;
        internal Vector3 hexCenter;
        /// <summary>
        /// The mesh of this hexagon.
        /// </summary>
        /// <remarks>
        /// This mesh is used in the parent chunk to represent this hexagon in the chunk mesh.
        /// </remarks>
        public Mesh localMesh;

        //resources
        /// <summary>
        /// The current resource on this hexagon.
        /// </summary>
        /// <remarks>
        /// This holds a reference to the global resource. All changes to this <see cref="Resource"/> will be reflected in
        /// other hexagon sharing the resource.
        /// </remarks>
        [SerializeField]
        public Resource currentResource;
        internal ResourceManager resourceManager;
        /// <summary>
        /// The locations of each resource mesh.
        /// </summary>
        public List<Vector3> resourceLocations = new List<Vector3>();
        /// <summary>
        /// The GameObject that holds the resource meshes for this hexagon.
        /// </summary>
        public GameObject rObject;

        //improvments
        /// <summary>
        /// The current improvement on this hexagon.
        /// </summary>
        /// <remarks>
        /// This holds a reference to the global improvement. All changes to this <see cref="Improvement"/> will be reflected in
        /// other hexagon sharing the impovement.
        /// </remarks>
        [SerializeField]
        public Improvement currentImprovement;
        internal ImprovementManager improvementManager;
        /// <summary>
        /// The GameObject that holds the improvement meshes for this hexagon.
        /// </summary>
        public GameObject iObject;

        /// <summary>
        /// The coordinates of the hexagon in axial grid format.
        /// </summary>
        /// <remarks>
        /// This is simply a lighter version of <see cref="CubeGridPosition"/>, made possible by the fact, x + y + z = 0.
        /// With this equation we can include only the (x,y) cordinates and assume
        /// </remarks>
        public Vector2 AxialGridPosition
        {
            get { return new Vector2(CubeGridPosition.x, CubeGridPosition.y); }
        }

        /// <summary>
        /// The coordinates of the hexagon in cube grid format.
        /// </summary>
        /// <remarks>
        /// This is simply the complete version of the grid location. You can use <see cref="AxialGridPosition"/> and imply
        /// the "z" location with the rule of x + y + z = 0.
        /// </remarks>
        public Vector3 CubeGridPosition
        {
            get { return gridPosition; }
            set { gridPosition = value; }
        }

        /// <summary>
        /// This is the setup called from HexChunk when it's ready for us to generate our meshes.
        /// </summary>
        /// <example>
        /// The following code will start hex operations on a new hex provided that the hexagon has a valid parent chunk and world manager.
        /// <code>
        /// class HexTest : MonoBehaviour
        /// {
        ///     HexInfo hex;
        ///     
        ///     void Start()
        ///     {
        ///         hex = new HexInfo();
        ///         
        ///         hex.Start();
        ///     }
        /// }
        /// </code>
        /// </example>
        public void Start()
        {
            //set tile type to the mountain tile if the feature is a mountain and the type exists
            if (terrainFeature == Feature.Mountain) { Tile mountain = parentChunk.worldManager.tileManager.TryGetMountain(); if (mountain != null) {  terrainType = mountain; } }

            //get the texture atlas from world manager
            worldTextureAtlas = parentChunk.worldManager.textureAtlas;

            //generate local mesh
            MeshSetup();

            //if we are NOT loading a map
            if (parentChunk.worldManager.generateNewValues == true)
            {
                //check for resources and default to no improvement
                currentImprovement = improvementManager.improvements[0];
                resourceManager.CheckForResource(this);
            }
        }

        /// <summary>
        /// Applies any changes on this hex to it's parent chunk.
        /// </summary>
        /// <remarks>
        /// This method must be called to apply any changes to a hexagon's <see cref="HexInfo.localMesh"/>. Without calling
        /// this method the changes won't be seen in the chunk mesh.
        /// </remarks>
        /// <example>
        /// The following code changes the very middle hexagon in the map to show its resource texture.
        /// <code>
        /// using System;
        /// using UnityEngine;
        /// using CivGrid;
        ///
        /// class ExampleClass : MonoBehaviour
        /// {
        ///    WorldManager worldManager;
        ///
        ///    public void Start()
        ///    {
        ///        //cache and find the world manager
        ///        worldManager = GameObject.FindObjectOfType&lt;WorldManager&gt;();
        ///
        ///        //gets the very middle hexagon in the map
        ///        HexChunk chunk = worldManager.hexChunks[worldManager.hexChunks.GetLength(0) / 2, worldManager.hexChunks.GetLength(1) / 2];
        ///        HexInfo hex = chunk.hexArray[chunk.hexArray.GetLength(0) / 2, chunk.hexArray.GetLength(1) / 2];
        ///
        ///        //change the hexes texture to the resource version
        ///        hex.ChangeTextureToResource();
        ///
        ///        //update the chunk mesh to apply the changes
        ///        hex.ApplyChanges();
        ///    }
        /// }
        ///
        /// </code>
        /// </example>
        public void ApplyChanges()
        {
            parentChunk.RegenerateMesh();
        }

        /// <summary>
        /// Switches this hexagon's UV data to display it's resource texture.
        /// </summary>
        /// <example>
        /// The following code changes this hexagon to show its resource texture if a resource and a resource tile
        /// texture is present.
        /// <code>
        /// using System;
        /// using UnityEngine;
        /// using CivGrid;
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///   WorldManager worldManager;
        ///
        ///    void Start()
        ///    {
        ///        worldManager = GameObject.FindObjectOfType&lt;WorldManager&gt;();
        ///
        ///        worldManager.hexChunks[0, 0].hexArray[0, 0].ChangeTextureToResource();
        ///    }
        /// }
        /// </code>
        /// </example>
        public void ChangeTextureToResource()
        {
            //if the current resource contains a ground texture
            if (currentResource.replaceGroundTexture == true)
            {
                //assign flat UV data
                if (terrainFeature == Feature.Flat)
                {
                    worldTextureAtlas.resourceLocations.TryGetValue(currentResource, out currentRectLocation);
                    AssignUVToTile(currentRectLocation);
                }
                //assign feature UV data
                else if (terrainFeature == Feature.Hill || terrainFeature == Feature.Mountain)
                {
                    worldTextureAtlas.resourceLocations.TryGetValue(currentResource, out currentRectLocation);
                    AssignPresetUVToTile(localMesh, currentRectLocation);
                }

                //regenerate chunk to incorperate our changes
                parentChunk.RegenerateMesh();
            }
        }

        /// <summary>
        /// Switches this hexagon's UV data to display it's improvement texture.
        /// </summary>
        /// <example>
        /// The following code changes this hexagon to show its improvement texture if a improvement and a improvement tile texture
        /// is present.
        /// <code>
        /// using System;
        /// using UnityEngine;
        /// using CivGrid;
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///   WorldManager worldManager;
        ///
        ///    void Start()
        ///    {
        ///        worldManager = GameObject.FindObjectOfType&lt;WorldManager&gt;();
        ///
        ///        worldManager.hexChunks[0, 0].hexArray[0, 0].ChangeTextureToImprovement();
        ///    }
        /// }
        /// </code>
        /// </example>
        public void ChangeTextureToImprovement()
        {
            //if the current improvement contains a ground texture
            if (currentImprovement.replaceGroundTexture == true)
            {
                //assign flat UV data
                if (terrainFeature == Feature.Flat)
                {
                    worldTextureAtlas.improvementLocations.TryGetValue(currentImprovement, out currentRectLocation);
                    AssignUVToTile(currentRectLocation);
                }
                //assign feature UV data
                else if (terrainFeature == Feature.Hill || terrainFeature == Feature.Mountain)
                {
                    worldTextureAtlas.improvementLocations.TryGetValue(currentImprovement, out currentRectLocation);
                    AssignPresetUVToTile(localMesh, currentRectLocation);
                }

                //regenerate chunk to incorperate our changes
                parentChunk.RegenerateMesh();
            }
        }

        /// <summary>
        /// Switches this hexagon's UV data to display it's normal base texture.
        /// </summary>
        /// <example>
        /// The following code changes this hexagon to show its normal texture.
        /// <code>
        /// using System;
        /// using UnityEngine;
        /// using CivGrid;
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///   WorldManager worldManager;
        ///
        ///    void Start()
        ///    {
        ///        worldManager = GameObject.FindObjectOfType&lt;WorldManager&gt;();
        ///
        ///        worldManager.hexChunks[0, 0].hexArray[0, 0].ChangeTextureToNormalTile();
        ///    }
        /// }
        /// </code>
        /// </example>
        public void ChangeTextureToNormalTile()
        {
            //if we have a texture that is not the base tile texture; switch it
            if (currentImprovement.replaceGroundTexture == true || currentResource.replaceGroundTexture == true)
            {
                //assign flat UV data
                if (terrainFeature == Feature.Flat)
                {
                    AssignUVToDefaultTile();
                }
                //assign feature UV data
                else if (terrainFeature == Feature.Hill || terrainFeature == Feature.Mountain)
                {
                    AssignPresetUVToDefaultTile(baseFeatureUV);
                }

                //regenerate chunk to incorperate our changes
                parentChunk.RegenerateMesh();
            }
        }

        /// <summary>
        /// Generate a mesh, normals, and UV data according to the tile type.
        /// </summary>
        public void MeshSetup()
        {
            //create new mesh to start fresh
            localMesh = new Mesh();

            #region Flat
            //if we are generating a flat regular hexagon
            if (terrainFeature == Feature.Flat)
            {
                //pull mesh data from WorldManager
                localMesh.vertices = parentChunk.worldManager.flatHexagonSharedMesh.vertices;
                localMesh.triangles = parentChunk.worldManager.flatHexagonSharedMesh.triangles;
                localMesh.uv = parentChunk.worldManager.flatHexagonSharedMesh.uv;

                //recalculate normals to play nicely with lighting
                localMesh.RecalculateNormals();

                //assign tile texture
                AssignUVToDefaultTile();
            }
            #endregion
            #region Feature
            //if we are generating a hexagon with a feature
            else if (terrainFeature == Feature.Mountain || terrainFeature == Feature.Hill)
            {
                Vector3[] vertices;

                //pull base mountain height map
                Texture2D localMountainTexture = new Texture2D(parentChunk.worldManager.mountainMap.width, parentChunk.worldManager.mountainMap.height);
                
                //overlay the base mountain height map with random noise with intesity depending on wether the feature is a hill or mountain
                if (terrainFeature == Feature.Mountain)
                {
                    //large overlay
                    localMountainTexture = NoiseGenerator.RandomOverlay(parentChunk.worldManager.mountainMap, Random.Range(-100f, 100f), Random.Range(0.005f, 0.18f), Random.Range(0.2f, 0.5f), Random.Range(0.3f, 0.6f), 2, true, false);
                }
                else if (terrainFeature == Feature.Hill)
                {
                    //small overlay
                    localMountainTexture = NoiseGenerator.RandomOverlay(parentChunk.worldManager.mountainMap, Random.Range(-100f, 100f), Random.Range(0.005f, 0.18f), Random.Range(0.75f, 1f), Random.Range(0.4f, 0.7f), 2, true, true);
                }

                //feature values
                int width = Mathf.Min(localMountainTexture.width, 255);
                int height = Mathf.Min(localMountainTexture.height, 255);
                int y = 0;
                int x = 0;

                // Build vertices
                vertices = new Vector3[height * width];
                Vector4[] tangents = new Vector4[height * width];

                //scale values
                Vector2 uvScale = new Vector2(1.0f / width, 1.0f / height);
                Vector3 sizeScale = new Vector3(parentChunk.hexSize.x / (width * 0.9f), parentChunk.worldManager.mountainScaleY, parentChunk.hexSize.z / (height * 0.9f));
                
                //raw UV covering 1:1
                Vector2[] rawUV = new Vector2[height * width];

                //generates vertices for this hexagon
                for (y = 0; y < height; y++)
                {
                    for (x = 0; x < width; x++)
                    {
                        //grab pixel height data from the texture created and scale it.
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

                        //position vertex
                        Vector3 vertex = new Vector3(x, pixelHeight - (parentChunk.worldManager.mountainScaleY / 100), y);
                        //scale vertex
                        vertices[y * width + x] = Vector3.Scale(sizeScale, vertex);
                        //uv map the vertex
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
                //assign our base feature UV to the generated 1:1 data
                baseFeatureUV = rawUV;

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

                // Build triangle indices: 3 indices into vertex array for each triangle
                int[] triangles = new int[vertices.Length * 6];
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

                // And assign them to the mesh
                localMesh.triangles = triangles;

                // Auto-calculate vertex normals from the mesh
                localMesh.RecalculateNormals();

                // Assign tangents after recalculating normals
                localMesh.tangents = tangents;

                //assign new hex extenets according to the collider
                hexExt = localMesh.bounds.extents;

                //assign the tile texture
                AssignPresetUVToDefaultTile(rawUV);
            }
            #endregion
        }

        /// <summary>
        /// Assigns the flat hexagon's UV data to the tile type.
        /// </summary>
        private void AssignUVToDefaultTile()
        {
            Vector2[] UV;

            //the base 1:1 UV map
            Vector2[] rawUV = parentChunk.worldManager.flatHexagonSharedMesh.uv;

            //if we are NOT loading
            if (parentChunk.worldManager.generateNewValues == true)
            {
                //if defualtRect location is null
                if (defaultRectLocation == new Rect())
                {
                    //get the postion of the texture on the texture atlas
                    parentChunk.worldManager.textureAtlas.tileLocations.TryGetValue(terrainType, out currentRectLocation);
                    defaultRectLocation = currentRectLocation;
                }
                //use cached rect location
                else { currentRectLocation = defaultRectLocation; }
            }

            //temp UV data
            UV = new Vector2[rawUV.Length];

            //shift base 1:1 UV map to the scale and location of the texture on the texture atlas
            for (int i = 0; i < rawUV.Length; i++)
            {
                UV[i] = new Vector2(rawUV[i].x * currentRectLocation.width + currentRectLocation.x, rawUV[i].y * currentRectLocation.height + currentRectLocation.y);
            }

            //assign the created UV data
            localMesh.uv = UV;
        }

        /// <summary>
        /// Assigns the flat hexagon's UV data to provided location on the texture atlas.
        /// </summary>
        /// <param name="rectArea">The location of the texture in the texture atlas</param>
        private void AssignUVToTile(Rect rectArea)
        {
            Vector2[] UV;

            //the base 1:1 UV map
            Vector2[] rawUV = parentChunk.worldManager.flatHexagonSharedMesh.uv;

            //temp UV data
            UV = new Vector2[rawUV.Length];

            //shift base 1:1 UV map to the scale and location of the texture on the texture atlas
            for (int i = 0; i < rawUV.Length; i++)
            {
                UV[i] = new Vector2(rawUV[i].x * rectArea.width + rectArea.x, rawUV[i].y * rectArea.height + rectArea.y);
            }

            //assign the created UV data
            localMesh.uv = UV;
        }

        /// <summary>
        /// Assign the UV maps for a hexagon with a feature to the default base tile texture .
        /// </summary>
        /// <param name="rawUV">UV map locations for (0,0) sector of texture atlas</param>
        private void AssignPresetUVToDefaultTile(Vector2[] rawUV)
        {
            Vector2[] UV;

            //if we are NOT loading a map
            if (parentChunk.worldManager.generateNewValues == true)
            {
                //if defaultRectLocation is not null
                if (defaultRectLocation == new Rect())
                {
                    //get the postion of the texture on the texture atlas
                    parentChunk.worldManager.textureAtlas.tileLocations.TryGetValue(terrainType, out currentRectLocation);
                    //cache location
                    defaultRectLocation = currentRectLocation;
                }
                //use cached location
                else { currentRectLocation = defaultRectLocation; }
            }

            //temp UV data
            UV = new Vector2[localMesh.vertexCount];

            //shift base 1:1 UV map to the scale and location of the texture on the texture atlas
            for (int i = 0; i < localMesh.vertexCount; i++)
            {
                UV[i] = new Vector2(rawUV[i].x * currentRectLocation.width + currentRectLocation.x, rawUV[i].y * currentRectLocation.height + currentRectLocation.y);
            }

            //assign the created UV data
            localMesh.uv = UV;
        }

        /// <summary>
        /// Assign the UV maps for a hexagon with a feature to the provided location on the texture atlas.
        /// </summary>
        /// <param name="mesh">Mesh to edit UV data from</param>
        /// <param name="rectArea">Location of the texture on the texture atlas</param>
        private void AssignPresetUVToTile(Mesh mesh, Rect rectArea)
        {
            Vector2[] UV;

            //temp UV data
            UV = new Vector2[mesh.vertexCount];

            //shift base 1:1 UV map to the scale and location of the texture on the texture atlas
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                UV[i] = new Vector2(mesh.uv[i].x * rectArea.width + rectArea.x, mesh.uv[i].y * rectArea.height + rectArea.y);
            }

            //assign the created UV data
            localMesh.uv = UV;
        }
    }
}