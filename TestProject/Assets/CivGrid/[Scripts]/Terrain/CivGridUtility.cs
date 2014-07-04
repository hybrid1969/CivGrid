using CivGrid;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace CivGrid
{
    /// <summary>
    /// Helper class for basic utility methods
    /// </summary>
    public static class CivGridUtility
    {
        #region CivGrid Component Spawner

        [MenuItem("CivGrid/Create CivGrid Camera", priority = 6)]
        public static void CreateCivGridCamera()
        {
            new GameObject("CivGrid Camera", typeof(Camera), typeof(GUILayer), typeof(AudioListener), typeof(CivGridCamera));
        }

        [MenuItem("CivGrid/Create CivGrid World Manager", priority = 5)]
        public static void CreateCivGridWorldManager()
        {
            new GameObject("CivGrid World Manager", typeof(TileManager), typeof(ResourceManager), typeof(ImprovementManager), typeof(WorldManager));
        }

        #endregion

        /// <summary>
        /// Get the surronding pixels of the referenced pixel location
        /// </summary>
        /// <param name="tex">Texture where the pixel is located</param>
        /// <param name="x">"X" cords of the pixel</param>
        /// <param name="y">"Y" cords of the pixel</param>
        /// <returns>The eight surronding pixels</returns>
        public static float[] GetSurrondingPixels(Texture2D tex, int x, int y)
        {
            float[] returnArray = new float[8];

            returnArray[0] = tex.GetPixel(x + 1, y).r;
            returnArray[1] = tex.GetPixel(x + 1, y + 1).r;
            returnArray[2] = tex.GetPixel(x, y + 1).r;
            returnArray[3] = tex.GetPixel(x - 1, y + 1).r;
            returnArray[4] = tex.GetPixel(x - 1, y).r;
            returnArray[5] = tex.GetPixel(x - 1, y - 1).r;
            returnArray[6] = tex.GetPixel(x, y - 1).r;
            returnArray[7] = tex.GetPixel(x + 1, x - 1).r;

            return returnArray;
        }

        #region Array Conversion Void Return
        /// <summary>
        /// Converts a two-dimensional array into a single array
        /// </summary>
        /// <param name="doubleArray">The two-dimensional array of CombineInstance to convert into a single array</param>
        /// <param name="singleArray">The converted array</param>
        public static void ToSingleArray(CombineInstance[,] doubleArray, out CombineInstance[] singleArray)
        {
            //list to copy the values from the two-dimensional array into
            List<CombineInstance> combineList = new List<CombineInstance>();

            //cycle through all the CombineInstances and copy them into the List
            foreach (CombineInstance combine in doubleArray)
            {
                combineList.Add(combine);
            }

            //convert our List that holds all the CombineInstances into a single array
            singleArray = combineList.ToArray();
        }

        /// <summary>
        /// Converts a two-dimensional array into a single array
        /// </summary>
        /// <param name="doubleArray">The two-dimensional array of Vector2 to convert into a single array</param>
        /// <param name="singleArray">The converted array</param>
        public static void ToSingleArray(Vector2[,] doubleArray, out Vector2[] singleArray)
        {
            List<Vector2> combineList = new List<Vector2>();
            
            foreach (Vector2 combine in doubleArray)
            {
                combineList.Add(combine);
            }

            singleArray = combineList.ToArray();
        }

        /// <summary>
        /// Converts a two-dimensional array into a single array
        /// </summary>
        /// <param name="doubleArray">The two-dimensional array of HexChunk to convert into a single array</param>
        /// <param name="singleArray">The converted array</param>
        public static void ToSingleArray(HexChunk[,] doubleArray, out HexChunk[] singleArray)
        {
            List<HexChunk> combineList = new List<HexChunk>();
            
            foreach (HexChunk combine in doubleArray)
            {
                combineList.Add(combine);
            }

            singleArray = combineList.ToArray();
        }

        /// <summary>
        /// Converts a two-dimensional array into a single array
        /// </summary>
        /// <param name="doubleArray">The two-dimensional array of Texture to convert into a single array</param>
        /// <param name="singleArray">The converted array</param>
        public static void ToSingleArray(Texture2D[,] doubleArray, out Texture2D[] singleArray)
        {
            List<Texture2D> combineList = new List<Texture2D>();

            foreach (Texture2D combine in doubleArray)
            {
                combineList.Add(combine);
            }

            singleArray = combineList.ToArray();
        }
        #endregion

        #region Array Conversion [] Return
        
        /// <summary>
        /// Converts a two-dimensional array into a single array
        /// </summary>
        /// <param name="doubleArray">The two-dimensional array of CombineInstance to convert into a single array</param>
        /// <returns>The converted array</returns>
        public static CombineInstance[] ToSingleArray(CombineInstance[,] doubleArray)
        {
            List<CombineInstance> combineList = new List<CombineInstance>();

            foreach (CombineInstance combine in doubleArray)
            {
                combineList.Add(combine);
            }

            return (combineList.ToArray());
        }

        /// <summary>
        /// Converts a two-dimensional array into a single array
        /// </summary>
        /// <param name="doubleArray">The two-dimensional array of Vector2 to convert into a single array</param>
        /// <returns>The converted array</returns>
        public static Vector2[] ToSingleArray(Vector2[,] doubleArray)
        {
            List<Vector2> combineList = new List<Vector2>();

            foreach (Vector2 combine in doubleArray)
            {
                combineList.Add(combine);
            }

            return (combineList.ToArray());
        }

        /// <summary>
        /// Converts a two-dimensional array into a single array
        /// </summary>
        /// <param name="doubleArray">The two-dimensional array of HexChunk to convert into a single array</param>
        /// <returns>The converted array</returns>
        public static HexChunk[] ToSingleArray(HexChunk[,] doubleArray)
        {
            List<HexChunk> combineList = new List<HexChunk>();

            foreach (HexChunk combine in doubleArray)
            {
                combineList.Add(combine);
            }

            return (combineList.ToArray());
        }

        /// <summary>
        /// Converts a two-dimensional array into a single array
        /// </summary>
        /// <param name="doubleArray">The two-dimensional array of Texture2D to convert into a single array</param>
        /// <returns>The converted array</returns>
        public static Texture2D[] ToSingleArray(Texture2D[,] doubleArray)
        {
            List<Texture2D> combineList = new List<Texture2D>();

            foreach (Texture2D combine in doubleArray)
            {
                combineList.Add(combine);
            }

            return (combineList.ToArray());
        }
        #endregion
    }

    /// <summary>
    /// Extends arrays of TileItem, ResourceItem, and ImprovementItem to act similar to a dictionary
    /// </summary>
    public static class DictionaryExtensionMethods
    {

        #region TryGetValue

        /// <summary>
        /// Attempts to return the texture atlas location of this Improvement from the array based on a key
        /// </summary>
        /// <param name="key">The key to search for a match within the array</param>
        /// <param name="location">A out reference of the Rect location of this Tile within the texture atlas</param>
        /// <returns>If the key was found in the array</returns>
        public static bool TryGetValue(this TileItem[] list, Tile key, out Rect location)
        {
            foreach (TileItem item in list)
            {
                if (item.Key.name == key.name)
                {
                    location = item.Value;
                    return true;
                }
            }
            Debug.LogError("Couldn't get a value from the given key: " + key.name);
            location = new Rect();
            return false;
        }

        /// <summary>
        /// Attempts to return the texture atlas location of this Improvement from the array based on a key
        /// </summary>
        /// <param name="key">The key to search for a match within the array</param>
        /// <param name="location">A out reference of the Rect location of this Resource within the texture atlas</param>
        /// <returns>If the key was found in the array</returns>
        public static bool TryGetValue(this ResourceItem[] list, Resource key, out Rect location)
        {
            foreach (ResourceItem item in list)
            {
                if (item.Key.name == key.name)
                {
                    location =  item.Value;
                    return true;
                }
            }
            Debug.LogError("Couldn't get a value from the given key: " + key.name);
            location = new Rect();
            return false;
        }

        /// <summary>
        /// Attempts to return the texture atlas location of this Improvement from the array based on a key
        /// </summary>
        /// <param name="key">The key to search for a match within the array</param>
        /// <param name="location">A out reference of the Rect location of this Improvement within the texture atlas</param>
        /// <returns>If the key was found in the array</returns>
        public static bool TryGetValue(this ImprovementItem[] list, Improvement key, out Rect location)
        {
            foreach (ImprovementItem item in list)
            {
                if (item.Key.name == key.name)
                {
                    location = item.Value;
                    return true;
                }
            }
            Debug.LogError("Couldn't get a value from the given key: " + key.name);
            location = new Rect();
            return false;
        }

        #endregion

        #region ContainsKey

        /// <summary>
        /// Checks if a key exists in the array
        /// </summary>
        /// <param name="key">The key to look for within this array</param>
        /// <returns>If the key was found</returns>
        public static bool ContainsKey(this List<TileItem> list, Tile key)
        {
            foreach (TileItem item in list)
            {
                if (item.Key == key)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if a key exists in the array
        /// </summary>
        /// <param name="key">The key to look for within this array</param>
        /// <returns>If the key was found</returns>
        public static bool ContainsKey(this List<ResourceItem> list, Resource key)
        {
            foreach (ResourceItem item in list)
            {
                if (item.Key == key)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if a key exists in the array
        /// </summary>
        /// <param name="key">The key to look for within this array</param>
        /// <returns>If the key was found</returns>
        public static bool ContainsKey(this List<ImprovementItem> list, Improvement key)
        {
            foreach (ImprovementItem item in list)
            {
                if (item.Key == key)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Add

        /// <summary>
        /// Adds a new entry of a key and matching value to this array
        /// </summary>
        /// <param name="tileToAdd">Tile to add as the key</param>
        /// <param name="rectToAdd">Rect to add as the value</param>
        public static void Add(this List<TileItem> list, Tile tileToAdd, Rect rectToAdd)
        {
            list.Add(new TileItem(tileToAdd, rectToAdd));
        }

        /// <summary>
        /// Adds a new entry of a key and matching value to this array
        /// </summary>
        /// <param name="tileToAdd">Tile to add as the key</param>
        /// <param name="rectToAdd">Rect to add as the value</param>
        public static void Add(this List<ResourceItem> list, Resource resourceToAdd, Rect rectToAdd)
        {
            list.Add(new ResourceItem(resourceToAdd, rectToAdd));
        }

        /// <summary>
        /// Adds a new entry of a key and matching value to this array
        /// </summary>
        /// <param name="tileToAdd">Tile to add as the key</param>
        /// <param name="rectToAdd">Rect to add as the value</param>s
        public static void Add(this List<ImprovementItem> list, Improvement improvementToAdd, Rect rectToAdd)
        {
            list.Add(new ImprovementItem(improvementToAdd, rectToAdd));
        }

        #endregion
    }

}