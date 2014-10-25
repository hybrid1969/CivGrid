using UnityEngine;
using System.Collections;
using UnityEditor;
using CivGrid;

namespace CivGrid.Debugging
{
    public class EditorDebugger
    {
        [MenuItem("CivGrid/Debug/GenerateRainfallMap")]
        private static void TestRainfallGeneration()
        {
            Texture2D tex = NoiseGenerator.PerlinNoiseRaw(1024, 1024, 8);

            FileUtility.SaveTexture(tex, "test", true);
        }
    }
}