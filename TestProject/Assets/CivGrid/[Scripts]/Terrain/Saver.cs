using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Xml;
using CivGrid;

namespace CivGrid
{

    /// <summary>
    /// Saves and loads numerious data types
    /// </summary>
    public static class Saver
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

        public static void SaveTerrain(string name, WorldManager manager)
        {
            System.Text.StringBuilder assetPrefix = new System.Text.StringBuilder(Application.dataPath);
            assetPrefix.Remove((assetPrefix.Length - 6), 6);

            using (XmlWriter writer = XmlWriter.Create(name + ".xml"))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Root");

                #region WorldManager
                writer.WriteStartElement("WorldManager");

                writer.WriteElementString("keepSymmetrical", manager.keepSymmetrical.ToString());
                writer.WriteElementString("useCivGridCamera", manager.useCivGridCamera.ToString());
                writer.WriteElementString("usePresetWorldValue", manager.useWorldTypeValues.ToString());
                writer.WriteElementString("worldType", manager.worldType.ToString());
                writer.WriteElementString("mapSizeX", manager.mapSize.x.ToString());
                writer.WriteElementString("mapSizeY", manager.mapSize.y.ToString());
                writer.WriteElementString("chunkSize", manager.chunkSize.ToString());
                writer.WriteElementString("hexRadiusSize", manager.hexRadiusSize.ToString());
                writer.WriteElementString("mountainMapLocation", (assetPrefix.ToString()  + AssetDatabase.GetAssetPath(manager.mountainMap)));

                writer.WriteEndElement();
                #endregion

                #region TileManager

                TileManager tileManager = manager.tileManager;

                writer.WriteStartElement("TileManager");

                writer.WriteElementString("tileCount", tileManager.tiles.Count.ToString());

                writer.WriteStartElement("Tiles");
                for (int i = 0; i < tileManager.tiles.Count; i++)
                {
                    writer.WriteStartElement("Tile");

                    writer.WriteElementString("name", tileManager.tiles[i].name);
                    writer.WriteElementString("isShore", tileManager.tiles[i].isShore.ToString());
                    writer.WriteElementString("isOcean", tileManager.tiles[i].isOcean.ToString());
                    writer.WriteElementString("isMountain", tileManager.tiles[i].isMountain.ToString());

                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                writer.WriteEndElement();

                #endregion

                //finish this
                #region ResourceManager

                ResourceManager resourceManager = manager.resourceManager;

                writer.WriteStartElement("ResourceManager");

                writer.WriteElementString("resourceCount", resourceManager.resources.Count.ToString());

                writer.WriteStartElement("Resources");
                for (int i = 0; i < resourceManager.resources.Count; i++)
                {
                    writer.WriteStartElement("Resource");

                    writer.WriteElementString("name", resourceManager.resources[i].name);
                    writer.WriteElementString("rarity", resourceManager.resources[i].rarity.ToString());
                    writer.WriteElementString("spawnAmount", resourceManager.resources[i].spawnAmount.ToString());

                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                writer.WriteEndElement();

                #endregion

                #region CivGridCamera

                writer.WriteStartElement("CivGridCamera");

                //camera settings
                writer.WriteElementString("enableWrapping", manager.civGridCamera.enableWrapping.ToString());
                writer.WriteElementString("cameraHeight", manager.civGridCamera.cameraHeight.ToString());
                writer.WriteElementString("cameraAngle", manager.civGridCamera.cameraAngle.ToString());
                writer.WriteElementString("cameraSpeed", manager.civGridCamera.cameraSpeed.ToString());

                writer.WriteEndElement();

                #endregion


                foreach (HexChunk chunk in manager.hexChunks)
                {
                    #region Chunk

                    writer.WriteStartElement("Chunk");

                    foreach (HexInfo hex in chunk.hexArray)
                    {
                        #region Hex

                        writer.WriteStartElement("Hexagon");

                        writer.WriteElementString("Type", (hex.terrainType.ToString()));

                        writer.WriteElementString("Feature", ((int)hex.terrainFeature).ToString());

                        writer.WriteElementString("Resource", hex.currentResource.ToString());

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

            //LoadTerrain(name, manager);
        }

        public static void LoadTerrain(string name, WorldManager manager)
        {
            //hexagon data buffer
            float xLoc;
            float yLoc = 0;
            string typeName;
            Feature feature;

            using (XmlTextReader reader = new XmlTextReader(name + ".xml"))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {
                            #region WorldLoading
                            case "Chunk":
                                //Debug.Log("opening new chunk");
                                break;
                            case "enableWrapping":
                                //Debug.Log(" before enableWrapping is " + manager.enableWrapping);
                                manager.civGridCamera.enableWrapping = (bool)reader.ReadElementContentAs(typeof(bool), null);
                                //Debug.Log(" after enableWrapping is " + manager.enableWrapping);
                                break;
                            case "keepSymmetrical":
                                manager.keepSymmetrical = (bool)reader.ReadElementContentAs(typeof(bool), null);
                                break;
                            case "cameraHeight":
                                manager.civGridCamera.cameraHeight = (float)reader.ReadElementContentAs(typeof(float), null);
                                break;
                            case "cameraAngle":
                                Debug.Log("lo");
                                manager.civGridCamera.cameraAngle = (float)reader.ReadElementContentAs(typeof(float), null);
                                break;
                            case "cameraSpeed":
                                //Debug.Log("lo");
                                manager.civGridCamera.cameraSpeed = (float)reader.ReadElementContentAs(typeof(float), null);
                                break;
                            #endregion

                            #region HexagonLoading
                            case "Hexagon":
                                // Debug.Log("opening new hexagon");
                                break;
                            case "gridPositionY":
                                yLoc = (float)reader.ReadElementContentAs(typeof(float), null);
                                //Debug.Log("Y: " + yLoc);
                                break;
                            case "gridPositionX":
                                xLoc = (float)reader.ReadElementContentAs(typeof(float), null);
                                //Debug.Log("X: " + xLoc);
                                break;
                            case "Type":
                                typeName = reader.ReadString();
                                //Debug.Log("Type: " + type);
                                break;
                            case "Feature":
                                feature = ((Feature)(XmlConvert.ToInt32(reader.ReadString())));
                                //Debug.Log("Feature: " + feature);

                                break;
                            #endregion

                            default:
                                //Debug.Log(yLoc);
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