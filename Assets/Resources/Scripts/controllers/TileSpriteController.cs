using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileSpriteController : MonoBehaviour {
    
    private Dictionary<Tile, GameObject> tileGameObjectMap;


    public World World {
        get
        {
            return WorldController.Instance.world;
        }
    }

    void Start() {
        tileGameObjectMap = new Dictionary<Tile, GameObject>();



        GameObject all_tiles_go = new GameObject();

        all_tiles_go.transform.SetParent(this.transform);
        all_tiles_go.name = "Tiles";

        for (int x = 0; x < World.Width; x++) {
            for (int y = 0; y < World.Height; y++) {
                Tile tile_data = World.GetTileAt(x, y);

                GameObject tile_obj = new GameObject();
                tile_obj.name = "Tile_" + x + "_" + y;
                tile_obj.transform.position = new Vector3(tile_data.X, tile_data.Y, 0);

                SpriteRenderer sr = tile_obj.AddComponent<SpriteRenderer>();
                sr.sprite = ResourceLoader.GetTileSprite("bg");
                sr.sortingLayerName = "Tiles";
                
                tile_obj.transform.SetParent(all_tiles_go.transform);
                
                tileGameObjectMap.Add(tile_data, tile_obj);
                OnTileChanged(tile_data);
            }
        }
        
        World.RegisterTileChanged(OnTileChanged);
        
        
    }

    // Update is called once per frame
    void Update() {

    }


    void OnTileChanged(Tile tile_data) {

        if (!tileGameObjectMap.ContainsKey(tile_data)) {
            Debug.LogError("tileGameObjectMap - Does not contain the tile data");
            return;
        }

        GameObject tile_go = tileGameObjectMap[tile_data];

        if (tile_go == null) {
            Debug.LogError("tileGameObjectMap - game object for tile data is not present ");
            return;
        }

        if (tile_data.Type == TileType.Floor) {
            tile_go.GetComponent<SpriteRenderer>().sprite = ResourceLoader.GetTileSprite("Floor");
        }
        else if (tile_data.Type == TileType.Empty) {
            tile_go.GetComponent<SpriteRenderer>().sprite = ResourceLoader.GetTileSprite("bg");
        }
        else {
            Debug.LogError("OnTileTypeChanged - Unregocnised tile type.");
        }
    }

}
