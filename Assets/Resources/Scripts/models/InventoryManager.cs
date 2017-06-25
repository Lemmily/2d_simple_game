using UnityEngine;
using System.Collections.Generic;

public class InventoryManager  {


    public Dictionary<string, List<Inventory>> inventories;


    public InventoryManager() {
        inventories = new Dictionary<string, List<Inventory>>();
    }

    public bool PlaceInventory (Tile tile, Inventory inv) {
        bool tileWasEmpty = tile.inventory == null;


        if ( ! tile.PlaceInventory(inv) ) {
            //the tile rejected the inventory.
            return false;
        }

        // At this point, "inv" might be an empty stack if it was merged to another stack.
        if (inv.stackSize == 0) {
            if (inventories.ContainsKey(tile.inventory.objectType)) {
                inventories[inv.objectType].Remove(inv);
            }
        }

        // We may also created a new stack on the tile, if the tile was previously empty.
        if (tileWasEmpty) {
            if (inventories.ContainsKey(tile.inventory.objectType) == false) {
                inventories[tile.inventory.objectType] = new List<Inventory>();
            }

            inventories[tile.inventory.objectType].Add(tile.inventory);
        }

        return true;
    }


    public bool PlaceInventory(Job job, Inventory inv) {

        if (job.DesiresInventoryType(inv) == false) {
            Debug.LogError("Tried to give a job an inventory item it didn;t need.");
        }
        //bool tileWasEmpty = tile.inventory == null;


        //if (!tile.PlaceInventory(inv)) {
        //    //the tile rejected the inventory.
        //    return false;
        //}

        //// At this point, "inv" might be an empty stack if it was merged to another stack.
        //if (inv.stackSize == 0) {
        //    if (inventories.ContainsKey(tile.inventory.objectType)) {
        //        inventories[inv.objectType].Remove(inv);
        //    }
        //}

        //// We may also created a new stack on the tile, if the tile was previously empty.
        //if (tileWasEmpty) {
        //    if (inventories.ContainsKey(tile.inventory.objectType) == false) {
        //        inventories[tile.inventory.objectType] = new List<Inventory>();
        //    }

        //    inventories[tile.inventory.objectType].Add(tile.inventory);
        //}

        return true;
    }
}
