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
        public int xSize;
        public int ySize;
        public Vector3 hexSize;

        //set by world master
        public int xSector;
        public int ySector;
        public WorldManager worldManager;

        private MeshFilter filter;
        private new BoxCollider collider;

        //temp
        public Texture2D mountainTexture;
        public float maxHeight = 1;

        public void SetSize(int x, int y)
        {
            xSize = x;
            ySize = y;
        }

        public void OnDestroy()
        {
            Destroy(renderer.material);
        }

        public void AllocateHexArray()
        {
            hexArray = new HexInfo[xSize, ySize];
        }

        public void GenerateChunk()
        {
            mountainTexture = worldManager.mountainMap;

            bool odd;

            for (int y = 0; y < ySize; y++)
            {
                odd = (y % 2) == 0;
                if (odd == true)
                {
                    for (int x = 0; x < xSize; x++)
                    {
                        //cache and create hex hex
                        HexInfo hex;
                        Vector2 worldArrayPosition;
                        hexArray[x, y] = new HexInfo();
                        hex = hexArray[x, y];

                        //set world array position
                        worldArrayPosition.x = x + (xSize * xSector);
                        worldArrayPosition.y = y + (ySize * ySector);

                        //set pixel location
                        hex.pixelLocation = worldArrayPosition;

                        hex.CubeGridPosition = new Vector3(worldArrayPosition.x - Mathf.Round((worldArrayPosition.y / 2) + .1f), worldArrayPosition.y, -(worldArrayPosition.x - Mathf.Round((worldArrayPosition.y / 2) + .1f) + worldArrayPosition.y));
                        //set local position of hex; this is the hex cord postion local to the chunk
                        hex.localPosition = new Vector3(x * ((worldManager.hexExt.x * 2)), 0, (y * worldManager.hexExt.z) * 1.5f);
                        //set world position of hex; this is the hex cord postion local to the world
                        hex.worldPosition = new Vector3(hex.localPosition.x + (xSector * (xSize * hexSize.x)), hex.localPosition.y, hex.localPosition.z + ((ySector * (ySize * hexSize.z)) * (.75f)));

                        ///Set Hex values
                        hex.terrainType = (Tile)(worldManager.PickHex((int)worldArrayPosition.x, (int)worldArrayPosition.y));
                        hex.name = "HexTile " + hex.CubeGridPosition;
                        hex.terrainFeature = worldManager.PickFeature((int)worldArrayPosition.x, (int)worldArrayPosition.y, DetermineWorldEdge(hex, x, y));
                        hex.hexExt = worldManager.hexExt;
                        hex.hexCenter = worldManager.hexCenter;
                        hex.rM = worldManager.rM;
                    }
                }
                else
                {
                    if (worldManager.keepSymmetrical)
                    {

                        for (int x = 0; x < xSize - 1; x++)
                        {
                            //cache and create hex hex
                            HexInfo hex;
                            Vector2 worldArrayPosition;
                            hexArray[x, y] = new HexInfo();
                            hex = hexArray[x, y];

                            //set world array position
                            worldArrayPosition.x = x + (xSize * xSector);
                            worldArrayPosition.y = y + (ySize * ySector);

                            //set pixel location
                            hex.pixelLocation = worldArrayPosition;


                            hex.CubeGridPosition = new Vector3(worldArrayPosition.x - Mathf.Round((worldArrayPosition.y / 2) + .1f), worldArrayPosition.y, -(worldArrayPosition.x - Mathf.Round((worldArrayPosition.y / 2) + .1f) + worldArrayPosition.y));
                            //set local position of hex; this is the hex cord position local to the chunk
                            hex.localPosition = new Vector3((x * (worldManager.hexExt.x * 2) + worldManager.hexExt.x), 0, (y * worldManager.hexExt.z) * 1.5f);
                            //set world position of hex; this is the hex cord postion local to the world
                            hex.worldPosition = new Vector3(hex.localPosition.x + (xSector * (xSize * hexSize.x)), hex.localPosition.y, hex.localPosition.z + ((ySector * (ySize * hexSize.z)) * (.75f)));

                            ///Set Hex values
                            hex.terrainType = (Tile)(worldManager.PickHex((int)worldArrayPosition.x, (int)worldArrayPosition.y));
                            hex.name = "HexTile " + hex.CubeGridPosition;
                            hex.terrainFeature = worldManager.PickFeature((int)worldArrayPosition.x, (int)worldArrayPosition.y, DetermineWorldEdge(hex, x, y));
                            hex.hexExt = worldManager.hexExt;
                            hex.hexCenter = worldManager.hexCenter;
                            hex.rM = worldManager.rM;
                        }
                    }
                    else
                    {
                        for (int x = 0; x < xSize; x++)
                        {
                            //cache and create hex hex
                            HexInfo hex;
                            Vector2 worldArrayPosition;
                            hexArray[x, y] = new HexInfo();
                            hex = hexArray[x, y];

                            //set world array position for real texture positioning
                            worldArrayPosition.x = x + (xSize * xSector);
                            worldArrayPosition.y = y + (ySize * ySector);

                            //set pixel location
                            hex.pixelLocation = worldArrayPosition;

                            hex.CubeGridPosition = new Vector3(worldArrayPosition.x - Mathf.Round((worldArrayPosition.y / 2) + .1f), worldArrayPosition.y, -(worldArrayPosition.x - Mathf.Round((worldArrayPosition.y / 2) + .1f) + worldArrayPosition.y));
                            //set local position of hex; this is the hex cord postion local to the chunk
                            hex.localPosition = new Vector3((x * (worldManager.hexExt.x * 2) + worldManager.hexExt.x), 0, (y * worldManager.hexExt.z) * 1.5f);
                            //set world position of hex; this is the hex cord postion local to the world
                            hex.worldPosition = new Vector3(hex.localPosition.x + (xSector * (xSize * hexSize.x)), hex.localPosition.y, hex.localPosition.z + ((ySector * (ySize * hexSize.z)) * (.75f)));


                            //print(hex.CubeGridPosition + "is in chunk " + gameObject.name + " at local position " + hex.localPosition + " and at world position " + hex.worldPosition); 

                            ///Set Hex values
                            hex.terrainType = (Tile)(worldManager.PickHex((int)worldArrayPosition.x, (int)worldArrayPosition.y));
                            hex.name = "HexTile " + hex.CubeGridPosition;
                            hex.terrainFeature = worldManager.PickFeature((int)worldArrayPosition.x, (int)worldArrayPosition.y, DetermineWorldEdge(hex, x, y));
                            hex.hexExt = worldManager.hexExt;
                            hex.hexCenter = worldManager.hexCenter;
                            hex.rM = worldManager.rM;
                        }
                    }
                }
            }
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
            if (xSector == 0 || xSector == ((worldManager.mapSize.x / worldManager.chunkSize) - 1))
            {
                if (x == (xSize - 1) || x == 0)
                {
                    return true;
                }
            }

            if (ySector == 0 || ySector == ((worldManager.mapSize.y / worldManager.chunkSize) - 1))
            {
                if (y == (ySize - 1) || y == 0)
                {
                    return true;
                }
            }

            return false;
        }

        //begin hexagon rendering
        public void Begin()
        {
            GenerateChunk();
            for (int x = 0; x < xSize; x++)
            {
                for (int z = 0; z < ySize; z++)
                {
                    if (hexArray[x, z] != null)
                    {
                        hexArray[x, z].parentChunk = this;
                        hexArray[x, z].Start();
                    }
                    else
                    {
                        print("null hexagon found in memory");
                    }
                }
            }
            Combine();
        }

        public void RegenerateMesh()
        {
            Combine();
        }

        void MakeCollider()
        {
            if (collider == null)
            {
                collider = gameObject.AddComponent<BoxCollider>();
            }
            collider.center = filter.mesh.bounds.center;
            collider.size = filter.mesh.bounds.size;
        }

        private void Combine()
        {
            CombineInstance[,] combine = new CombineInstance[xSize, ySize];

            for (int x = 0; x < xSize; x++)
            {
                for (int z = 0; z < ySize; z++)
                {

                    combine[x, z].mesh = hexArray[x, z].localMesh;
                    Matrix4x4 matrix = new Matrix4x4();
                    if (hexArray[x, z].terrainFeature != Feature.Mountain)
                    {
                        matrix.SetTRS(hexArray[x, z].localPosition, Quaternion.identity, Vector3.one);
                    }
                    else if (hexArray[x, z].terrainFeature == Feature.Mountain)
                    {
                        matrix.SetTRS(new Vector3(hexArray[x, z].localPosition.x, hexArray[x, z].localPosition.y - 0.01f, hexArray[x, z].localPosition.z), Quaternion.identity, Vector3.one);
                    }
                    combine[x, z].transform = matrix;
                }
            }

            filter = gameObject.GetComponent<MeshFilter>();
            filter.mesh = new Mesh();

            CombineInstance[] final;

            CivGridUtility.ToSingleArray(combine, out final);

            filter.mesh.CombineMeshes(final);
            filter.mesh.RecalculateNormals();
            filter.mesh.RecalculateBounds();
            MakeCollider();
        }
    }
}