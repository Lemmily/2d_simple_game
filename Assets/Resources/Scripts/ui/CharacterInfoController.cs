using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class CharacterInfoController : MonoBehaviour {
    private Text myText;

    void Start()
    {
        myText = this.GetComponent<Text>();

        if (myText == null)
        {
            Debug.LogError("MouseOverTileTypeText: No \"Text\" UI Component on this object");
            this.enabled = false;
            return;
        }

    }

    // Update is called once per frame
    void Update()
    {

        myText.text = WorldController.Instance.world.character.CurrentJob;
    }
}
