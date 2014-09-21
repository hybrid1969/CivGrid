﻿using UnityEngine;
using System.Collections;
using CivGrid;

public class BorderDebugTesting : MonoBehaviour {

	[SerializeField] public WorldManager worldManager;

	void Start () {
	
		WorldManager.onHexClick += OnHexClick;

	}

	void OnHexClick(CustomHex hex, int mouseButton)
	{
		if(mouseButton == 0){

			hex.ownedByTeam = 0;
			RefreshAffectedBorders( hex );

		}
		if(mouseButton == 1)
		{

			hex.ownedByTeam = 1;
			RefreshAffectedBorders( hex );

		}
	}

	private void RefreshAffectedBorders( CustomHex byChangeInHex ) {
		
		// Make the world manager fresh chunks affected by this update.
		
		// TODO : This is highly inefficient if updating multiple tiles' border values at once. Should figure out a different way to
		//        refresh the tiles maybe?

		worldManager.RefreshBorders( byChangeInHex );

	}
}
