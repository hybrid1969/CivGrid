using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CivGrid;

namespace CivGrid
{

    /// <summary>
    /// Contains all possible resources.
    /// Handles the addition and removal of these resources upon hexagons.
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        //resources
        public List<Resource> resources;
        public string[] resourceNames;

        //cached managers
        public WorldManager worldManager;
        public TileManager tileManager;

        /// <summary>
        /// Sets up the resource manager.
        /// Caches all needed values.
        /// </summary>
        public void SetUp()
        {
            //insert default "None" resource into the resource array
            resources.Insert(0, new Resource("None", 0, 0, null, null, false, null));

            //cache managers
            tileManager = GetComponent<TileManager>();
            worldManager = GetComponent<WorldManager>();

            //instatiate the improvement name array
            if (resourceNames == null)
            {
                UpdateResourceNames();
            }
        }

        /// <summary>
        /// Called on start-up to make sure all hexs with resources are changed to use their resource texture.
        /// </summary>
        public void InitiateResourceTexturesOnHexs()
        {
            //loop through all hexs
            foreach (HexChunk chunk in worldManager.hexChunks)
            {
                foreach (HexInfo hex in chunk.hexArray)
                {
                    //has a resource?
                    if (hex.currentResource.name != "None")
                    {
                        //change texture to resource
                        hex.ChangeTextureToResource();
                    }
                }
            }
        }

        /// <summary>
        /// Adds a resource to the resource array.
        /// </summary>
        /// <param name="r">Resource to add</param>
        public void AddResource(Resource r)
        {
            resources.Add(r);
            UpdateResourceNames();
        }

        /// <summary>
        /// Adds a resource to the resource array at the provided index.
        /// </summary>
        /// <param name="r">Resource to add</param>
        /// <param name="index">Index in which to add the resource</param>
        public void AddResourceAtIndex(Resource r, int index)
        {
            resources.Insert(index, r);
            UpdateResourceNames();
        }

        /// <summary>
        /// Removes a resource from the resource array.
        /// </summary>
        /// <param name="r">Resource to remove</param>
        public void DeleteResource(Resource r)
        {
            resources.Remove(r);
            UpdateResourceNames();
        }

        /// <summary>
        /// Attempts to return a resource from a provided name.
        /// </summary>
        /// <param name="name">The name of the resource to look for</param>
        /// <returns>The improvement with the name; null if not found</returns>
        public Resource TryGetResource(string name)
        {
            //cycle through all resources
            foreach(Resource r in resources)
            {
                //if the resource shares the name; return it
                if(r.name == name)
                {
                    return r;
                }
            }
            //not found; return null
            return null;
        }

        /// <summary>
        /// Creates an array of the resource names.
        /// </summary>
        public void UpdateResourceNames()
        {
            //only update if there are resources
            if (resources != null && resources.Count > 0)
            {
                //instatiate resource names array
                resourceNames = new string[resources.Count];
                for (int i = 0; i < resources.Count; i++)
                {
                    //assign each name into the array
                    resourceNames[i] = resources[i].name;
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

            //loop through all resources
            for (int i = 0; i < resources.Count; i++)
            {
                //get each resource and check if we can spawn them
                Resource r = resources[i];
                if (r.rule != null)
                {
                    //runs through the tests and if any return false, we can not spawn this resource; check the next
                    if (RuleTest.Test(hex, r.rule, tileManager))
                    {
                        //we can spawn it, but should we?
                        int number = (int)Random.Range(0, r.rarity);
                        if (number == 0)
                        {
                            //spawn resource
                            returnResource = r;
                            SpawnResource(hex, returnResource, false);
                            return;
                        }
                    }
                }
            }

            //no resource spawned; return "None"
            returnResource = resources[0];
        }

        /// <summary>
        /// Spawns the provided resource on the tile.
        /// Optional to regenerate the chunk.
        /// </summary>
        /// <param name="hex">Hex to spawn the resource on</param>
        /// <param name="r">Resource to spawn</param>
        /// <param name="regenerateChunk">If the parent chunk should be regenerated</param>
        public void SpawnResource(HexInfo hex, Resource r, bool regenerateChunk)
        {
            //reset resource locations
            hex.resourceLocations.Clear();
            
            //destroy previous resource objects
            if (hex.rObject != null)
            {
                Destroy(hex.rObject);
            }

            //if the resource has a mesh to spawn
            if (r.meshToSpawn != null)
            {
                //calculate y position to spawn the resources
                float y;
                if (hex.localMesh == null)
                {
                    y = (worldManager.hexExt.y); if (y == 0) { y -= ((hex.worldPosition.y + worldManager.hexExt.y) / Random.Range(4, 8)); } else { y = hex.worldPosition.y + worldManager.hexExt.y + hex.currentResource.meshToSpawn.bounds.extents.y; }
                }
                else
                {
                    y = (hex.localMesh.bounds.extents.y); if (y == 0) { y -= ((hex.worldPosition.y + hex.localMesh.bounds.extents.y) / Random.Range(4, 8)); } else { y = hex.worldPosition.y + hex.localMesh.bounds.extents.y + hex.currentResource.meshToSpawn.bounds.extents.y; }
                }

                //spawn a resource for each spawn amount
                for (int i = 0; i < r.spawnAmount; i++)
                {
                    //position setting
                    float x = (worldManager.hexCenter.x + Random.Range(-0.2f, 0.2f));
                    float z = (worldManager.hexCenter.z + Random.Range(-0.2f, 0.2f));
                    hex.resourceLocations.Add(new Vector3(x, y, z));
                }

                //number of resources
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

                    //create gameobject to hold the resource meshes 
                    GameObject holder = new GameObject(r.name + " at " + hex.AxialGridPosition, typeof(MeshFilter), typeof(MeshRenderer));

                    //set the gameobject position to the hex position
                    holder.transform.position = hex.worldPosition;
                    holder.transform.parent = hex.parentChunk.transform;

                    //set the resource mesh texture
                    holder.renderer.material.mainTexture = r.meshTexture;

                    //assign the combined mesh to the resource holder gameobject
                    MeshFilter filter = holder.GetComponent<MeshFilter>();
                    filter.mesh = new Mesh();
                    filter.mesh.CombineMeshes(combine);

                    //set the hex's resource object to the resource holder
                    hex.rObject = holder;

                    //UV mapping
                    Rect rectArea;
                    worldManager.textureAtlas.resourceLocations.TryGetValue(r, out rectArea);

                    //temp UV data
                    Vector2[] uv;
                    uv = new Vector2[filter.mesh.vertexCount];

                    //calculate the combined UV data
                    for (int i = 0; i < filter.mesh.vertexCount; i++)
                    {
                        uv[i] = new Vector2(filter.mesh.uv[i].x * rectArea.width + rectArea.x, filter.mesh.uv[i].y * rectArea.height + rectArea.y);
                    }

                    //assign the resource holder's UV data
                    filter.mesh.uv = uv;
                }
            }

            //if needed; regenerate the chunk and it's UV data
            if (regenerateChunk)
            {
                hex.ChangeTextureToResource();
                hex.parentChunk.RegenerateMesh();
            }
        }
    }


    /// <summary>
    /// Resource class that contains all the values for the base resource
    /// </summary>
    [System.Serializable]
    public class Resource
    {
        //improvement values
        public string name;
        public HexRule rule;
        public float rarity;
        public int spawnAmount;

        //object values
        public Mesh meshToSpawn;
        public Texture2D meshTexture;
        public bool replaceGroundTexture;

        public Resource(string name, float rarity, int spawnAmount, Mesh mesh, Texture2D resourceMeshTexture, bool replaceGroundTexture, HexRule rule)
        {
            this.name = name;
            this.rarity = rarity;
            this.meshToSpawn = mesh;
            this.meshTexture = resourceMeshTexture;
            this.replaceGroundTexture = replaceGroundTexture;
            this.rule = rule;
        }
    }
}