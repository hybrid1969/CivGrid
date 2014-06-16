using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CivGrid;

namespace CivGrid
{

    public static class TextureManager
    {
        private static List<int> Factor(int number)
        {
            List<int> factors = new List<int>();
            int max = (int)Mathf.Sqrt(number);  //round down
            for (int factor = 1; factor <= max; ++factor)
            { //test from 1 to the square root, or the int below it, inclusive.
                if (number % factor == 0)
                {
                    factors.Add(factor);
                    if (factor != number / factor)
                    { // Don't add the square root twice!  Thanks Jon
                        factors.Add(number / factor);
                    }
                }
            }
            return factors;
        }

        private static Vector2 FindDimensions(int length, out bool addedBlank)
        {
            if (length % 2 > 0)
            {
                length += 1;
                addedBlank = true;
            }
            else
            {
                addedBlank = false;
            }

            //special case
            if (length == 2)
            {
                return new Vector2(2f, 1f);
            }

            List<int> list = Factor(length);

            foreach (int i in list)
            {
                if (i != 1 && i != length)
                {
                    Vector2 returnVal = new Vector2(length / i, i);
                    Debug.Log(returnVal.ToString());
                    return returnVal;
                }
            }

            Debug.LogError("Couldnt find possible atlas dimensions");
            return new Vector2();
        }

        public static Texture2D AtlasTextures(Texture2D[] textures, Vector2 dimensions, out Vector2 numberOfTextures/*,out bool blank*/)
        {
            Debug.Log(textures[0].GetPixel(0,0).ToString());
            try
            {
                textures[0].GetPixel(0, 0);
            }
            catch (UnityException e)
            {
                if (e.Message.StartsWith("Texture '" + textures[0].name + "' is not readable"))
                {
                    Debug.LogError("Please enable read/write on texture [" + textures[0].name + "]");
                }
            }
             
            int xRow = 0;
            int yColumn = 0;
            int xPixelPos;
            int yPixelPos;

            if (CheckTextureSizes(textures) == false)
            {
                Debug.Log("Textures must be the same sizes");
                numberOfTextures = new Vector2(0,0);
                return null;
            }

            Texture2D returnTexture = new Texture2D(textures[0].width * (int)dimensions.x, textures[0].height * (int)dimensions.y);
            
            for(int i = 0; i <= (textures.Length-1); i++)
            {
                xRow++;
                if (xRow == (dimensions.x + 1))
                {
                    xRow = 0;
                    Debug.Log("This used to be = 1");
                    yColumn++;
                }

                for (int x = 0; x < textures[i].width; x++)
                {
                    for (int y = 0; y < textures[i].height; y++)
                    {
                        xPixelPos = xRow * textures[0].width + x;
                        yPixelPos = yColumn * textures[0].height + y;
                        if (i == 0)
                        {
                            Debug.Log("check this line");
                            //Debug.Log(yPixelPos.ToString());
                        }

                        returnTexture.SetPixel(xPixelPos, yPixelPos, textures[i].GetPixel(x, y));
                    }
                }
            }

            numberOfTextures = new Vector2(dimensions.x, dimensions.y);
             
            return returnTexture;
        }

        private static bool CheckTextureSizes(Texture2D[] textures)
        {
            Vector2 lastDimension = new Vector2(textures[0].width, textures[0].height);

            for (int i = 0; i < textures.Length; i++)
            {
                if (textures[i].width == lastDimension.x && textures[i].height == lastDimension.y)
                {
                    lastDimension.x = textures[i].width;
                    lastDimension.y = textures[i].height;
                    continue;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}