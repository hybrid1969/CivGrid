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
        public WorldManager worldManager;

        Vector2[] uv;

        public void SetUp()
        {
            worldManager = GetComponent<WorldManager>();

            resources.Insert(0, new Resource("None", 0, null, null, null));
            resources[0].meshToSpawn = null;
            resources[0].spawnAmount = 0;

            if (resourceNames == null)
            {
                UpdateResourceNames();
            }
        }

        public void UpdateResourceNames()
        {
            if (resources != null && resources.Count > 0)
            {
                resourceNames = new string[resources.Count];
                for (int i = 0; i < resources.Count; i++)
                {
                    resourceNames[i] = resources[i].name;
                }
            }
        }

        public void AddResource(Resource r)
        {
            resources.Add(r);
            UpdateResourceNames();
        }

        public void AddResourceAt(Resource r, int index)
        {
            resources.Insert(index, r);
            UpdateResourceNames();
        }

        public void DeleteResource(Resource r)
        {
            resources.Remove(r);
            UpdateResourceNames();
        }

        public void HideResourceMesh(HexInfo hex)
        {
            Destroy(hex.rObject);
        }

        public void SpawnResource(HexInfo hex, Resource r, bool regenerateChunk)
        {
            hex.resourceLocations.Clear();
            if (hex.rObject != null)
            {
                Destroy(hex.rObject);
            }

            float y = (hex.localMesh.bounds.extents.y); if (y == 0) { y -= ((hex.worldPosition.y + hex.localMesh.bounds.extents.y) / Random.Range(4, 8)); } else { y = hex.worldPosition.y + hex.localMesh.bounds.extents.y + hex.currentResource.meshToSpawn.bounds.extents.y; }
            for (int i = 0; i < r.spawnAmount; i++)
            {
                if (hex.currentResource.meshToSpawn == null && hex.currentResource.name != "None") { Debug.LogWarning("No Mesh was assigned to spawn for resource: " + hex.currentResource.name + ". Aborting spawning of graphics for this resource."); return; }

                hex.localMesh.RecalculateBounds();

                //position setting
                float x = (hex.localMesh.bounds.center.x + Random.Range(-0.2f, 0.2f));
                float z = (hex.localMesh.bounds.center.z + Random.Range(-0.2f, 0.2f));
                hex.resourceLocations.Add(new Vector3(x, y, z));
            }

            int size = hex.resourceLocations.Count;

            //number of resources to combine
            if (size > 0)
            {
                //combine instances
                CombineInstance[] combine = new CombineInstance[size];
                Matrix4x4 matrix = new Matrix4x4();
                matrix.SetTRS(Vector3.zero, Quaternion.identity, Vector3.one);

                //skip first combine instance due to presetting
                for (int k = 0; k < size; k++)
                {
                    combine[k].mesh = hex.currentResource.meshToSpawn;
                    matrix.SetTRS(hex.resourceLocations[k], Quaternion.identity, Vector3.one);
                    combine[k].transform = matrix;
                }

                GameObject holder = new GameObject(r.name, typeof(MeshFilter), typeof(MeshRenderer));

                holder.transform.position = hex.worldPosition;
                holder.transform.parent = hex.parentChunk.transform;

                MeshFilter filter = holder.GetComponent<MeshFilter>();

                holder.renderer.material.mainTexture = r.resourceMeshTexture;

                filter.mesh = new Mesh();
                filter.mesh.CombineMeshes(combine);

                hex.rObject = holder;

                //UV mapping
                Rect rectArea;
                worldManager.textureAtlas.resourceLocations.TryGetValue(r, out rectArea);
                uv = new Vector2[filter.mesh.vertexCount];

                for (int i = 0; i < filter.mesh.vertexCount; i++)
                {
                    uv[i] = new Vector2(filter.mesh.uv[i].x * rectArea.width + rectArea.x, filter.mesh.uv[i].y * rectArea.height + rectArea.y);

                    uv[i] = new Vector2(Mathf.Clamp(uv[i].x, 0.1f, 0.9f), Mathf.Clamp(uv[i].y, 0.1f, 0.9f));
                }

                filter.mesh.uv = uv;
            }

            if (regenerateChunk)
            {
                hex.ChangeTextureToResource();
                hex.parentChunk.RegenerateMesh();
            }
        }

        public void InitResourceTexturesOnHexs()
        {
            foreach (HexChunk chunk in worldManager.hexChunks)
            {
                foreach (HexInfo hex in chunk.hexArray)
                {
                    if (hex.currentResource.name != "None")
                    {
                        hex.ChangeTextureToResource();
                    }
                }
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

        private static bool Test(HexInfo hex, ResourceRule rule)
        {
            bool returnVal;
            TileManager tM = hex.parentChunk.worldManager.tileManager;

            for(int i =0; i < rule.possibleTiles.Length; i++)
            {
                returnVal = TestRule(hex, tM.tiles[rule.possibleTiles[i]]);
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
        public string name;
        public ResourceRule rule;
        bool possible;
        public float rarity;

        public Vector2 atlasLocation;

        public Mesh meshToSpawn;
        public Texture2D resourceMeshTexture;
        public int spawnAmount = 3;

        public Resource(string name, float rarity, Mesh mesh, Texture2D resourceMeshTexture, ResourceRule rule)
        {
            this.name = name;
            this.rule = rule;
            this.rarity = rarity;
            this.meshToSpawn = mesh;
            this.resourceMeshTexture = resourceMeshTexture;
        }
    }

    [System.Serializable]
    public class ResourceRule
    {
        public int[] possibleTiles;
        public Feature[] possibleFeatures;

        public ResourceRule(int[] possibleTiles, Feature[] possibleFeatures)
        {
            this.possibleTiles = possibleTiles;
            this.possibleFeatures = possibleFeatures;
        }
    }
}