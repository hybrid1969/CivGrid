using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CivGrid;

namespace CivGrid
{
    public class ResourceManager : MonoBehaviour
    {

        public List<Resource> resources;
        public string[] resourceNames;

        public bool hideResources;

        public void SetUp()
        {
            resources.Insert(0, new Resource("None", 0, null, null));
            resources[0].meshToSpawn = null;
            resources[0].spawnAmount = 0;

            if (resourceNames == null)
            {
                UpdateResourceNames();
            }
        }

        public void AddResource(Resource r)
        {
            resources.Add(r);
            UpdateResourceNames();
        }

        public void UpdateResourceNames()
        {
            resourceNames = new string[resources.Count];
            for (int i = 0; i < resources.Count; i++)
            {
                resourceNames[i] = resources[i].resourceName;
            }
        }

        public void DeleteResource(Resource r)
        {
            resources.Remove(r);
            UpdateResourceNames();
        }

        public void SpawnResource(HexInfo hex, Resource r, bool regenerateChunk)
        {
            float y = (hex.localMesh.bounds.extents.y); if (y == 0) { y -= ((hex.worldPosition.y + hex.localMesh.bounds.extents.y) / Random.Range(4, 8)); } else { y = hex.worldPosition.y + hex.localMesh.bounds.extents.y + hex.currentResource.meshToSpawn.bounds.extents.y; }
            for (int i = 0; i < r.spawnAmount; i++)
            {
                if (hex.currentResource.meshToSpawn == null && hex.currentResource.resourceName != "None") { Debug.LogWarning("No Mesh was assigned to spawn for resource: " + hex.currentResource.resourceName + ". Aborting spawning of graphics for this resource."); return; }

                hex.localMesh.RecalculateBounds();

                //position setting
                float x = (hex.localMesh.bounds.center.x + Random.Range(-0.2f, 0.2f));
                float z = (hex.localMesh.bounds.center.z + Random.Range(-0.2f, 0.2f));
                hex.resourceLocations.Add(new Vector3(x, y, z));
                hex.CombineWithOthers(hex.resourceLocations.Count, hex.resourceLocations.ToArray());
            }

            if (regenerateChunk)
            {
                hex.ChangeTextureToResource();
                hex.parentChunk.RegenerateMesh();
            }
        }

        /// <summary>
        /// Checks if a resource should be spawned on a hexagon
        /// </summary>
        /// <param name="hex">The hexagon to check</param>
        /// <param name="returnResource">The resource that is spawned on the hexagon</param>
        public void CheckForResource(HexInfo hex, out Resource returnResource)
        {
            //if (resources == null) { SetUp(); }
            bool possible = false;

            for(int i = 0; i < resources.Count; i++)
            {
                Resource r = resources[i];
                if (r.rule != null)
                {
                    if (Test(hex, r.rule))
                    {
                        int number = (int)Random.Range(0, r.rarity);
                        if (number == 0)
                        {
                            returnResource = r;
                            SpawnResource(hex, returnResource, false);
                            return;
                        }
                    }
                    else
                    {
                        possible = false;
                        continue;
                    }
                } 
                else
                {
                    if ((int)Random.Range(0, r.rarity) == 0 && possible)
                    {
                        returnResource = r;
                        SpawnResource(hex, returnResource, false);
                        return;
                    }
                }    
            }

            returnResource = resources[0];
        }

        private static bool Test(HexInfo hex, ResourceRules rule)
        {
            bool returnVal;

            for(int i =0; i < rule.possibleTiles.Length; i++)
            {
                returnVal = TestRule(hex, rule.possibleTiles[i]);
                if (returnVal == true) break;
                if(i == (rule.possibleTiles.Length-1)) { return false;}
            }
            for (int i = 0; i < rule.possibleFeatures.Length; i++)
            {
                returnVal = TestRule(hex, rule.possibleFeatures[i]);
                if(returnVal == true) return true;
                if (i == (rule.possibleFeatures.Length - 1)) { return false; }
            }
            return false;
        }

        private static bool TestRule(HexInfo hex, Tile tile)
        {
            if (hex.terrainType == tile) { return true; } else { return false; }
        }

        private static bool TestRule(HexInfo hex, Feature feature)
        {
            if (hex.terrainFeature == feature) { return true; } else { return false; }
        }

    }


    /// <summary>
    /// Resource
    /// </summary>
    [System.Serializable]
    public class Resource
    {
        public string resourceName;
        public ResourceRules rule;
        bool possible;
        public float rarity;

        public Vector2 atlasLocation;

        public Mesh meshToSpawn;
        public int spawnAmount = 3;

        public Resource(string name, float rarity, Mesh mesh, ResourceRules rule)
        {
            this.resourceName = name;
            this.rule = rule;
            this.rarity = rarity;
            this.meshToSpawn = mesh;
        }
    }

    [System.Serializable]
    public class ResourceRules
    {
        public Tile[] possibleTiles;
        public Feature[] possibleFeatures;

        public ResourceRules(Tile[] possibleTiles, Feature[] possibleFeatures)
        {
            this.possibleTiles = possibleTiles;
            this.possibleFeatures = possibleFeatures;
        }
    }
}