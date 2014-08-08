/// using System;
/// using UnityEngine;
/// using CivGrid;
///
/// public class ExampleClass : MonoBehaviour
/// {
///    TileManager tileManager;
///
///    void Start()
///    {
///        tileManager = GameObject.FindObjectOfType<TileManager>();
///
///        //this method is not encouraged, used as a specific example and not best practice. Add tiles in the
///        //inspector instead.
///        tileManager.AddTile(new Tile("Shore", true, false, true));
///
///        Tile tile = tileManager.TryGetShore();
///
///        Debug.Log(tile.name);
///    }
/// }
///
/// //Output:
/// //"Shore"


