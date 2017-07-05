using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class InventorySpriteController : MonoBehaviour
{
    public static InventorySpriteController Instance;
    public GameObject inventoryUIPrefab;

    Dictionary<Inventory, GameObject> inventoryGameObjectMap;

    Dictionary<string, Sprite> inventorySprites;

    World world
    {
        get { return WorldController.Instance.world; }
    }

    // Use this for initialization
    void Start() {
        Instance = this;
        // Instantiate our dictionary that tracks which GameObject is rendering which Tile data.
        inventoryGameObjectMap = new Dictionary<Inventory, GameObject>();

        // Register our callback so that our GameObject gets updated whenever
        // the tile's type changes.
        world.inventoryManager.RegisterInventoryCreated(OnInventoryCreated);
        //world.inventoryManager.RegisterInventoryRemoved(OnInventoryRemoved);

        // Check for pre-existing inventory, which won't do the callback.
        foreach (string objectType in world.inventoryManager.inventories.Keys) {
            foreach (Inventory inv in world.inventoryManager.inventories[objectType]) {
                OnInventoryCreated(inv);
            }
        }


        //c.SetDestination( world.GetTileAt( world.Width/2 + 5, world.Height/2 ) );
    }

    //private void OnInventoryRemoved(Inventory inv)
    //{
    //    Debug.Log("OnInventoryRemoved:- " + inv);
    //    //do something here to make it disappear.
    //    if (inventoryGameObjectMap.ContainsKey(inv)) {
    //        GameObject go = inventoryGameObjectMap[inv];
    //        inventoryGameObjectMap.Remove(inv);
    //        DestroyAllChildren(go.transform);

    //        Destroy(go);
    //        Debug.Log("InventoryRemoved - did sum destructions.");
    //    } else {
    //        //where is it and how much should i delete?
    //    }
    //    inv.UnregisterInventoryChanged(OnInventoryChanged);
    //}


    //private void DestroyAllChildren(Transform go)
    //{
    //    foreach (Transform each_go in go.GetComponents<Transform>()) {
    //        if (each_go.GetComponents<Transform>().Length > 0) {
    //            foreach (Transform baby in go.GetComponents<Transform>()) {
    //                DestroyAllChildren(baby);
    //            }
    //        }
    //        GameObject.Destroy(each_go);
    //    }
    //}

    private void OnInventoryCreated(Inventory inv) {
        Debug.Log("OnInventoryCreated:- " + inv);
        // Create a visual GameObject linked to this data.

        // FIXME: Does not consider multi-tile objects nor rotated objects

        // This creates a new GameObject and adds it to our scene.
        GameObject inv_go = new GameObject();
        // Add our tile/GO pair to the dictionary.
        inventoryGameObjectMap.Add(inv, inv_go);

        inv_go.name = inv.objectType;
        inv_go.transform.position = new Vector3(inv.tile.X, inv.tile.Y, 0);
        inv_go.transform.SetParent(this.transform, true);

        SpriteRenderer sr = inv_go.AddComponent<SpriteRenderer>();
        sr.sprite = ResourceLoader.instance.itemSpriteMap[inv.objectType];
        sr.sortingLayerName = "Inventory";

        if (inv.maxStackSize > 1) {
            // This is a stackable object, so let's add a InventoryUI component
            // (Which is text that shows the current stackSize.)

            GameObject ui_go = Instantiate(inventoryUIPrefab);
            ui_go.transform.SetParent(inv_go.transform);
            ui_go.transform.localPosition = new Vector3(-0.5f, -0.5f);
            ui_go.GetComponentInChildren<Text>().text = inv.stackSize.ToString();
        }

        // Register our callback so that our GameObject gets updated whenever
        // the object's into changes.
        // FIXME: Add on changed callbacks
        inv.RegisterInventoryChanged(OnInventoryChanged);

    }

    public void OnInventoryChanged(Inventory inv) {
        // FIXME:  Still needs to work!  And get called!

        Debug.Log("OnInventoryChanged:- " + inv);
        // Make sure the inventory's graphics are correct.

        if (!inventoryGameObjectMap.ContainsKey(inv)) {
            Debug.LogError("OnInventoryChanged -- trying to change visuals for inventory not in our map??");
            return;
        }

        GameObject inv_go = inventoryGameObjectMap[inv];

        //gotta make some kind of decision for when the character is carrying the items?

        if (inv.stackSize > 0) {

            inv_go.transform.position = new Vector3(inv.tile.X, inv.tile.Y, 0);
            Text t = inv_go.transform.GetComponentInChildren<Text>();
            if (t != null)
                t.text = "" + inv.stackSize;
        } else {
            Destroy(inv_go);
            inventoryGameObjectMap.Remove(inv);
            inv.UnregisterInventoryChanged(OnInventoryChanged);
        }
    }



}
