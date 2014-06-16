using CivGrid;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CivGrid
{

    public static class CivGridUtility
    {
        #region void
        public static void ToSingleArray(CombineInstance[,] doubleArray, out CombineInstance[] singleArray)
        {
            List<CombineInstance> combineList = new List<CombineInstance>();
            
            foreach (CombineInstance combine in doubleArray)
            {
                combineList.Add(combine);
            }

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
        public Texture2D terrainAtlas;
        //public Vector2 texturesInAtlas;
        public TileItem[] tileLocations;

        //public Texture2D resourceAtlas;
        public ResourceItem[] resourceLocations;
        //public Vector2 resourceTexturesInAtlas;

        //public Texture2D improvementAtlas;
        public ImprovementItem[] improvementLocations;
        //public Vector2 improvementTexturesInAtlas;
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

        public static Rect TryGetValue(this TileItem[] list, Tile key)
        {
            foreach (TileItem item in list)
            {
                if (item.Key == key)
                {
                    return item.Value;
                }
            }
            Debug.LogError("Couldn't get a value from the given key: " + key.ToString());
            return new Rect();
        }

        public static Rect TryGetValue(this ResourceItem[] list, Resource key)
        {
            foreach (ResourceItem item in list)
            {
                Debug.Log(item.Key.resourceName + " ?= " + key.resourceName);
                if (item.Key.resourceName == key.resourceName)
                {
                    return item.Value;
                }
            }
            Debug.LogError("Couldn't get a value from the given key: " + key.resourceName);
            return new Rect();
        }

        public static Rect TryGetValue(this ImprovementItem[] list, Improvement key)
        {
            foreach (ImprovementItem item in list)
            {
                if (item.Key.improvementName == key.improvementName)
                {
                    return item.Value;
                }
            }
            Debug.LogError("Couldn't get a value from the given key: " + key.improvementName);
            return new Rect();
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