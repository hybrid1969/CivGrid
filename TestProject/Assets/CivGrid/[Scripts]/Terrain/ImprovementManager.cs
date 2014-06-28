using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CivGrid;

namespace CivGrid
{

    public class ImprovementManager : MonoBehaviour
    {

        private static List<Improvement> improvements;
        public List<Improvement> searalizableImprovements;
        public string[] improvementNames;

        public ResourceManager rM;
        public TileManager tM;

        public void SetUp()
        {
            improvements = searalizableImprovements;
            improvements.Insert(0, new Improvement("None", 0, null, null, null));
            improvements[0].meshToSpawn = null;
            improvements[0].spawnAmount = 0;

            rM = GetComponent<ResourceManager>();
            tM = GetComponent<TileManager>();

            if (improvementNames == null)
            {
                UpdateImprovementNames();
            }
        }

        public void AddImprovement(Improvement i)
        {
            searalizableImprovements.Add(i);
            UpdateImprovementNames();
        }

        public void AddImprovementAtIndex(Improvement i, int index)
        {
            searalizableImprovements.Insert(index, i);
            UpdateImprovementNames();
        }

        public void DeleteImprovement(Improvement i)
        {
            searalizableImprovements.Remove(i);
            UpdateImprovementNames();
        }

        public void UpdateImprovementNames()
        {
            improvementNames = new string[searalizableImprovements.Count];
            for (int i = 0; i < searalizableImprovements.Count; i++)
            {
                improvementNames[i] = searalizableImprovements[i].name;
            }
        }

        /// <summary>
        /// Adds improvement to specified hex if it meets the rule requirements; slower than passing in an Improvement
        /// </summary>
        /// <param name="hex">Hex to attempt to add the improvement upon</param>
        /// <param name="improvementName">"Improvement to attempt to add</param>
        public static void TestedAddImprovementToTile(HexInfo hex, string improvementName)
        {
            Debug.Log("I want a stack trace");
            bool possible = false;

            Improvement improvement = new Improvement();
            foreach (Improvement i in improvements)
            {
                if (improvementName == i.name)
                {
                    improvement = i;
                }
            }

            if (Test(improvement.rule, hex))
            {
                possible = true;
            }
            else
            {
                possible = false;
            }
            

            if (possible)
            {
                hex.currentImprovement = improvement;
                hex.parentChunk.worldManager.iM.InitiateImprovement(hex, improvement);
            }
        }

        /// <summary>
        /// Adds improvement to specified hex if it meets the rule requirements
        /// </summary>
        /// <param name="hex">Hex to attempt to add the improvement upon</param>
        /// <param name="improvementName">"Improvement to attempt to add</param>
        public static void TestedAddImprovementToTile(HexInfo hex, Improvement improvement)
        {
            bool possible = false;

            if (Test(improvement.rule, hex))
            {
                possible = true;
            }
            else
            {
                possible = false;
            }


            if (possible)
            {
                hex.currentImprovement = improvement;
                hex.parentChunk.worldManager.iM.InitiateImprovement(hex, improvement);
            }
        }

        /// <summary>
        /// Removes the improvement from the specified hex and restores its past state
        /// </summary>
        /// <param name="hex">Hex to remove all improvements from</param>
        public static void RemoveImprovementFromTile(HexInfo hex)
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
                        hex.rM.SpawnResource(hex, hex.currentResource, true);
                    }
                }
            }

            //return "None"
            hex.currentImprovement = improvements[0];
        }

        private void InitiateImprovement(HexInfo hex, Improvement i)
        {
            GameObject resourceHolder = hex.parentChunk.gameObject;
            if (resourceHolder == null) { Debug.LogError("Could not find the resource holder!"); }

            //remove current improvements
            Destroy(hex.iObject);

            //remove current resource gameobjects
            rM.HideResourceMesh(hex);

            hex.ChangeTextureToImprovement();
           

            //spawn gameObject if needed
            if (i.meshToSpawn != null)
            {
                float y = (hex.worldPosition.y + hex.hexExt.y) - ((hex.hexExt.y) / 5f); if (y == 0) { y -= ((hex.worldPosition.y + hex.hexExt.y) / Random.Range(4, 8)); }
                GameObject holder = new GameObject(i.name + " at " + hex.CubeGridPosition, typeof(MeshFilter), typeof(MeshRenderer));
                holder.GetComponent<MeshFilter>().mesh = hex.currentImprovement.meshToSpawn;
                holder.transform.position = new Vector3((hex.worldPosition.x + hex.hexCenter.x + Random.Range(-0.2f, 0.2f)), y, (hex.worldPosition.z + hex.hexCenter.z + Random.Range(-0.2f, 0.2f)));
                holder.transform.rotation = Quaternion.identity;
                holder.renderer.material.mainTexture = i.improvementMeshTexture;
                holder.transform.parent = hex.parentChunk.transform;

                hex.iObject = holder;
            }
        }

        private static bool Test(ImprovementRule rule, HexInfo hex)
        {
            bool returnVal;
            TileManager tM = hex.parentChunk.worldManager.tM;

            for (int i = 0; i < rule.possibleTiles.Length; i++)
            {
                returnVal = TestRule(hex, tM.tiles[rule.possibleTiles[i]]);
                if (returnVal == true) break;
                if (i == (rule.possibleTiles.Length - 1)) { return false; }
            }
            for (int i = 0; i < rule.possibleFeatures.Length; i++)
            {
                returnVal = TestRule(hex, rule.possibleFeatures[i]);
                if (returnVal == true) return true;
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

        private static bool NotTestRule(HexInfo hex, Feature feature)
        {
            if (hex.terrainFeature == feature) { return false; } else { return true; }
        }

        private static bool NotTestRule(HexInfo hex, Tile tile)
        {
            if (hex.terrainType == tile) { return false; } else { return true; }
        }
    }

    /// <summary>
    /// Improvement
    /// </summary>
    [System.Serializable]
    public class Improvement
    {
        public string name;
        public ImprovementRule rule;
        bool possible;
        public float rarity;

        //public Vector2 atlasLocation;

        public Mesh meshToSpawn;
        public Texture2D improvementMeshTexture;
        public int spawnAmount = 3;

        public Improvement(string name, float rarity, Mesh mesh, Texture2D improvementMeshTexture, ImprovementRule rule)
        {
            this.name = name;
            this.rule = rule;
            this.rarity = rarity;
            this.improvementMeshTexture = improvementMeshTexture;
            this.meshToSpawn = mesh;
        }

        public Improvement() { }
    }

    [System.Serializable]
    public class ImprovementRule
    {
        public int[] possibleTiles;
        public Feature[] possibleFeatures;

        public ImprovementRule(int[] possibleTiles, Feature[] possibleFeatures)
        {
            this.possibleTiles = possibleTiles;
            this.possibleFeatures = possibleFeatures;
        }
    }
}