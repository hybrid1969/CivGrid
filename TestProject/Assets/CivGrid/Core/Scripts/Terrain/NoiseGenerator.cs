using UnityEngine;
using System.Collections;
using CivGrid;

namespace CivGrid
{


    /// <summary>
    /// Makes and cleans noise for world generation
    /// </summary>
    public static class NoiseGenerator
    {

        /// <summary>
        /// A perlin noise generator
        /// </summary>
        /// <param name="xSize">Amount of tiles in the x-axis of the map </param>
        /// <param name="ySize">Amount of tiles in the y-axis of the map </param>
        /// <param name="noiseScale">Scale of the noise </param>
        /// <returns> TileMap in Texture2D format </returns>
        public static Texture2D PerlinNoise(int xSize, int ySize, float noiseScale)
        {
            //texture to return
            Texture2D tex = new Texture2D(xSize, ySize);

            //loop through all pixels
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    //calculate a modified version of perlin noise and assign its value to the pixel
                    float randomValue = Random.Range(0, 3);
                    float pixelValue = Mathf.PerlinNoise(x * noiseScale + randomValue, y * noiseScale + randomValue) * 1.3f;

                    pixelValue = Mathf.RoundToInt(pixelValue);
                    tex.SetPixel(x, y, new Color(pixelValue, pixelValue, pixelValue, 1));
                }
            }

            //return completed perlin noise texture
            return tex;
        }


        /// <summary>
        /// An inverted version of perlin noise with ocean smoothing
        /// </summary>
        /// <param name="xSize">Amount of tiles in the x-axis of the map</param>
        /// <param name="ySize">Amount of tiles in the y-axis of the map</param>
        /// <param name="noiseScale">Scale of the noise</param>
        /// <param name="smoothingCutoff">Amount of tiles needed to remain water/ground</param>
        /// <returns>A TileMap in Texture2D format</returns>
        public static Texture2D SmoothPerlinNoise(int xSize, int ySize, float noiseScale, int smoothingCutoff)
        {
            //texture to return
            Texture2D tex = new Texture2D(xSize, ySize);

            //loop through all the pixels
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    //calculate a smooth modified version of perlin noise and assign the value to the pixel
                    float randomValue = Random.value;
                    float pixelValue = Mathf.PerlinNoise(x * noiseScale + randomValue, y * noiseScale + randomValue);

                    if (pixelValue >= 0.5f) { pixelValue *= 1.35f; }

                    pixelValue = StabalizeFloat(pixelValue);

                    tex.SetPixel(x, y, new Color(pixelValue, pixelValue, pixelValue, 1));
                }
            }

            //smooth the land vs water
            CleanWater(tex, noiseScale, smoothingCutoff);

            //return perlin noise texture
            return tex;
        }

        /// <summary>
        /// Smooths perlin noise to generate more realistic and smooth terrain
        /// </summary>
        /// <param name="texture">Texture to smooth</param>
        /// <param name="noiseScale">Noise scale you used to generate the texture</param>
        /// <param name="smoothingCutoff">The number of like tiles surronding a pixel needed in order for it to remain the original tupe</param>
        private static void CleanWater(Texture2D texture, float noiseScale, int smoothingCutoff)
        {
            //loop through all pixels in the texture
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    //get the pixels around this pixel
                    float[] surrondingTiles = Utility.GetSurrondingPixels(texture, x, y);

                    //WATER
                    if (texture.GetPixel(x, y).r == 0)
                    {
                        //amount of water tiles around this tile
                        int surrondingWater = 0;

                        foreach (float tile in surrondingTiles)
                        {
                            if (tile.Equals(0f))
                            {
                                surrondingWater += 1;
                            }
                        }

                        //not enough water around; set to land
                        if (surrondingWater < smoothingCutoff)
                        {
                            //generate noise for the pixel we are setting to land
                            float randomValue = Random.value;
                            float pixelValue = Mathf.PerlinNoise(x * noiseScale + randomValue, y * noiseScale + randomValue);
                            pixelValue = StabalizeFloat(pixelValue);

                            if (pixelValue < 0.5)
                            {
                                pixelValue = 0.5f;
                            }

                            texture.SetPixel(x, y, new Color(pixelValue, pixelValue, pixelValue));
                        }
                    }
                    //LAND
                    else
                    {
                        //amount of water tiles around this tile
                        int surrondingLand = 0;

                        foreach (float tile in surrondingTiles)
                        {
                            if (tile > 0f)
                            {
                                surrondingLand += 1;
                            }
                        }

                        //not enough land around; set to water
                        if (surrondingLand < smoothingCutoff)
                        {
                            texture.SetPixel(x, y, new Color(0f, 0f, 0f));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Uses internal logic to round floats to usable values(0,0.5,0.8,1)
        /// </summary>
        /// <param name="f">float to round</param>
        /// <returns>The rounded float</returns>
        public static float StabalizeFloat(float f)
        {
            //round to water
            if (f < 0.5f)
            {
                f = 0;
                return f;
            }
            //round to land
            else if (f >= 0.5 && f <= 1)
            {
                f = 0.5f;
                return f;
            }
            //round to hill
            else if (f > 1f && f < 1.15f)
            {
                f = 0.8f;
                return f;
            }
            //round to mountain
            else if (f >= 1.15f) { f = 1f; }
            else { Debug.LogError("Rounding failed inverting to 0"); f = 0; }

            return f;
        }

        /// <summary>
        /// Overlays a texture with perlin noise; formated for mountain generation
        /// </summary>
        /// <param name="texture">Texture to add perlin noise onto</param>
        /// <param name="position">Pixel position to read the perlin noise from</param>
        /// <param name="noiseScale">Controls amount of possible change from pixel-to-pixel</param>
        /// <param name="noiseSize">Scales the value of the noise pixel</param>
        /// <param name="finalSize">Scales the value of the final pixel</param>
        /// <param name="maxHeight">Maximum height the final pixel can be</param>
        /// <param name="ignoreBlack">Avoids adding noise to fully black pixels of the source texture</param>
        /// <param name="noiseFalloff">Whether or not to use smooth noise falloff</param>
        /// <returns></returns>
        public static Texture2D RandomOverlay(Texture2D texture, float position, float noiseScale, float noiseSize, float finalSize, float maxHeight, bool ignoreBlack, bool noiseFalloff)
        {
            //shifts the source over
            position *= Random.Range(0.2f, 5f);

            //creates the return texture
            Texture2D returnTexture = new Texture2D(texture.width, texture.height);

            //loop through all pixels in the source texture
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    //get the pixel from the source texture
                    float pixelColor = texture.GetPixel(x, y).grayscale;

                    float noiseValue;

                    //overlay noise upon the pixel
                    if (ignoreBlack == false || pixelColor > 0.05f)
                    {
                        if (noiseFalloff)
                        {
                            noiseValue = (Mathf.PerlinNoise(x * noiseScale + position, y * noiseScale + position) * noiseSize) * (pixelColor * 2);
                        }
                        else
                        {
                            noiseValue = Mathf.PerlinNoise(x * noiseScale + position, y * noiseScale + position) * noiseSize;
                        }
                    }
                    //no overlay noise
                    else
                    {
                        noiseValue = 0;
                    }

                    //add the base pixel and the noise value to make a combined pixel color
                    float finalValue = pixelColor + noiseValue;
                    finalValue = Mathf.Clamp(finalValue, 0, maxHeight);
                    finalValue *= finalSize;
                    Color totalColor = new Color(finalValue, finalValue, finalValue, 1);

                    returnTexture.SetPixel(x, y, totalColor);
                }
            }

            //return the source texture with noise overlay
            return returnTexture;
        }
    }
}