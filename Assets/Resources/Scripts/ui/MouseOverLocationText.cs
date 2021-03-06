﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MouseOverLocationText : MonoBehaviour {
    private MouseController mouseController;
    Text myText;

	void Start () {
        myText = this.GetComponent<Text>();

        if (myText == null) {
            Debug.LogError("MouseOverTileTypeText: No \"Text\" UI Component on this object");
            this.enabled = false;
            return;
        }

        mouseController = GameObject.FindObjectOfType<MouseController>();
        if (mouseController == null) {
            Debug.LogError(" WHY IS THERE NO MOUSE CONTROLLER");
            return;
        }
	}
	
	// Update is called once per frame
	void Update () {
        Tile t = mouseController.GetMouseOverTile();
        if ( t != null) {
            myText.text = "Location: " + t.X + "," + t.Y;
        }
	}
}
