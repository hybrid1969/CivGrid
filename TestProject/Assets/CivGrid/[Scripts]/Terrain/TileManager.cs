using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CivGrid;

namespace CivGrid
{
    public class TileManager : MonoBehaviour
    {
        public List<Tile> tiles;
        private Tile[] internalTiles;
        public string[] tileNames;

        public void SetUp()
        {
            internalTiles = tiles.ToArray();
            UpdateTileNames();
        }


        public void UpdateTileNames()
        {
            if (tiles != null && tiles.Count > 0)
            {
                tileNames = new string[tiles.Count];
                for (int i = 0; i < tiles.Count; i++)
                {
                    tileNames[i] = tiles[i].name;
                }
            }
        }

        public void AddTile(Tile t)
        {
            tiles.Add(t);
            UpdateTileNames();
        }

        public void DeleteTile(Tile t)
        {
            tiles.Remove(t);
            UpdateTileNames();
        }

        public Tile TryGet(string name)
        {
            foreach (Tile t in internalTiles)
            {
                if (name == t.name)
                {
                    return t;
                }
            }
            return null;
        }

        public Tile TryGet(int index)
        {
            return internalTiles[index];
        }

        public Tile TryGetOcean()
        {
            foreach (Tile t in internalTiles)
            {
                if (t.name == "Ocean" && t.isOcean)
                {
                    return t;
                }
            }
            //only is we didnt find ocean
            foreach (Tile t in internalTiles)
            {
                if (t.isOcean)
                {
                    return t;
                }
            }
            return null;
        }

        public Tile TryGetMountain()
        {
            foreach (Tile t in internalTiles)
            {
                if (t.isMountain)
                {
                    return t;
                }
            }
            return null;
        }

        public Tile TryGetShore()
        {
            foreach (Tile t in internalTiles)
            {
                if (t.isShore)
                {
                    return t;
                }
            }
            return null;
        }

        public Tile GetTileFromLattitude(float lat)
        {
            for (int i = 0; i < internalTiles.Length; i++)
            {
                if ((tiles[i].isMountain == false && tiles[i].isOcean == false && tiles[i].isShore == false))
                {
                    if (lat < tiles[i].bottomLat) { continue; }
                    if (lat > tiles[i].topLat) { continue; }
                    else
                    {
                        return tiles[i];
                    }
                }
            }
            Debug.LogError("Couldn't find tile for this lattitude: " + lat);
            return null;
        }
    }

    [System.Serializable]
    public class Tile
    {
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