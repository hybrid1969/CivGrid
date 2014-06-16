using UnityEngine;
using System.Collections;
using CivGrid;

namespace CivGrid
{
    public static class TexturePacker
    {
        public static Texture2D AtlasTextures(Texture2D[] textures, out Rect[] rectAreas)
        {
            Texture2D packedTexture = new Texture2D(1024, 1024);

            rectAreas = packedTexture.PackTextures(textures, 5, 2048);
            packedTexture.Apply();

            return packedTexture;
        }
    }
}