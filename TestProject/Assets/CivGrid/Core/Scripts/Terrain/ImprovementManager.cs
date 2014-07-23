using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CivGrid;

namespace CivGrid
{
    /// <summary>
    /// Contains all possible improvements.
    /// Handles the addition and removal of these improvements upon hexagons.
    /// </summary>
    public class ImprovementManager : MonoBehaviour
    {
        //improvements
        public List<Improvement> improvements;
        public string[] improvementNames;

        //cached managers
        public ResourceManager resourceManager;
        public TileManager tileManager;

        /// <summary>
        /// Sets up the improvement manager.
        /// Caches all needed values.
        /// </summary>
        public void SetUp()
        {
            //insert default "None" improvement into the improvement array
            improvements.Insert(0, new Improvement("None", null, null, false, null));

            //cache managers
            resourceManager = GetComponent<ResourceManager>();
            tileManager = GetComponent<TileManager>();

            //instatiate the improvement name array
            if (improvementNames == null)
            {
                UpdateImprovementNames();
            }
        }

        /// <summary>
        /// Creates the improvement GameObject and switches the hex texture.
        /// </summary>
        /// <param name="hex">Hex to create the improvement on</param>
        /// <param name="i">Improvement to add</param>
        private void InitiateImprovementsOnHexs(HexInfo hex, Improvement i)
        {
            //get the parent chunk object; this is where we will parent the improvement objects to
            GameObject resourceHolder = hex.parentChunk.gameObject;
            if (resourceHolder == null) { Debug.LogError("Could not find the resource holder!"); }

            //remove current improvements
            Destroy(hex.iObject);

            //remove current resource gameobjects
            Destroy(hex.rObject);

            //switch the hex's texture to this improvement's ground texture
            hex.ChangeTextureToImprovement();


            //spawn gameObject if there is a mesh to spawn
            if (i.meshToSpawn != null)
            {
                float y = (hex.worldPosition.y + hex.hexExt.y) - ((hex.hexExt.y) / 5f); if (y == 0) { y -= ((hex.worldPosition.y + hex.hexExt.y) / Random.Range(4, 8)); }
                GameObject holder = new GameObject(i.name + " at " + hex.AxialGridPosition, typeof(MeshFilter), typeof(MeshRenderer));
                holder.GetComponent<MeshFilter>().mesh = hex.currentImprovement.meshToSpawn;
                holder.transform.position = new Vector3((hex.worldPosition.x + hex.hexCenter.x + Random.Range(-0.2f, 0.2f)), y, (hex.worldPosition.z + hex.hexCenter.z + Random.Range(-0.2f, 0.2f)));
                holder.transform.rotation = Quaternion.identity;
                holder.renderer.material.mainTexture = i.meshTexture;
                holder.transform.parent = hex.parentChunk.transform;

                hex.iObject = holder;
            }
        }

        /// <summary>
        /// Adds an improvement to the improvement array.
        /// </summary>
        /// <param name="i">Improvement to add</param>
        public void AddImprovement(Improvement i)
        {
            improvements.Add(i);
            UpdateImprovementNames();
        }

        /// <summary>
        /// Adds an improvement to the improvement array at the provided index.
        /// </summary>
        /// <param name="i">Improvement to add</param>
        /// <param name="index">Index in which to add the improvement</param>
        public void AddImprovementAtIndex(Improvement i, int index)
        {
            improvements.Insert(index, i);
            UpdateImprovementNames();
        }

        /// <summary>
        /// Removes an improvement from the improvement array.
        /// </summary>
        /// <param name="i">Improvement to remove</param>
        public void DeleteImprovement(Improvement i)
        {
            improvements.Remove(i);
            UpdateImprovementNames();
        }

        /// <summary>
        /// Attempts to return an improvement from a provided name.
        /// </summary>
        /// <param name="name">The name of the improvement to look for</param>
        /// <returns>The improvement with the name; null if not found</returns>
        public Improvement TryGetImprovement(string name)
        {
            //cycle through all improvements
            foreach(Improvement i in improvements)
            {
                //if the improvement shares the name; return it
                if(i.name == name)
                {
                    return i;
                }
            }
            //not found; return null
            return null;
        }

        /// <summary>
        /// Creates an array of the improvement names.
        /// </summary>
        public void UpdateImprovementNames()
        {
            //only update if there are improvements
            if(improvements != null && improvements.Count > 0)
            {
                //instatiate improvement names array
                improvementNames = new string[improvements.Count];
                for (int i = 0; i < improvements.Count; i++)
                {
                    //assign each name into the array
                    improvementNames[i] = improvements[i].name;
                }
            }
        }

        /// <summary>
        /// Adds improvement to specified hex if it meets the rule requirements; slower than passing in an Improvement.
        /// </summary>
        /// <param name="hex">Hex to attempt to add the improvement upon</param>
        /// <param name="improvementName">"Improvement to attempt to add</param>
        public void TestedAddImprovementToTile(HexInfo hex, string improvementName)
        {
            //if it's possible to spawn the improvement according to it's rules
            bool possible = false;

            //gets improvement from it's name
            Improvement improvement = TryGetImprovement(improvementName);

            //runs through the tests and if any return false, we can not spawn the improvement
            if (RuleTest.Test(hex, improvement.rule, tileManager))
            {
                possible = true;
            }
            else
            {
                possible = false;
            }
            
            //spawn the improvement on the tile
            if (possible)
            {
                hex.currentImprovement = improvement;
                hex.parentChunk.worldManager.improvementManager.InitiateImprovementsOnHexs(hex, improvement);
            }
        }

        /// <summary>
        /// Adds improvement to specified hex if it meets the rule requirements.
        /// </summary>
        /// <param name="hex">Hex to attempt to add the improvement upon</param>
        /// <param name="improvementName">"Improvement to attempt to add</param>
        [System.Obsolete("Use improvementIndex overload; otherwise retrieve [index+1] to return correct improvement")]
        public void TestedAddImprovementToTile(HexInfo hex, Improvement improvement)
        {
            //if it's possible to spawn the improvement according to it's rules
            bool possible = false;

            //runs through the tests and if any return false, we can not spawn the improvement
            if (RuleTest.Test(hex, improvement.rule, tileManager))
            {
                possible = true;
            }
            else
            {
                possible = false;
            }

            //spawn the improvement on the tile
            if (possible)
            {
                hex.currentImprovement = improvement;
                hex.parentChunk.worldManager.improvementManager.InitiateImprovementsOnHexs(hex, improvement);
            }
        }

        /// <summary>
        /// Adds improvement to specified hex if it meets the rule requirements.
        /// </summary>
        /// <param name="hex">Hex to attempt to add the improvement upon</param>
        /// <param name="improvementIndex">Index of the improvement within the improvement manager to attemp to add</param>
        public void TestedAddImprovementToTile(HexInfo hex, int improvementIndex)
        {
            //if it's possible to spawn the improvement according to it's rules
            bool possible = false;

            //gets improvement from it's index
            Improvement improvement = improvements[improvementIndex+1];

            //runs through the tests and if any return false, we can not spawn the improvement
            if (RuleTest.Test(hex, improvement.rule, tileManager))
            {
                possible = true;
            }
            else
            {
                possible = false;
            }

            //spawn the improvement on the tile
            if (possible)
            {
                hex.currentImprovement = improvement;
                hex.parentChunk.worldManager.improvementManager.InitiateImprovementsOnHexs(hex, improvement);
            }
        }

        /// <summary>
        /// Removes the improvement from the specified hex and restores its past state.
        /// </summary>
        /// <param name="hex">Hex to remove all improvements from</param>
        public void RemoveImprovementFromTile(HexInfo hex)
        {
            if (hex.currentImprovement != null)
            {
                if (hex.currentImprovement.name != "None")
                {
                    //change texture of this hexagon back to its original if it has a resource it will be corrected below
                    hex.ChangeTextureToNormalTile();


                    //destory improvement children
                    Destroy(hex.iObject);

                    //respawn resource model
                    if (hex.currentResource.name != "None")
                    {
                        hex.resourceManager.SpawnResource(hex, hex.currentResource, true);
                    }
                }
            }

            //return "None"
            hex.currentImprovement = improvements[0];
        }
    }

    /// <summary>
    /// Improvement class that contains all values for the base improvement
    /// </summary>
    [System.Serializable]
    public class Improvement
    {
        //improvement values
        public string name;
        public HexRule rule;

        //object values
        public Mesh meshToSpawn;
        public Texture2D meshTexture;
        public bool replaceGroundTexture;

        public Improvement(string name, Mesh mesh, Texture2D improvementMeshTexture, bool replaceGroundTexture, HexRule rule)
        {
            this.name = name;
            this.rule = rule;
            this.meshTexture = improvementMeshTexture;
            this.meshToSpawn = mesh;
        }

        public Improvement() { }
    }
}