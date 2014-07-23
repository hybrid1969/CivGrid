using UnityEngine;
using System.Collections;
using CivGrid;

namespace CivGrid
{
    /// <summary>
    /// Contains all possible tiles and features for the improvement to spawn on
    /// </summary>
    [System.Serializable]
    public class HexRule
    {
        //array of tiles that the improvement can spawn on
        public int[] possibleTiles;
        //array of the features that the improvement can spawn on
        public Feature[] possibleFeatures;

        public HexRule(int[] possibleTiles, Feature[] possibleFeatures)
        {
            this.possibleTiles = possibleTiles;
            this.possibleFeatures = possibleFeatures;
        }
    }

    public static class RuleTest
    {
        /// <summary>
        /// Checks all rules in the rule list.
        /// </summary>
        /// <param name="hex">Hex to compare the rules upon</param>
        /// <param name="rule">Rules to check</param>
        /// <returns>If the hex passed the tests</returns>
        public static bool Test(HexInfo hex, HexRule rule, TileManager tileManager)
        {
            //check tile rules
            for (int i = 0; i < rule.possibleTiles.Length; i++)
            {
                //if the hex's tile type is in the list of possible tiles, break out of the loop and check features
                if (TestRule(hex, tileManager.tiles[rule.possibleTiles[i]]) == true) { break; }
                //the hex's tile type was not in the list of possible tiles, return false 
                if (i == (rule.possibleTiles.Length - 1)) { return false; }
            }
            //check feature rules
            for (int i = 0; i < rule.possibleFeatures.Length; i++)
            {
                //if hex's feature type is in the list of possible features, return true since both rules have been passed
                if (TestRule(hex, rule.possibleFeatures[i]) == true) { return true; }
                //the hex's feature type was not in the list of possible features, return false 
                if (i == (rule.possibleFeatures.Length - 1)) { return false; }
            }
            //unreachable mandatory code because c# is funky
            return false;
        }

        /// <summary>
        /// Check if the hex's tile type is the provided tile
        /// </summary>
        /// <param name="hex">Hex to compare to the tile</param>
        /// <param name="tile">Tile to compare to the hex</param>
        /// <returns></returns>
        private static bool TestRule(HexInfo hex, Tile tile)
        {
            if (hex.terrainType == tile) { return true; } else { return false; }
        }

        /// <summary>
        /// Check if the hex's feature type is the provided feature
        /// </summary>
        /// <param name="hex">Hex to compare to the feature</param>
        /// <param name="feature">Feature to compare to the hex</param>
        /// <returns></returns>
        private static bool TestRule(HexInfo hex, Feature feature)
        {
            if (hex.terrainFeature == feature) { return true; } else { return false; }
        }
    }
}