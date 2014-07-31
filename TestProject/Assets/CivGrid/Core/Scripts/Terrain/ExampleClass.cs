/// using System;
/// using UnityEngine;
/// using CivGrid;
///
/// class ExampleClass : MonoBehaviour
/// {
///    WorldManager worldManager;
///
///    public void Start()
///    {
///        //cache and find the world manager
///        worldManager = GameObject.FindObjectOfType<WorldManager>();
///
///        //gets the very middle hexagon in the map
///        HexChunk chunk = worldManager.hexChunks[worldManager.hexChunks.GetLength(0) / 2, worldManager.hexChunks.GetLength(1) / 2];
///        HexInfo hex = chunk.hexArray[chunk.hexArray.GetLength(0) / 2, chunk.hexArray.GetLength(1) / 2];
///
///        //change the hexes texture to the resource version
///        hex.ChangeTextureToResource();
///
///        //update the chunk mesh to apply the changes
///        hex.ApplyChanges();
///    }
/// }
