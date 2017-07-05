using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class FurnitureSpriteController : MonoBehaviour
{

    private Dictionary<Furniture, GameObject> furnitureGameObjectMap;
    GameObject all_tiles_go;

    public World World
    {
        get
        {
            return WorldController.Instance.world;
        }
    }

    void Start() {
        furnitureGameObjectMap = new Dictionary<Furniture, GameObject>();

        all_tiles_go = new GameObject();

        all_tiles_go.transform.SetParent(this.transform);
        all_tiles_go.name = "Furniture";


        World.RegisterFurnitureCreated(OnFurnitureCreated);

        foreach (Furniture furniture in World.furnitures)
        {
            OnFurnitureCreated(furniture);
        }
    }

    public void OnFurnitureCreated(Furniture furn) {

        GameObject furn_go = new GameObject();
        //GameObject obj_go = installedObjectGameObjectMap[obj];

        furnitureGameObjectMap.Add(furn, furn_go);


    


        furn_go.name = furn.objectType + "_" + furn.tile.X + "_" + furn.tile.Y;
        furn_go.transform.position = new Vector3(furn.tile.X, furn.tile.Y, 0);
        furn_go.transform.SetParent(this.transform, true);


        //FIXME: this is hardcoded - not ideal!!!
        if (furn.objectType == "door") {
            //check for e-w or n-s walls.

            Tile northTile = World.GetTileAt(furn.tile.X, furn.tile.Y + 1);
            Tile southTile = World.GetTileAt(furn.tile.X, furn.tile.Y - 1);

            if (northTile != null && southTile != null && (southTile.furniture != null && southTile.furniture.objectType == "wall") ||
                (northTile.furniture != null && northTile.furniture.objectType == "wall")) {
                furn_go.transform.rotation = Quaternion.Euler(0, 0, 90);
            }
        }


        furn_go.transform.SetParent(all_tiles_go.transform);
        SpriteRenderer sr = furn_go.AddComponent<SpriteRenderer>();
        sr.sprite = GetSpriteForFurniture(furn);
        sr.sortingLayerName = "Furniture";

        furn.RegisterOnChangedCallback(OnFurnitureChanged);
    }

    void OnFurnitureChanged(Furniture furn) {
        if (!furnitureGameObjectMap.ContainsKey(furn)) {
            Debug.LogError("OnFurnitureChanged - trie dto change visuals for something not in the map!");
        }
        GameObject furn_go = furnitureGameObjectMap[furn];
        furn_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);


        //if this is a door


    }

    public Sprite GetSpriteForFurniture(Furniture furn) {
        string spriteName = furn.objectType;
        if (furn.linksToNeighbour == false) {


            if (furn.objectType == "door") {
                if (furn.furnParameters["openness"] < 0.1f) {
                    spriteName = "door";
                }
                else if (furn.furnParameters["openness"] < 0.5f) {
                    spriteName = "door_openness_1";
                }
                else if (furn.furnParameters["openness"] < 0.9f) {
                    spriteName = "door_openness_2";
                }
                else {
                    spriteName = "door_openness_3";
                }
            }
            return ResourceLoader.instance.furnitureSpriteMap[spriteName];
        }

        int x = furn.tile.X;
        int y = furn.tile.Y;

        spriteName = furn.objectType + "_";
        Tile t;

        t = World.GetTileAt(x, y + 1);

        if (t != null && t.furniture != null && t.furniture.objectType == furn.objectType) {
            spriteName += "N";
        }
        t = World.GetTileAt(x + 1, y);
        if (t != null && t.furniture != null && t.furniture.objectType == furn.objectType) {
            spriteName += "E";
        }
        t = World.GetTileAt(x, y - 1);
        if (t != null && t.furniture != null && t.furniture.objectType == furn.objectType) {
            spriteName += "S";
        }
        t = World.GetTileAt(x - 1, y);
        if (t != null && t.furniture != null && t.furniture.objectType == furn.objectType) {
            spriteName += "W";
        }


        if (furn.objectType == "door") {
            if (furn.furnParameters["openness"] < 0.1f) {
                spriteName = "door";
            }
            else if (furn.furnParameters["openness"] < 0.5f) {
                spriteName = "door_openness_1";
            }
            else if (furn.furnParameters["openness"] < 0.9f) {
                spriteName = "door_openness_2";
            }
            else {
                spriteName = "door_openness_3";
            }
        }


        if (furn.objectType == "stockpile") {
            spriteName = "stockpile";
        }

        try {

            return ResourceLoader.instance.furnitureSpriteMap[spriteName];
        }
        catch (KeyNotFoundException ex) {
            Debug.Log(ex.Data);
            return null;

        }

    }

}
