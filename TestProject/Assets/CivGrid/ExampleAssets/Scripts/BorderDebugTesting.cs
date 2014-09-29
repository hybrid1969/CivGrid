using UnityEngine;
using System.Collections;
using CivGrid;

public class BorderDebugTesting : MonoBehaviour
{

    [SerializeField]
    public WorldManager worldManager;

    void Start()
    {
        WorldManager.onHexClick += OnHexClick;
    }

    void OnHexClick(Hex hex, int mouseButton)
    {
        if (mouseButton == 0)
        {
            hex.BorderID = 0;
        }
        if (mouseButton == 1)
        {
            hex.BorderID = 1;
        }
    }
}