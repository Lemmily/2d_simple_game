using UnityEngine;
using System.Collections;

public class SoundController : MonoBehaviour {

    float soundCooldown = 0;

	void Start () {
        WorldController.Instance.world.RegisterTileChanged(OnTileChanged);
        WorldController.Instance.world.RegisterFurnitureCreated(OnFurnitureCreated);
    }
	
	void Update () {
        soundCooldown -= Time.deltaTime;
	}


    public void OnTileChanged(Tile tile_data) {
        if (soundCooldown > 0) {
            return;
        }
        AudioSource.PlayClipAtPoint(ResourceLoader.GetSound("Floor_OnCreated"), Camera.main.transform.position);
        soundCooldown = 0.1f;
    }

    public void OnFurnitureCreated(Furniture furniture) {
        if(soundCooldown > 0) {
            return;
        }
        AudioClip ac = ResourceLoader.GetSound(furniture.objectType + "_OnCreated");
        if (ac == null) {
            AudioSource.PlayClipAtPoint(ResourceLoader.GetSound("Wall_OnCreated"), Camera.main.transform.position);
        }
        else {
            AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
        }
        soundCooldown = 0.1f;
    }



}
