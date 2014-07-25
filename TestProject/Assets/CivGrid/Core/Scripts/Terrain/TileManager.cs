using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CivGrid;

namespace CivGrid
{
    /// <summary>
    /// Contains all possible tiles.
    /// </summary>
    public class TileManager : MonoBehaviour
    {
        //tiles
        public List<Tile> tiles;
        public string[] tileNames;

        //internal array for speed
        private Tile[] internalTiles;

        /// <summary>
        /// Sets up the tile manager.
        /// Caches all needed values.
        /// </summary>
        public void SetUp()
        {
            internalTiles = tiles.ToArray();
            UpdateTileNames();
        }

        /// <summary>
        /// Creates an array of tile names.
        /// </summary>
        public void UpdateTileNames()
        {
            //only update if there are tiles
            if (internalTiles != null && internalTiles.Length > 0)
            {
                //instantiate tile names array
                tileNames = new string[internalTiles.Length];

                //loop through all tiles
                for (int i = 0; i < internalTiles.Length; i++)
                {
                    //asign each name into the array
                    tileNames[i] = internalTiles[i].name;
                }
            }
        }

        /// <summary>
        /// Adds a tile to the tile array.
        /// </summary>
        /// <param name="t">Tile to add</param>
        public void AddTile(Tile t)
        {
            tiles.Add(t);
            internalTiles = tiles.ToArray();
            UpdateTileNames();
        }

        /// <summary>
        /// Removes a tile from the tile array.
        /// </summary>
        /// <param name="t">Improvement to remove</param>
        public void DeleteTile(Tile t)
        {
            tiles.Remove(t);
            internalTiles = tiles.ToArray();
            UpdateTileNames();
        }

        /// <summary>
        /// Attempts to return a tile from a provided name.
        /// </summary>
        /// <param name="name">The name of the tile to look for</param>
        /// <returns>The tile with the name provided; null if not found</returns>
        public Tile TryGetTile(string name)
        {
            //cycle through all tiles
            foreach (Tile t in internalTiles)
            {
                //if the improvement shares the name; return it
                if (name == t.name)
                {
                    return t;
                }
            }
            //not found; return null
            return null;
        }

        /// <summary>
        /// Wrapper method of TryGetTile() to find a tile marked as an ocean.
        /// </summary>
        /// <returns>The tile found as ocean; null if not found</returns>
        public Tile TryGetOcean()
        {
            //loop through all tiles and find one marked as the ocean tile
            foreach (Tile t in internalTiles)
            {
                if (t.isOcean)
                {
                    return t;
                }
            }
            //not found
            return null;
        }

        /// <summary>
        /// Wrapper method of TryGetTile() to find a tile marked as a mountain.
        /// </summary>
        /// <returns>The tile found as mountain; null if not found</returns>
        public Tile TryGetMountain()
        {
            //loop through all tiles and find one marked as the mountain tile
            foreach (Tile t in internalTiles)
            {
                if (t.isMountain)
                {
                    return t;
                }
            }
            //not found
            return null;
        }

        /// <summary>
        /// Wrapper method of TryGetTile() to find a tile marked as a shore.
        /// </summary>
        /// <returns></returns>
        public Tile TryGetShore()
        {
            //loop through all tiles and find one marked as a shore tile
            foreach (Tile t in internalTiles)
            {
                if (t.isShore)
                {
                    return t;
                }
            }
            //not found
            return null;
        }

        /// <summary>
        /// Finds a tile for a provided lattitude.
        /// </summary>
        /// <param name="lat"></param>
        /// <returns></returns>
        public Tile GetTileFromLattitude(float lat)
        {
            //loop through all tiles
            for (int i = 0; i < internalTiles.Length; i++)
            {
                //if its not a special tile
                if ((internalTiles[i].isMountain == false && internalTiles[i].isOcean == false && internalTiles[i].isShore == false))
                {
                    //if it doesnt not fit within the lattitude clamp continue to the next tile, otherwise return this tile
                    if (lat < internalTiles[i].bottomLat) { continue; }
                    if (lat > internalTiles[i].topLat) { continue; }
                    else
                    {
                        return internalTiles[i];
                    }
                }
            }
            //tile not found
            Debug.LogError("Couldn't find tile for this lattitude: " + lat);
            return null;
        }
    }

    /// <summary>
    /// Tile class that contains all values for the base tile
    /// </summary>
    [System.Serializable]
    public class Tile
    {
        //tiles values
        public string name = "None";
        public float bottomLat;
        public float topLat;
        public bool isOcean;
        public bool isShore;
        public bool isMountain;

        public Tile(string name, float bottomLat, float topLat)
        {
            this.name = name;
            this.bottomLat = bottomLat;
            this.topLat = topLat;
        }

        public Tile(string name, bool isShore, bool isOcean, bool isMountain, float bottomLat, float topLat)
        {
            this.name = name;
            this.isShore = isShore;
            this.isOcean = isOcean;
            this.isMountain = isMountain;
            this.bottomLat = bottomLat;
            this.topLat = topLat;
        }

        public Tile(string name, bool isShore, bool isOcean, bool isMountain)
        {
            this.name = name;
            this.isShore = isShore;
            this.isOcean = isOcean;
            this.isMountain = isMountain;
        }
    }
}