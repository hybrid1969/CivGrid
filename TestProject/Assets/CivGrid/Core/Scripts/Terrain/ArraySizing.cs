using System;
using UnityEngine;
using CivGrid;
using CivGrid.SampleResources;

/// class ArraySizing : MonoBehaviour
/// {
///   float[,] randomValues2D = new float[3,2] { { 0, 1 }, { 2, 3 }, { 4, 5 } };
///
///    void Start()
///    {
///        Debug.Log("Array Size Before: " + randomValues2D.Length);
///        randomValues2D = CivGridUtility.Resize2DArray<float>(randomValues2D, 5, 2);
///        Debug.Log("Array Size After: " + randomValues2D.Length);
///    }
///    //Output:
///    //randomValues2D = { { 0, 1 }, { 2, 3 }, { 4, 5 }, { 0, 0 }, { 0, 0 } };
///    //Array Size Before: 6
///    //Array Size After: 10
/// }