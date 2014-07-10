using System;
using System.Collections;
using UnityEngine;
using CivGrid;

namespace CivGrid
{
    public class HexChunk : MonoBehaviour
    {

        [SerializeField]
        public HexInfo[,] hexArray;
        public Vector2 chunkSize;
        public Vector3 hexSize;

        //set by world master
        public int xSector;
        public int ySector;
        public WorldManager worldManager;

        private MeshFilter filter;
        private new BoxCollider collider;

        //temp
        public Texture2D mountainTexture;
        public float maxHeight = 1.5f;

        /// <summary>
        /// Sets the amount of hexagons in the chunk
        /// </summary>
        /// <param name="x">Amount of hexagons in "x" axis</param>
        /// <param name="y">Amount of hexagons in "y" axis</param>
        public void SetSize(int x, int y)
        {
            chunkSize = new Vector2(x, y);
        }

        /// <summary>
        /// Cleans up the material on this object after it is destroyed
        /// </summary>
        public void OnDestroy()
        {
            Destroy(renderer.material);
        }

        /// <summary>
        /// Allocates the hex array of this chunk
        /// </summary>
        public void AllocateHexArray()
        {
            hexArray = new HexInfo[(int)chunkSize.x, (int)chunkSize.y];
        }

        /// <summary>
        /// Generates all hexagons in this chunk in their location
        /// </summary>
        public void GenerateChunk()
        {
            //create the hexagons in memory
            hexArray = new HexInfo[(int)chunkSize.x, (int)chunkSize.y];

            //if this is an offset row
            bool even;

            //cycle through all hexagons in this chunk in the "y" axis
            for (int y = 0; y < chunkSize.y; y++)
            {
                //determine if we are in an odd row; if so we need to offset the hexagons
                even = ((y % 2) == 0);
                //actually not 
                if (even == true)
                {
                    //generate the hexagons in the normal positioning for this row
                    for (int x = 0; x < chunkSize.x; x++)
                    {
                        //spawn hexagon
                        GenerateHex(x, y);
                    }
                }
                else
                {
                    //generate the hexagons in the offset positioning for this row
                    for (int x = 0; x < chunkSize.x; x++)
                    {
                        //spawn offset hexagon
                        GenerateHexOffset(x, y);
                    }
                }
            }
        }

        /// <summary>
        /// Generates the hex in the provided array location
        /// </summary>
        /// <param name="x">Array location of hex in x axis</param>
        /// <param name="y">Array location of hex in y axis</param>
        private void GenerateHex(int x, int y)
        {
            //cache and create hex hex
            HexInfo hex;
            Vector2 worldArrayPosition;
            hexArray[x, y] = new HexInfo();
            hex = hexArray[x, y];

            //set world array position
            worldArrayPosition.x = x + (chunkSize.x * xSector);
            worldArrayPosition.y = y + (chunkSize.y * ySector);

            hex.CubeGridPosition = new Vector3(worldArrayPosition.x - Mathf.Round((worldArrayPosition.y / 2) + .1f), worldArrayPosition.y, -(worldArrayPosition.x - Mathf.Round((worldArrayPosition.y / 2) + .1f) + worldArrayPosition.y));
            //set local position of hex; this is the hex cord position local to the chunk
            hex.localPosition = new Vector3(x * ((worldManager.hexExt.x * 2)), 0, (y * worldManager.hexExt.z) * 1.5f);
            //set world position of hex; this is the hex cord position local to the world
            hex.worldPosition = new Vector3(hex.localPosition.x + (xSector * (chunkSize.x * hexSize.x)), hex.localPosition.y, hex.localPosition.z + ((ySector * (chunkSize.y * hexSize.z)) * (.75f)));

            ///Set Hex values
            hex.terrainFeature = worldManager.PickFeatureType((int)worldArrayPosition.x, (int)worldArrayPosition.y, DetermineWorldEdge(hex, x, y));
            hex.terrainType = worldManager.PickHexType((int)worldArrayPosition.x, (int)worldArrayPosition.y);
            hex.hexExt = worldManager.hexExt;
            hex.hexCenter = worldManager.hexCenter;
            hex.resourceManager = worldManager.resourceManager;
            hex.improvementManager = worldManager.improvementManager;
        }

        /// <summary>
        /// Generates the offset hex in the provided array location
        /// </summary>
        /// <param name="x">Array location of hex in x axis</param>
        /// <param name="y">Array location of hex in y axis</param>
        private void GenerateHexOffset(int x, int y)
        {
            //cache and create hex hex
            HexInfo hex;
            Vector2 worldArrayPosition;
            hexArray[x, y] = new HexInfo();
            hex = hexArray[x, y];

            //set world array position
            worldArrayPosition.x = x + (chunkSize.x * xSector);
            worldArrayPosition.y = y + (chunkSize.y * ySector);

            hex.CubeGridPosition = new Vector3(worldArrayPosition.x - Mathf.Round((worldArrayPosition.y / 2) + .1f), worldArrayPosition.y, -(worldArrayPosition.x - Mathf.Round((worldArrayPosition.y / 2) + .1f) + worldArrayPosition.y));
            //set local position of hex; this is the hex cord position local to the chunk
            hex.localPosition = new Vector3((x * (worldManager.hexExt.x * 2) + worldManager.hexExt.x), 0, (y * worldManager.hexExt.z) * 1.5f);
            //set world position of hex; this is the hex cord position local to the world
            hex.worldPosition = new Vector3(hex.localPosition.x + (xSector * (chunkSize.x * hexSize.x)), hex.localPosition.y, hex.localPosition.z + ((ySector * (chunkSize.y * hexSize.z)) * (.75f)));

            ///Set Hex values
            hex.terrainFeature = worldManager.PickFeatureType((int)worldArrayPosition.x, (int)worldArrayPosition.y, DetermineWorldEdge(hex, x, y));
            hex.terrainType = worldManager.PickHexType((int)worldArrayPosition.x, (int)worldArrayPosition.y);
            hex.hexExt = worldManager.hexExt;
            hex.hexCenter = worldManager.hexCenter;
            hex.resourceManager = worldManager.resourceManager;
            hex.improvementManager = worldManager.improvementManager;
        }

        /// <summary>
        /// Determine if this hexagon is on the world edge
        /// </summary>
        /// <param name="hex">Hexagon to check</param>
        /// <param name="x">Width location of hex within chunk</param>
        /// <param name="y">Height location of hex within chunk</param>
        /// <returns>If the hex is on the world edge</returns>
        private bool DetermineWorldEdge(HexInfo hex, int x, int y)
        {
            //check if hex is in a chunk on the horizontal sides
            if (xSector == 0 || xSector == ((worldManager.mapSize.x / worldManager.chunkSize) - 1))
            {
                //checks if hex is on the horizontal edge of a horizontal edge chunk
                if (x == (chunkSize.x - 1) || x == 0)
                {
                    return true;
                }
            }

            //check if hex is in a chunk on the vertical sides
            if (ySector == 0 || ySector == ((worldManager.mapSize.y / worldManager.chunkSize) - 1))
            {
                //checks if hex is on the vettical edge of a vertical edge chunk
                if (y == (chunkSize.y - 1) || y == 0)
                {
                    return true;
                }
            }

            //not an edge hex
            return false;
        }

        /// <summary>
        /// Starts chunk operations of spawning the hexagons and then chunking them
        /// </summary>
        public void Begin()
        {
            //begin making hexagons
            GenerateChunk();

            //cycle through all hexagons
            for (int x = 0; x < chunkSize.x; x++)
            {
                for (int z = 0; z < chunkSize.y; z++)
                {
                    //check if this hexagon is null; if so throw an error
                    if (hexArray[x, z] != null)
                    {
                        //set parent chunk of the hex to this
                        hexArray[x, z].parentChunk = this;
                        //start hex operations(pulling down the mesh)
                        hexArray[x, z].Start();
                    }
                    else
                    {
                        //throw error if the hexagon is null in memory
                        Debug.LogError("null hexagon found in memory: " + x + " " + z);
                    }
                }
            }

            //combine all the hexagon's meshes in this chunk into one mesh
            RegenerateMesh();
        }

        /// <summary>
        /// Adds a collider if there is none; resizes to meshes' size
        /// </summary>
        void MakeCollider()
        {
            //if we dont have a collider, add one
            if (collider == null)
            {
                collider = gameObject.AddComponent<BoxCollider>();
            }
            //set the collider's center and size to our mesh's size/center
            collider.center = filter.mesh.bounds.center;
            collider.size = filter.mesh.bounds.size;
        }

        /// <summary>
        /// Combines all localMeshes' in hexs of this chunk into one mesh
        /// </summary>
        public void RegenerateMesh()
        {
            //make a two-dimensional array to remain constant with our hexArray
            CombineInstance[,] combine = new CombineInstance[(int)chunkSize.x, (int)chunkSize.y];

            //cycle through all the hexagons in this chunk
            for (int x = 0; x < chunkSize.x; x++)
            {
                for (int z = 0; z < chunkSize.y; z++)
                {
                    //set the CombineInstance's mesh to this hexagon's localMesh
                    combine[x, z].mesh = hexArray[x, z].localMesh;
                    //create a Matrix4x4 for the meshes positioning
                    Matrix4x4 matrix = new Matrix4x4();
                    //set the matrix position, rotation, and scale to correct values
                    matrix.SetTRS(hexArray[x, z].localPosition, Quaternion.identity, Vector3.one);
                    //assign the CombineInstance's transform to the matrix; therefore correctly positioning it
                    combine[x, z].transform = matrix;
                }
            }

            //get the filter on the chunk gameObject
            filter = gameObject.GetComponent<MeshFilter>();
            //create a new mesh on it
            filter.mesh = new Mesh();

            //convert our two-dimensional array into a normal array so that we can use mesh.CombineMeshes()
            CombineInstance[] final;

            //conver to single array
            CivGridUtility.ToSingleArray<CombineInstance>(combine, out final);

            //set the chunk's mesh to the combined mesh of all the hexagon's in this chunk
            filter.mesh.CombineMeshes(final);
            //recalculate the normals of the new mesh to play nicely with lighting
            filter.mesh.RecalculateNormals();
            //generate/set the collider dimensions for this chunk
            MakeCollider();
        }
    }
}