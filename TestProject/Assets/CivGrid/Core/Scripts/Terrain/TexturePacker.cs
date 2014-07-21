using UnityEngine;
using System.Collections;
using CivGrid;

namespace CivGrid
{
    /// <summary>
    /// Packs textures into atlases.
    /// </summary>
    public static class TexturePacker
    {
        /// <summary>
        /// Creates a single texture atlas from the provided source textures.
        /// </summary>
        /// <param name="textures">Textures to combine into one</param>
        /// <param name="rectAreas">Rect locations of each texture</param>
        /// <returns>The created atlased texture</returns>
        public static Texture2D AtlasTextures(Texture2D[] textures, int textureSize, out Rect[] rectAreas)
        {
            //creates return texture atlas
            Texture2D packedTexture = new Texture2D(textureSize, textureSize);

            //packs all source textures into one
            rectAreas = packedTexture.PackTextures(textures, 0, textureSize);
            packedTexture.Apply();

            //returns texture atlas
            return packedTexture;
        }
    }
}