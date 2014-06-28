using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CivGrid;

namespace CivGrid
{
    public class TileManager : MonoBehaviour
    {
        public List<Tile> tiles;
        public string[] tileNames;
        [HideInInspector]
        public WorldManager worldManager;

        public void SetUp()
        {
            worldManager = GetComponent<WorldManager>();

            tiles.Insert(0, new Tile("None", 0, 0));

            UpdateTileNames();
        }

        public void UpdateTileNames()
        {
            tileNames = new string[tiles.Count];
            for (int i = 0; i < tiles.Count; i++)
            {
                tileNames[i] = tiles[i].name;
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
            foreach (Tile t in tiles)
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
            return tiles[index];
        }

        public Tile GetOcean()
        {
            foreach (Tile t in tiles)
            {
                if (t.name == "Ocean" && t.isWater)
                {
                    return t;
                }
            }
            //only is we didnt find ocean
            foreach (Tile t in tiles)
            {
                if (t.isWater)
                {
                    return t;
                }
            }
            return null;
        }

        public Tile GetTileFromLattitude(float lat)
        {
            for(int i = 0; i < tiles.Count; i++)
            {
                if (lat < tiles[i].bottomLat) {continue; }
                if (lat > tiles[i].topLat) {continue; }
                else
                {
                    return tiles[i];
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
        public bool isWater;

        public Tile(string name, float bottomLat, float topLat)
        {
            this.name = name;
            this.bottomLat = bottomLat;
            this.topLat = topLat;
        }

        public Tile(string name, float bottomLat, float topLat, bool isWater)
        {
            this.name = name;
            this.bottomLat = bottomLat;
            this.topLat = topLat;
            this.isWater = isWater;
        }
    }
}