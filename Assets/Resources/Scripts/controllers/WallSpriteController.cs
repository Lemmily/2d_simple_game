using UnityEngine;
using System.Collections.Generic;
using System;

public class WallSpriteController : MonoBehaviour
{

    public GameObject tileSpritePrefab;
    private Dictionary<Furniture, GameObject> wallGameObjectMap;
    private GameObject wall_tiles_go;

    public World World
    {
        get
        {
            return WorldController.Instance.world;
        }
    }

    private void Start()
    {
        wallGameObjectMap = new Dictionary<Furniture, GameObject>();
        wall_tiles_go = new GameObject();
        wall_tiles_go.transform.SetParent(this.transform);
        wall_tiles_go.name = "Wall Tiles";


        World.RegisterFurnitureCreated(OnFurnitureCreated);

    }

    private void OnFurnitureCreated(Furniture furn)
    {
        if (furn.objectType != "wall") {
            return;
        }

        GameObject furn_go = GameObject.Instantiate(tileSpritePrefab);

        wallGameObjectMap.Add(furn, furn_go);

        furn_go.name = furn.objectType + "_" + furn.tile.X + "_" + furn.tile.Y;
        furn_go.transform.position = new Vector3(furn.tile.X + ((furn.Width - 1) / 2f), furn.tile.Y + ((furn.Height - 1) / 2f), 0);   //
        furn_go.transform.SetParent(wall_tiles_go.transform, true);


        //get the tilesprite.

        ConstructTileSprite(furn_go.GetComponent<TileSprites>(), furn);



        furn.RegisterOnChangedCallback(OnFurnitureChanged);
        furn.RegisterOnRemovedCallback(OnFurnitureRemoved);
    }


    public void ConstructTileSprite(TileSprites tileSprite, Furniture furn)
    {
        //find neighbours
        int x = furn.tile.X;
        int y = furn.tile.Y;
        //A cares about N, NW, W
        tileSprite.SetA(GetTileSection(x, y, -1, 1, furn.objectType, "a"));
        //B cares about N, NE, E
        tileSprite.SetB(GetTileSection(x, y, 1, 1, furn.objectType, "b"));
        //C cares about S(0,-1), SW(-1,-1), W(-1,0)
        tileSprite.SetC(GetTileSection(x, y, -1, -1, furn.objectType, "c"));
        //D cares about S, SE, E
        tileSprite.SetD(GetTileSection(x, y, 1, -1, furn.objectType, "d"));


    }

    private Sprite GetTileSection (int x, int y, int xMod, int yMod, string objectType, string tilePos)
    {
        string spriteName = "walls_" + tilePos;
        //check vertical
        if (isSameTypeNeighbourAt(World.GetTileAt(x , y + yMod), objectType)) {
            //check horizontal
            if (isSameTypeNeighbourAt(World.GetTileAt(x + xMod, y), objectType)) {
                //check the diagonal
                if (isSameTypeNeighbourAt(World.GetTileAt(x + xMod, y +yMod), objectType)) {
                    spriteName += "3";
                }
                else {
                    spriteName += "1";
                }
            }
            else {
                spriteName += "5";
            }
        } else {
            if (isSameTypeNeighbourAt(World.GetTileAt(x + xMod, y), objectType)) {
                spriteName += "4";
            }
            else {
                //nothing at all
                spriteName += "2";
            }
        }
        return ResourceLoader.instance.furnitureSpriteMap[spriteName];
    }



    private bool isSameTypeNeighbourAt(Tile t, string objectType)
    {
        if (t?.furniture?.objectType == objectType) {
            return true;
        }

        return false;
    }

    private void OnFurnitureRemoved(Furniture obj)
    {
        throw new NotImplementedException();
    }

    private void OnFurnitureChanged(Furniture furn)
    {
        if(!wallGameObjectMap.ContainsKey(furn)){
            Debug.LogError("OnFurnitureChanged tried to change visuals for sometthing it has no record of.");
            return;
        }
        GameObject furn_go = wallGameObjectMap[furn];
        ConstructTileSprite(furn_go.GetComponent<TileSprites>(), furn);
        furn_go.GetComponent<TileSprites>().tint(furn.tint);
    }
}
