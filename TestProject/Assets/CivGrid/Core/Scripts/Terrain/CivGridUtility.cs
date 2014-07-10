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


        /// <summary>
        /// Converts a two-dimensional array into a single array
        /// </summary>
        /// <param name="doubleArray">The two-dimensional array of type T to convert into a single array</param>
        /// <param name="singleArray">The converted array</param>
        public static void ToSingleArray<T>(T[,] doubleArray, out T[] singleArray)
        {
            //list to copy the values from the two-dimensional array into
            List<T> combineList = new List<T>();

            //cycle through all the members and copy them into the List
            foreach (T combine in doubleArray)
            {
                combineList.Add(combine);
            }

            //convert our List into a single array
            singleArray = combineList.ToArray();
        }


        /// <summary>
        /// Converts a two-dimensional array into a single array
        /// </summary>
        /// <param name="doubleArray">The two-dimensional array of CombineInstance to convert into a single array</param>
        /// <returns>The converted array</returns>
        public static T[] ToSingleArray<T>(T[,] doubleArray)
        {
            List<T> combineList = new List<T>();

            foreach (T combine in doubleArray)
            {
                combineList.Add(combine);
            }

            return (combineList.ToArray());
        }

        public static T[,] Resize2DArray<T>(T[,] original, int rows, int cols)
        {
            var newArray = new T[rows, cols];
            int minRows = Math.Min(rows, original.GetLength(0));
            int minCols = Math.Min(cols, original.GetLength(1));
            for (int i = 0; i < minRows; i++)
                for (int j = 0; j < minCols; j++)
                    newArray[i, j] = original[i, j];
            return newArray;
        }

        public static bool StringToBool(string stringToCast)
        {
            if (stringToCast == "true" || stringToCast == "True") { return true; }
            else
            {
                return false;
            }
        }
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
            List<Rect> tempList = new List<Rect>();

            foreach (TileItem item in list)
            {
                if (item.Key.name == key.name)
                {
                    tempList.Add(item.Value);
                }
            }

            if (tempList.Count > 1)
            {
                int index = UnityEngine.Random.Range(0, tempList.Count);

                location = tempList[index];
                return true;
            }
            else if (tempList.Count == 1)
            {
                location = tempList[0];
                return true;
            }
            else
            {
                //not found
                Debug.LogError("Couldn't get a value from the given key: " + key.name);
                location = new Rect();
                return false;
            }
        }

        /// <summary>
        /// Attempts to return the texture atlas location of this Improvement from the array based on a key
        /// </summary>
        /// <param name="key">The key to search for a match within the array</param>
        /// <param name="location">A out reference of the Rect location of this Resource within the texture atlas</param>
        /// <returns>If the key was found in the array</returns>
        public static bool TryGetValue(this ResourceItem[] list, Resource key, out Rect location)
        {
            List<Rect> tempList = new List<Rect>();

            foreach (ResourceItem item in list)
            {
                if (item.Key.name == key.name)
                {
                    tempList.Add(item.Value);
                }
            }

            if (tempList.Count > 1)
            {
                int index = UnityEngine.Random.Range(0, tempList.Count);

                location = tempList[index];
                return true;
            }
            else if (tempList.Count == 1)
            {
                location = tempList[0];
                return true;
            }
            else
            {
                //not found
                Debug.LogError("Couldn't get a value from the given key: " + key.name);
                location = new Rect();
                return false;
            }
        }

        /// <summary>
        /// Attempts to return the texture atlas location of this Improvement from the array based on a key
        /// </summary>
        /// <param name="key">The key to search for a match within the array</param>
        /// <param name="location">A out reference of the Rect location of this Improvement within the texture atlas</param>
        /// <returns>If the key was found in the array</returns>
        public static bool TryGetValue(this ImprovementItem[] list, Improvement key, out Rect location)
        {
            List<Rect> tempList = new List<Rect>();

            foreach (ImprovementItem item in list)
            {
                if (item.Key.name == key.name)
                {
                    tempList.Add(item.Value);
                }
            }

            if (tempList.Count > 1)
            {
                int index = UnityEngine.Random.Range(0, tempList.Count);

                location = tempList[index];
                return true;
            }
            else if (tempList.Count == 1)
            {
                location = tempList[0];
                return true;
            }
            else
            {
                //not found
                Debug.LogError("Couldn't get a value from the given key: " + key.name);
                location = new Rect();
                return false;
            }
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

        public static bool ContainsKey(this TileItem[] list, Tile key)
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

        public static bool ContainsKey(this ResourceItem[] list, Resource key)
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

        public static bool ContainsKey(this ImprovementItem[] list, Improvement key)
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