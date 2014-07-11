using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using CivGrid;

namespace CivGrid
{

    /// <summary>
    /// Saves and loads numerious data types
    /// </summary>
    public static class CivGridSaver
    {
        public static void SaveTexture(Texture2D texture, string name, bool openTextureToView)
        {
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + "/../" + name + ".png", bytes);
            if (openTextureToView) { Application.OpenURL(Application.dataPath + "/../" + name + ".png"); }
            AssetDatabase.Refresh();
        }

        public static void SaveTexture(Texture2D texture, string location, string name, bool openTextureToView)
        {
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(location + "/" + name + ".png", bytes);
            if (openTextureToView) { Application.OpenURL(location + "/" + name + ".png"); }
            AssetDatabase.Refresh();
        }

        public static void SaveTexture(byte[] texture, string location, string name, bool openTextureToView)
        {
            File.WriteAllBytes(location + "/" + name + ".png", texture);
            if (openTextureToView) { Application.OpenURL(location + "/" + name + ".png"); }
            AssetDatabase.Refresh();
        }

        public static void SaveTerrain(string name, WorldManager worldManager)
        {
            System.Text.StringBuilder assetPrefix = new System.Text.StringBuilder(Application.dataPath);
            assetPrefix.Remove((assetPrefix.Length - 6), 6);

            using (XmlWriter writer = XmlWriter.Create(name + ".xml"))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Root");

                #region WorldManager
                writer.WriteStartElement("WorldManager");

                writer.WriteElementString("useCivGridCamera", XmlConvert.ToString(worldManager.useCivGridCamera));
                writer.WriteElementString("usePresetWorldValue", XmlConvert.ToString(worldManager.useWorldTypeValues));
                writer.WriteElementString("worldType", ((int)worldManager.worldType).ToString());
                writer.WriteElementString("mapSizeX", worldManager.mapSize.x.ToString());
                writer.WriteElementString("mapSizeY", worldManager.mapSize.y.ToString());
                writer.WriteElementString("chunkSize", worldManager.chunkSize.ToString());
                writer.WriteElementString("hexRadiusSize", worldManager.hexRadiusSize.ToString());
                writer.WriteElementString("mountainMapLocation", (assetPrefix.ToString()  + AssetDatabase.GetAssetPath(worldManager.mountainMap)));

                writer.WriteEndElement();
                #endregion

                #region TileManager

                TileManager tileManager = worldManager.tileManager;

                writer.WriteStartElement("TileManager");

                writer.WriteStartElement("Tiles");
                for (int i = 0; i < tileManager.tiles.Count; i++)
                {
                    writer.WriteStartElement("NewTile");

                    writer.WriteAttributeString("name", tileManager.tiles[i].name);
                    writer.WriteAttributeString("bottomLat", XmlConvert.ToString(tileManager.tiles[i].bottomLat));
                    writer.WriteAttributeString("topLat", XmlConvert.ToString(tileManager.tiles[i].topLat));
                    writer.WriteAttributeString("isShore", XmlConvert.ToString(tileManager.tiles[i].isShore));
                    writer.WriteAttributeString("isOcean", XmlConvert.ToString(tileManager.tiles[i].isOcean));
                    writer.WriteAttributeString("isMountain", XmlConvert.ToString(tileManager.tiles[i].isMountain));

                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                writer.WriteEndElement();

                #endregion

                #region ResourceManager

                ResourceManager resourceManager = worldManager.resourceManager;

                writer.WriteStartElement("ResourceManager");

                    writer.WriteStartElement("Resources");
                        for (int i = 1; i < resourceManager.resources.Count; i++)
                        {
                            writer.WriteStartElement("NewResource");

                                writer.WriteAttributeString("name", resourceManager.resources[i].name);
                                writer.WriteAttributeString("rarity", resourceManager.resources[i].rarity.ToString());
                                writer.WriteAttributeString("spawnAmount", resourceManager.resources[i].spawnAmount.ToString());
                            Debug.Log(resourceManager.resources[i].meshToSpawn);
                            Debug.Log(AssetDatabase.GetAssetPath(resourceManager.resources[i].meshToSpawn));
                                writer.WriteAttributeString("meshToSpawn", (AssetDatabase.GetAssetPath(resourceManager.resources[i].meshToSpawn)));
                                writer.WriteAttributeString("meshTexture", (AssetDatabase.GetAssetPath(resourceManager.resources[i].meshTexture)));
                                writer.WriteAttributeString("replaceGroundTexture", XmlConvert.ToString(resourceManager.resources[i].replaceGroundTexture));

                                writer.WriteAttributeString("numOfPossibleTiles", resourceManager.resources[i].rule.possibleTiles.Length.ToString());

                                for (int y = 0; y < resourceManager.resources[i].rule.possibleTiles.Length; y++)
                                {
                                    writer.WriteAttributeString("possibleTile" + y, resourceManager.resources[i].rule.possibleTiles[y].ToString());
                                }

                                writer.WriteAttributeString("numOfPossibleFeatures", resourceManager.resources[i].rule.possibleFeatures.Length.ToString());

                                for (int y = 0; y < resourceManager.resources[i].rule.possibleFeatures.Length; y++)
                                {
                                    writer.WriteAttributeString("possibleFeature" + y, resourceManager.resources[i].rule.possibleFeatures[y].ToString());
                                }

                            writer.WriteEndElement();
                        }
                    writer.WriteEndElement();

                writer.WriteEndElement();

                #endregion

                #region ImprovementManager

                ImprovementManager improvementManager = worldManager.improvementManager;

                writer.WriteStartElement("ImprovementManager");

                writer.WriteElementString("improvementCount", improvementManager.searalizableImprovements.Count.ToString());

                writer.WriteStartElement("Improvements");
                for (int i = 1; i < improvementManager.searalizableImprovements.Count; i++)
                {
                    writer.WriteStartElement("Improvement");

                    writer.WriteElementString("name", improvementManager.searalizableImprovements[i].name);
                    writer.WriteElementString("meshToSpawn", (assetPrefix.ToString() + AssetDatabase.GetAssetPath(improvementManager.searalizableImprovements[i].meshToSpawn)));
                    writer.WriteElementString("meshTexture", (assetPrefix.ToString() + AssetDatabase.GetAssetPath(improvementManager.searalizableImprovements[i].meshTexture)));
                    writer.WriteElementString("replaceGroundTexture", improvementManager.searalizableImprovements[i].replaceGroundTexture.ToString());

                    writer.WriteStartElement("ImprovementRule");

                    writer.WriteStartElement("PossibleTiles");
                    for (int y = 0; y < improvementManager.searalizableImprovements[i].rule.possibleTiles.Length; y++)
                    {
                        writer.WriteStartElement("Tile");
                        writer.WriteElementString("tile", improvementManager.searalizableImprovements[i].rule.possibleTiles[i].ToString());
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    writer.WriteStartElement("PossibleFeatures");
                    for (int y = 0; y < improvementManager.searalizableImprovements[i].rule.possibleFeatures.Length; y++)
                    {
                        writer.WriteStartElement("Feature");
                        writer.WriteElementString("feature", improvementManager.searalizableImprovements[i].rule.possibleFeatures[i].ToString());
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    writer.WriteEndElement();

                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                writer.WriteEndElement();

                #endregion

                #region CivGridCamera

                writer.WriteStartElement("CivGridCamera");

                //camera settings
                writer.WriteElementString("enableWrapping", XmlConvert.ToString(worldManager.civGridCamera.enableWrapping));
                writer.WriteElementString("cameraHeight", worldManager.civGridCamera.cameraHeight.ToString());
                writer.WriteElementString("cameraAngle", worldManager.civGridCamera.cameraAngle.ToString());
                writer.WriteElementString("cameraSpeed", worldManager.civGridCamera.cameraSpeed.ToString());

                writer.WriteEndElement();

                #endregion


                foreach (HexChunk chunk in worldManager.hexChunks)
                {
                    #region Chunk

                    writer.WriteStartElement("Chunk");

                    foreach (HexInfo hex in chunk.hexArray)
                    {
                        #region Hex

                        writer.WriteStartElement("Hexagon");

                        writer.WriteElementString("Type", hex.terrainType.name);

                        writer.WriteElementString("Feature", ((int)hex.terrainFeature).ToString());

                        writer.WriteElementString("Resource", hex.currentResource.name);

                        writer.WriteElementString("Improvement", hex.currentImprovement.name);

                        writer.WriteEndElement();

                        #endregion
                    }
                    writer.WriteEndElement();

                    #endregion
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        public static void LoadTerrain(string location, WorldManager worldManager)
        {
            TileManager tileManager = worldManager.tileManager;
            ResourceManager resourceManager = worldManager.resourceManager;

            tileManager.tiles.Clear();
            resourceManager.resources.Clear();

            using (XmlReader reader = XmlReader.Create(location + ".xml"))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {
                            case "Root":
                                Debug.Log("opening root");
                                break;
                            #region WorldManager
                            case "WorldManager":
                                Debug.Log("opening WorldManager");
                                break;
                            case "useCivGridCamera":
                                worldManager.useCivGridCamera = XmlConvert.ToBoolean(reader.ReadString());
                                break;
                            case "usePresetWorldValue":
                                worldManager.useWorldTypeValues = XmlConvert.ToBoolean(reader.ReadString());
                                break;
                            case "worldType":
                                worldManager.worldType = (WorldType)XmlConvert.ToInt32(reader.ReadString());
                                break;
                            case "mapSizeX":
                                worldManager.mapSize = new Vector2(XmlConvert.ToInt32(reader.ReadString()), worldManager.mapSize.y);
                                break;
                            case "mapSizeY":
                                worldManager.mapSize = new Vector2(worldManager.mapSize.x, XmlConvert.ToInt32(reader.ReadString()));
                                break;
                            case "chunkSize":
                                worldManager.chunkSize = XmlConvert.ToInt32(reader.ReadString());
                                break;
                            case "hexRadiusSize":
                                worldManager.hexRadiusSize = (float)XmlConvert.ToDouble(reader.ReadString());
                                break;
                            case "mountainMapLocation":
                                string loc = reader.ReadString();
                                worldManager.mountainMap = (Texture2D)AssetDatabase.LoadAssetAtPath(((string)loc.Clone()).Remove(0, loc.IndexOf("Assets")), typeof(Texture2D));
                                break;
                            #endregion
                            #region TileManager
                            case "TileManager":
                                Debug.Log("opening tile manager");
                                break;
                            case "Tiles":
                                Debug.Log("finding tiles");
                                break;
                            case "NewTile":
                                Debug.Log("adding Tile: " + reader["name"]);
                                tileManager.AddTile(new Tile(reader["name"], XmlConvert.ToBoolean(reader["isShore"]), XmlConvert.ToBoolean(reader["isOcean"]), XmlConvert.ToBoolean(reader["isMountain"]), (float)XmlConvert.ToDouble(reader["bottomLat"]), (float)XmlConvert.ToDouble(reader["topLat"])));
                                break;
                            #endregion
                            #region ResourceManager
                            case "ResourceManager":
                                Debug.Log("opening resource manager");
                                break;
                            case "Resources":
                                Debug.Log("opening resources");
                                break;
                            case "NewResource":
                                string loc1 = reader["meshToSpawn"];
                                string loc2 = reader["meshTexture"];

                                List<int> possibleTiles = new List<int>();
                                List<Feature> possibleFeatures = new List<Feature>();

                                for(int i = 0; i < XmlConvert.ToInt32(reader["numOfPossibleTiles"]); i++)
                                {
                                    possibleTiles.Add(XmlConvert.ToInt32(reader["possibleTile" + i]));
                                }

                                for (int i = 0; i < XmlConvert.ToInt32(reader["numOfPossibleFeatures"]); i++)
                                {
                                    possibleFeatures.Add((Feature)System.Enum.Parse(typeof(Feature), reader["possibleFeature" + i]));
                                }

                                resourceManager.resources.Add(new Resource(
                                    reader["name"],
                                    ((float)XmlConvert.ToDouble(reader["rarity"])),
                                    XmlConvert.ToInt32(reader["spawnAmount"]),
                                    ((Mesh)AssetDatabase.LoadAssetAtPath(loc1, typeof(Mesh))),
                                    ((Texture2D)AssetDatabase.LoadAssetAtPath(loc2, typeof(Texture2D))),
                                    XmlConvert.ToBoolean(reader["replaceGroundTexture"]), new ResourceRule(possibleTiles.ToArray(), possibleFeatures.ToArray())));
                                break;
                            #endregion
                            default:
                                //Debug.Log("unhandled exception");
                                break;

                        }
                    }
                }
            }
        }

        public static Texture2D LoadTexture(string location)
        {
            byte[] bytes;
            bytes = File.ReadAllBytes(location);
            Texture2D tex = new Texture2D(0, 0);

            tex.LoadImage(bytes);
            return tex;
        }
    }
}