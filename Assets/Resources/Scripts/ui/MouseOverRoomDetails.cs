using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MouseOverRoomDetails : MonoBehaviour {
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

        if (t != null && t.room != null && t.world.rooms.IndexOf(t.room) >= 0)
        {
            string s = "";
            foreach (string g in t.room.GetGasNames())
            {
                s += g + ": " + t.room.GetGasAmount(g) + " (" + t.room.GetGasPercentage(g)*100 + "%) \n";
            }
            myText.text = s;
            return;
        }
        myText.text = "No gases present.";
    }
}
