using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CivGrid;
using CivGrid.SampleResources;

namespace CivGrid
{
    internal enum EdgeType
    {
        WaterEdge,
        LandEdge,
        None
    }

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
        /// The position of the hexagon in the parent chunk array.
        /// </summary>
        public Vector2 chunkArrayPosition;

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
        /// If this hexagon is on the edge of the parent chunk
        /// </summary>
        public bool onEdge;

        /// <summary>
        /// Bordering hexagons of this hexagon.
        /// </summary>
        public HexInfo[] neighbors;

        /// <summary>
        /// The coordinates of the hexagon in axial coordinates.
        /// </summary>
        /// <remarks>
        /// This is simply a lighter version of <see cref="CubeCoordinates"/>, made possible by the fact, x + y + z = 0.
        /// With this equation we can include only the (x,y) cordinates and assume
        /// </remarks>
        public Vector2 AxialCoordinates
        {
            get { return new Vector2(CubeCoordinates.x, CubeCoordinates.y); }
        }

        /// <summary>
        /// The coordinates of the hexagon in cube coordinates.
        /// </summary>
        /// <remarks>
        /// This is simply the complete version of the grid location. You can use <see cref="AxialCoordinates"/> and imply
        /// the "z" location with the rule of x + y + z = 0.
        /// </remarks>
        public Vector3 CubeCoordinates
        {
            get { return gridPosition; }
            set { gridPosition = value; }
        }

        /// <summary>
        /// The coordinates of the hexagon in odd-r offset coordinates.
        /// </summary>
        /// <remarks>
        /// This is a different format of coordinates. Safe to use, however is not the most efficient method. <see cref="CubeCoordinates"/> is used internally
        /// and all other formats are converted on demand.
        /// </remarks>
        public Vector3 OffsetCoordinates
        {
            get { return new Vector3(CubeCoordinates.x + (CubeCoordinates.z - ((int)CubeCoordinates.z&1)) /2, CubeCoordinates.z);}
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

            parentChunk.worldManager.axialToHexDictionary.Add(AxialCoordinates, this);
            Debug.Log(AxialCoordinates);

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
                    AssignUVToDefaultTile();
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
                    AssignUVToDefaultTile();
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
                    AssignUVToDefaultTile();
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

            //cache neighbors of this hexagon
            neighbors = parentChunk.worldManager.GetNeighboursOfHex(this);
            Debug.Log("My Axial Position: " + AxialCoordinates);
            for (int i = 0; i < 6; i++)
            {
                if (neighbors[i] != null)
                {
                    Debug.Log("Neighbor: " + neighbors[i].AxialCoordinates);
                }
            }

                //create new mesh to start fresh
                localMesh = new Mesh();

            //if we are generating a flat regular hexagon
            if (parentChunk.worldManager.levelOfDetail == 0 || terrainType.isShore || terrainType.isOcean)
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
            else
            {
                Texture2D localMountainTexture;

                localMesh.vertices = parentChunk.worldManager.flatHexagonSharedMesh.vertices;
                localMesh.triangles = parentChunk.worldManager.flatHexagonSharedMesh.triangles;
                localMesh.uv = parentChunk.worldManager.flatHexagonSharedMesh.uv;

                if (terrainFeature == Feature.Mountain)
                {
                    localMountainTexture = NoiseGenerator.RandomOverlay(parentChunk.worldManager.mountainHeightMap, Random.Range(-100f, 100f), Random.Range(0.005f, 0.18f), Random.Range(0.2f, 0.5f), Random.Range(0.3f, 0.6f), 2, true, false);
                }
                else if (terrainFeature == Feature.Hill)
                {
                    localMountainTexture = NoiseGenerator.RandomOverlay(parentChunk.worldManager.mountainHeightMap, Random.Range(-100f, 100f), Random.Range(0.005f, 0.18f), Random.Range(0.75f, 1f), Random.Range(0.4f, 0.7f), 2, true, false);
                }
                else
                {
                    localMountainTexture = parentChunk.flatHeightMap;
                }

                Vector3[] vertices = localMesh.vertices;
                for (int i = 0; i < vertices.Length; i++)
                {
                    if(VertexIsEdge(i))
                    {
                        if (onEdge == false)
                        {
                            Edge edge = GetEdgeFromVertex(i);
                            edge.adjacentHex = GetAdjacentHexFromEdgeDirection(edge.direction);
                        }

                        //if (edge.adjacentHex.terrainType.isOcean == true || edge.adjacentHex.terrainType.isShore == true)
                        {
                            vertices[i].Set(vertices[i].x, 0f, vertices[i].z);
                        }
                        //else
                        //{
                        //    vertices[i].Set(vertices[i].x, 0.1f, vertices[i].z);
                       // }
                    }
                    else
                    {
                        float pixelHeight = localMountainTexture.GetPixelBilinear(localMesh.uv[i].x, localMesh.uv[i].y).grayscale;
                        if (terrainFeature == Feature.Mountain) { pixelHeight *= 1.5f; vertices[i].Set(vertices[i].x, vertices[i].y + (pixelHeight - (parentChunk.worldManager.mountainScaleY / 100)), vertices[i].z); }
                        if (terrainFeature == Feature.Hill) { vertices[i].Set(vertices[i].x, vertices[i].y + (pixelHeight - (parentChunk.worldManager.mountainScaleY / 100)), vertices[i].z); }
                        //flat
                        else
                        {
                            pixelHeight = localMountainTexture.GetPixelBilinear(localMesh.uv[i].x + (chunkArrayPosition.x * localMesh.uv[i].x), localMesh.uv[i].y + (chunkArrayPosition.y * localMesh.uv[i].y)).grayscale;
                            vertices[i].Set(vertices[i].x, vertices[i].y + pixelHeight / 10, vertices[i].z);
                        }
                    }
                    //EdgeType edgeType = GetVertexEdgeType(i);
                    ////not an edge
                    //if (edgeType == EdgeType.None)
                    //{
                    //    float pixelHeight = localMountainTexture.GetPixelBilinear(localMesh.uv[i].x, localMesh.uv[i].y).grayscale;
                    //    if (terrainFeature == Feature.Mountain) { pixelHeight *= 1.5f; vertices[i].Set(vertices[i].x, vertices[i].y + (pixelHeight - (parentChunk.worldManager.mountainScaleY / 100)), vertices[i].z); }
                    //    if (terrainFeature == Feature.Hill) { vertices[i].Set(vertices[i].x, vertices[i].y + (pixelHeight - (parentChunk.worldManager.mountainScaleY / 100)), vertices[i].z); }
                    //    //flat
                    //    else
                    //    {
                    //        pixelHeight = localMountainTexture.GetPixelBilinear(localMesh.uv[i].x + (chunkArrayPosition.x * localMesh.uv[i].x), localMesh.uv[i].y + (chunkArrayPosition.y * localMesh.uv[i].y)).grayscale;
                    //        vertices[i].Set(vertices[i].x, vertices[i].y + pixelHeight/10, vertices[i].z);
                    //    }
                    //}
                    ////hex next to this edge is a land tile
                    //else if(edgeType == EdgeType.LandEdge)
                    //{
                    //    Debug.Log("land edge");
                    //    vertices[i].Set(vertices[i].x, 0.1f, vertices[i].z);
                    //}
                    ////hex next to this is a water tile do nothing
                    //else if(edgeType == EdgeType.WaterEdge)
                    //{
                    //    Debug.Log("water edge");
                    //    vertices[i].Set(vertices[i].x, 0.1f, vertices[i].z);
                    //}
                    //else
                    //{
                    //    Debug.LogError("Logic Error; Unreachable code reached.");
                    //}
                }
                localMesh.vertices = vertices;


                //recalculate normals to play nicely with lighting
                localMesh.RecalculateNormals();

                //assign tile texture
                AssignUVToDefaultTile();
            }

        }

        private bool VertexIsEdge(int vertexIndex)
        {
            for (int i = 0; i < parentChunk.worldManager.edgeVertices.Length; i++)
            {
                if (parentChunk.worldManager.edgeVertices[i] == vertexIndex) { return true; }
            }
            return false;
        }

        //private EdgeType GetVertexEdgeType(int vertexIndex)
        //{
        //    Edge edge = GetEdgeFromVertex(vertexIndex);
        //    if (edge != null)
        //    {
        //        edge.adjacentHex = GetHexFromEdgeDirection(this, edge.direction);

        //        if (edge.adjacentHex != null)
        //        {
        //            Debug.Log("found adjacent hex");
        //            if (edge.adjacentHex.terrainType.isOcean == true || edge.adjacentHex.terrainType.isShore == true) { Debug.Log("water edge"); return EdgeType.WaterEdge; }
        //            else { Debug.Log("land edge"); return EdgeType.LandEdge; }
        //        }
        //    }
        //    return EdgeType.None;
        //}

        private Edge GetEdgeFromVertex(int index)
        {
            for (int i = 0; i < parentChunk.worldManager.edges.Length; i++)
            {
                if (parentChunk.worldManager.edges[i].vertexIndex[0] == index) { return parentChunk.worldManager.edges[i]; }
                if (parentChunk.worldManager.edges[i].vertexIndex[1] == index) { return parentChunk.worldManager.edges[i]; }
            }
            return null;
        }

        private HexInfo GetAdjacentHexFromEdgeDirection(Vector2 direction)
        {
            if (onEdge)
            {
                return null;
            }
            else
            {
                return parentChunk.worldManager.GetHexFromAxialCoordinates(new Vector2(AxialCoordinates.x + direction.x, AxialCoordinates.y + direction.y));
            }
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
    }
}