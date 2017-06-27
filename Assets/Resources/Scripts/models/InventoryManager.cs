using UnityEngine;
using System.Collections.Generic;
using System;

public class InventoryManager  {


    public Dictionary<string, List<Inventory>> inventories;
    private Action<Inventory> cbInventoryCreated;
    private Action<Inventory> cbInventoryChanged;
    private Action<Inventory> cbInventoryRemoved;

    public InventoryManager() {
        inventories = new Dictionary<string, List<Inventory>>();
    }

    public bool PlaceInventory (Tile tile, Inventory inv) {
        bool tileWasEmpty = tile.inventory == null;

        int a = inv.stackSize;
        if ( ! tile.PlaceInventory(inv) ) {
            //the tile rejected the inventory.
            Debug.Log("Inventory Manager:- Tried & failed to placed inventory:-" + inv + " on tile :- " + tile);
            return false;
        }


        if (tile.inventory != null && inv.stackSize != a) {
            //how would inv change if there was no tile inventory?
            cbInventoryChanged(tile.inventory);
        }

        // At this point, "inv" might be an empty stack if it was merged to another stack.
        if (inv.stackSize == 0) {
            ClearEmptyInventory(inv);
        }

        // We may also created a new stack on the tile, if the tile was previously empty.

        //little unsure if this logic isn't actually duplicating somehow?
        if (tileWasEmpty) {
            if (inventories.ContainsKey(tile.inventory.objectType) == false) {
                inventories[tile.inventory.objectType] = new List<Inventory>();
            }

            inventories[tile.inventory.objectType].Add(tile.inventory);
            Debug.Log("Inventory Manager:- Placed new inventory:-" + inv + " on tile :- " + tile);
            cbInventoryCreated(tile.inventory);
        }
        Debug.Log("inventories:-" + PrintInventories());
        return true;
    }

    private void ClearEmptyInventory(Inventory inv)
    {
        if (inv.stackSize == 0 && inventories.ContainsKey(inv.objectType)) {
            inventories[inv.objectType].Remove(inv);
            cbInventoryRemoved(inv);

            if (inv.tile != null) {
                inv.tile.inventory = null;
                inv.tile = null;
            }
            if (inv.character != null) {
                inv.character.inventory = null;
                inv.character = null;

            }
        }
    }

    public bool PlaceInventory(Job job, Inventory inv) {

        if (job.DesiresInventoryType(inv) == false) {
            Debug.LogError("Tried to give a job an inventory item it didn;t need.");
        }
        job.inventoryRequirements[inv.objectType].stackSize += inv.stackSize;
        if (job.inventoryRequirements[inv.objectType].maxStackSize < job.inventoryRequirements[inv.objectType].stackSize) {
            inv.stackSize = job.inventoryRequirements[inv.objectType].stackSize - job.inventoryRequirements[inv.objectType].maxStackSize;
            job.inventoryRequirements[inv.objectType].stackSize = job.inventoryRequirements[inv.objectType].maxStackSize;
           
        } else {
            //already put stuff into job inventory so clear the other!
            inv.stackSize = 0;
        }

        if (inv.stackSize == 0) {
            ClearEmptyInventory(inv);
        }


        return true;
    }

    public bool PlaceInventory(Character character, Inventory inv)
    {

        character.inventory.stackSize += inv.stackSize;

        if(character.inventory.maxStackSize < character.inventory.stackSize) {
            inv.stackSize = character.inventory.stackSize - character.inventory.maxStackSize;
            character.inventory.stackSize = character.inventory.maxStackSize;
        }
       
        if (inv.stackSize == 0) {
            ClearEmptyInventory(inv);
        }
        return true;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="objectType"></param>
    /// <param name="t"></param>
    /// <param name="desiredAmount">Desired Amount. If no stack has enough it returns the largest</param>
    /// <returns></returns>
    public Inventory GetClosestInventoryOfType(string objectType, Tile t, int desiredAmount)
    {

        //FIXME: lies aout the closest item
        if (inventories.ContainsKey(objectType) == false)
            return null;

        foreach (Inventory inv in inventories[objectType])
	    {
            if (inv.tile != null) {
                return inv;
            }
	    }

        return null;
    }


    public string PrintInventories()
    {
        string message = "";
        foreach (KeyValuePair<string, List<Inventory>> inventoryLists in inventories) {
            message += inventoryLists.Key + ":";

            List<Inventory> list = inventoryLists.Value;
            if (list.Count > 0) {
                for (int i = 0; i < list.Count; i++) {
                    message += "\n";
                    message += list[i];
                }
            }
            else {
                message += "" + 0;
            }
        }

        return message;
    }




    public void RegisterInventoryCreated(Action<Inventory> callbackfunc)
    {
        cbInventoryCreated += callbackfunc;
    }

    public void UnregisterInventoryCreated(Action<Inventory> callbackfunc)
    {
        cbInventoryCreated -= callbackfunc;
    }


    public void RegisterInventoryChanged(Action<Inventory> callbackfunc)
    {
        cbInventoryChanged += callbackfunc;
    }

    public void UnregisterInventoryChanged(Action<Inventory> callbackfunc)
    {
        cbInventoryChanged -= callbackfunc;
    }
    public void RegisterInventoryRemoved(Action<Inventory> callbackfunc)
    {
        cbInventoryRemoved += callbackfunc;
    }

    public void UnregisterInventoryRemoved(Action<Inventory> callbackfunc)
    {
        cbInventoryRemoved -= callbackfunc;
    }
}
