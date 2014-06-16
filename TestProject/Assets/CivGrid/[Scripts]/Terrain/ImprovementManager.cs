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

        public bool hideImprovements;

        public void SetUp()
        {
            improvements = searalizableImprovements;
            improvements.Insert(0, new Improvement("None", 0, null, null));
            improvements[0].meshToSpawn = null;
            improvements[0].spawnAmount = 0;

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
                improvementNames[i] = searalizableImprovements[i].improvementName;
            }
        }

        /// <summary>
        /// Adds improvement to specified hex if it meets the rule requirements
        /// </summary>
        /// <param name="hex">Hex to attempt to add the improvement upon</param>
        /// <param name="improvementName">"Improvement to attemp to add</param>
        /// <param name="returnImprovement">Improvement that has been added</param>
        public static void TestedAddImprovementToTile(HexInfo hex, string improvementName)
        {
            bool possible = false;

            Improvement returnImprovement = new Improvement();
            foreach (Improvement i in improvements)
            {
                if (improvementName == i.improvementName)
                {
                    returnImprovement = i;
                }
            }

            if (Test(returnImprovement.rule, hex))
            {
                possible = true;
            }
            else
            {
                possible = false;
            }
            

            if (possible)
            {
                hex.currentImprovement = returnImprovement;
                InitiateImprovement(hex, returnImprovement);
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
                if (hex.currentImprovement.improvementName != "None")
                {
                    //change texture of this hexagon back to its original if it has a resource it will be corrected below
                    hex.ChangeTextureToNormalTile();


                    //destory improvement children
                    foreach (GameObject obj in hex.improvementGameObjects)
                    {
                        Destroy(obj);
                    }

                    //reset improvementGameObject list
                    hex.improvementGameObjects = new List<GameObject>();

                    //respawn resource model
                    if (hex.currentResource.resourceName != "None")
                    {
                        hex.rM.SpawnResource(hex, hex.currentResource, true);
                    }
                }
            }

            //return "None"
            hex.currentImprovement = improvements[0];
        }

        private static void InitiateImprovement(HexInfo hex, Improvement i)
        {
            //delete resource gameobject
            GameObject holder = hex.parentChunk.gameObject;
            if (holder == null) { Debug.LogError("Could not find the resource holder!"); }

            //remove current improvements
            foreach (GameObject obj in hex.improvementGameObjects)
            {
                Destroy(obj);
            }
            //reset improvement gameObject list
            hex.improvementGameObjects = new List<GameObject>();

            //remove current resource gameobjects
            hex.RemoveResources();

            hex.ChangeTextureToImprovement();
           

            //spawn gameObject if needed
            if (i.meshToSpawn != null)
            {
                float y = (hex.worldPosition.y + hex.hexExt.y) - ((hex.worldPosition.y + hex.hexExt.y) / Random.Range(3, 6)); if (y == 0) { y -= ((hex.worldPosition.y + hex.hexExt.y) / Random.Range(4, 8)); }
                GameObject obj = new GameObject(i.improvementName + " at " + hex.CubeGridPosition, typeof(MeshFilter), typeof(MeshRenderer));
                obj.GetComponent<MeshFilter>().mesh = hex.currentImprovement.meshToSpawn;
                obj.transform.position = new Vector3((hex.worldPosition.x + hex.hexCenter.x + Random.Range(-0.2f, 0.2f)), y, (hex.worldPosition.z + hex.hexCenter.z + Random.Range(-0.2f, 0.2f)));
                obj.transform.rotation = Quaternion.identity;
                obj.renderer.material.mainTexture = hex.parentChunk.worldManager.textureAtlas.terrainAtlas;


                //add object to hexagon's improvementGameObject list
                hex.improvementGameObjects.Add(obj);

                //hide resource in heirarchy?
                if (hex.rM.hideResources) { obj.hideFlags = HideFlags.HideInHierarchy; } else { obj.hideFlags = HideFlags.None; }
            }
        }

        private static bool Test(ImprovementRule rule, HexInfo hex)
        {
            bool returnVal;

            for (int i = 0; i < rule.possibleTiles.Length; i++)
            {
                returnVal = TestRule(hex, rule.possibleTiles[i]);
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
        public string improvementName;
        public ImprovementRule rule;
        bool possible;
        public float rarity;

        //public Vector2 atlasLocation;

        public Mesh meshToSpawn;
        public int spawnAmount = 3;

        public Improvement(string name, float rarity, Mesh mesh, ImprovementRule rule)
        {
            this.improvementName = name;
            this.rule = rule;
            this.rarity = rarity;
            this.meshToSpawn = mesh;
        }

        public Improvement() { }
    }

    [System.Serializable]
    public class ImprovementRule
    {
        public Tile[] possibleTiles;
        public Feature[] possibleFeatures;

        public ImprovementRule(Tile[] possibleTiles, Feature[] possibleFeatures)
        {
            this.possibleTiles = possibleTiles;
            this.possibleFeatures = possibleFeatures;
        }
    }
}