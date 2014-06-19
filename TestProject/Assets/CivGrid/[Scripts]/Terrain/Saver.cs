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
            using (XmlWriter writer = XmlWriter.Create(name + ".xml"))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Start");

                #region WorldManager
                writer.WriteStartElement("WorldManager");

                //camera settings
                writer.WriteStartElement("enableWrapping");
                writer.WriteValue(manager.civGridCamera.enableWrapping);
                writer.WriteEndElement();

                writer.WriteStartElement("keepSymmetrical");
                writer.WriteValue(manager.keepSymmetrical);
                writer.WriteEndElement();

                writer.WriteStartElement("cameraHeight");
                writer.WriteValue(manager.civGridCamera.cameraHeight);
                writer.WriteEndElement();

                writer.WriteStartElement("cameraAngle");
                writer.WriteValue(manager.civGridCamera.cameraAngle);
                writer.WriteEndElement();

                writer.WriteStartElement("cameraSpeed");
                writer.WriteValue(manager.civGridCamera.cameraSpeed);
                writer.WriteEndElement();

                writer.WriteElementString("worldType", manager.worldType.ToString());

                writer.WriteStartElement("mapSizeX");
                writer.WriteValue(manager.mapSize.x);
                writer.WriteEndElement();

                writer.WriteStartElement("mapSizeY");
                writer.WriteValue(manager.mapSize.y);
                writer.WriteEndElement();

                writer.WriteStartElement("chunkSize");
                writer.WriteValue(manager.chunkSize);
                writer.WriteEndElement();

                writer.WriteEndElement();
                #endregion


                foreach (HexChunk chunk in manager.hexChunks)
                {
                    writer.WriteStartElement("Chunk");

                    //xLoc
                    writer.WriteStartElement("xSector");
                    writer.WriteValue(chunk.xSector);
                    writer.WriteEndElement();
                    //yLoc
                    writer.WriteStartElement("ySector");
                    writer.WriteValue(chunk.ySector);
                    writer.WriteEndElement();

                    writer.WriteEndElement();

                    foreach (HexInfo hex in chunk.hexArray)
                    {
                        writer.WriteStartElement("Hexagon");


                        writer.WriteStartElement("gridPositionX");
                        writer.WriteValue(hex.CubeGridPosition.x);
                        writer.WriteEndElement();

                        writer.WriteStartElement("Buffer");
                        writer.WriteEndElement();

                        writer.WriteStartElement("gridPositionY");
                        writer.WriteValue(hex.CubeGridPosition.y);
                        writer.WriteEndElement();

                        writer.WriteElementString("Type", (hex.terrainType.ToString()));

                        writer.WriteElementString("Feature", ((int)hex.terrainFeature).ToString());

                        writer.WriteEndElement();
                    }
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