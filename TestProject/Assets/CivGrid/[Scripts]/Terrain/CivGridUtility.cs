using CivGrid;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CivGrid
{
    /// <summary>
    /// Helper class for basic utility methods
    /// </summary>
    public static class CivGridUtility
    {
        #region void
        /// <summary>
        /// Converts a two-dimensional array into a single array
        /// </summary>
        /// <param name="doubleArray">The two-dimensional array array to convert into a single array</param>
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

        public static void ToSingleArray(Vector2[,] doubleArray, out Vector2[] singleArray)
        {
            List<Vector2> combineList = new List<Vector2>();
            
            foreach (Vector2 combine in doubleArray)
            {
                combineList.Add(combine);
            }

            singleArray = combineList.ToArray();
        }

        public static void ToSingleArray(HexChunk[,] doubleArray, out HexChunk[] singleArray)
        {
            List<HexChunk> combineList = new List<HexChunk>();
            
            foreach (HexChunk combine in doubleArray)
            {
                combineList.Add(combine);
            }

            singleArray = combineList.ToArray();
        }

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

        #region returns
        public static CombineInstance[] ToSingleArray(CombineInstance[,] doubleArray)
        {
            List<CombineInstance> combineList = new List<CombineInstance>();

            foreach (CombineInstance combine in doubleArray)
            {
                combineList.Add(combine);
            }

            return (combineList.ToArray());
        }

        public static Vector2[] ToSingleArray(Vector2[,] doubleArray)
        {
            List<Vector2> combineList = new List<Vector2>();

            foreach (Vector2 combine in doubleArray)
            {
                combineList.Add(combine);
            }

            return (combineList.ToArray());
        }

        public static HexChunk[] ToSingleArray(HexChunk[,] doubleArray)
        {
            List<HexChunk> combineList = new List<HexChunk>();

            foreach (HexChunk combine in doubleArray)
            {
                combineList.Add(combine);
            }

            return (combineList.ToArray());
        }

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

    [Serializable]
    public class TextureAtlas
    {
        [SerializeField]
        public Texture2D terrainAtlas;
        [SerializeField]
        public TileItem[] tileLocations;
        [SerializeField]
        public ResourceItem[] resourceLocations;
        [SerializeField]
        public ImprovementItem[] improvementLocations;
    }

    [Serializable]
    public class TileItem
    {
        [SerializeField]
        private Tile key;

        [SerializeField]
        public Tile Key
        {
            get
            {
                return key;
            }
            set
            {
                key = value;
            }
        }

        [SerializeField]
        private Rect value;

        [SerializeField]
        public Rect Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }

        [SerializeField]
        public TileItem(Tile key, Rect value)
        {
            this.key = key;
            this.value = value;
        }
    }

    [Serializable]
    public class ResourceItem
    {
        [SerializeField]
        private Resource key;

        [SerializeField]
        public Resource Key
        {
            get
            {
                return key;
            }
            set
            {
                key = value;
            }
        }

        [SerializeField]
        private Rect value;

        [SerializeField]
        public Rect Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }

        [SerializeField]
        public ResourceItem(Resource key, Rect value)
        {
            this.key = key;
            this.value = value;
        }
    }

    [Serializable]
    public class ImprovementItem
    {
        [SerializeField]
        private Improvement key;

        [SerializeField]
        public Improvement Key
        {
            get
            {
                return key;
            }
            set
            {
                key = value;
            }
        }

        [SerializeField]
        private Rect value;

        [SerializeField]
        public Rect Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }

        [SerializeField]
        public ImprovementItem(Improvement key, Rect value)
        {
            this.key = key;
            this.value = value;
        }
    }

    public static class DictionaryExtensionMethods
    {

        #region TryGetValue

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

        public static void Add(this List<TileItem> list, Tile tileToAdd, Rect rectToAdd)
        {
            list.Add(new TileItem(tileToAdd, rectToAdd));
        }

        public static void Add(this List<ResourceItem> list, Resource resourceToAdd, Rect rectToAdd)
        {
            list.Add(new ResourceItem(resourceToAdd, rectToAdd));
        }

        public static void Add(this List<ImprovementItem> list, Improvement improvementToAdd, Rect rectToAdd)
        {
            list.Add(new ImprovementItem(improvementToAdd, rectToAdd));
        }

        #endregion
    }
}