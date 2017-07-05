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

    private void CleanUpInventory(Inventory inv)
    {
        if (inv.stackSize > 0)
            return;
        if (inv.stackSize == 0 && inventories.ContainsKey(inv.objectType)) {
            inventories[inv.objectType].Remove(inv);
            //cbInventoryRemoved(inv);

            if (inv.tile != null) {
                inv.tile.inventory = null;
                inv.tile = null;
            }
            if (inv.character != null) {
                inv.character.inventory = null;
                inv.character = null;
            }
            //if (inv.job != null) {
            // inv.job.inventories.Remove(inv);
            // inv.job = null;
            //}
        } else if (inv.stackSize == 0) {
            Debug.Log("InventoryManager:- Have an inventory that I didn't know about! What do?");
        }
        
        inv.UnregisterInventoryChanged(InventorySpriteController.Instance.OnInventoryChanged);
        inv = null;

    }


    //public void ChangeInventory(Inventory inventory,  int stackSize, string objectType = "-", int maxStackSize=-1)
    //{
    //    if (maxStackSize != -1)
    //        inventory.maxStackSize = maxStackSize;
        
    //    if(objectType != "-")
    //        inventory.objectType = objectType;
        
    //    inventory.stackSize = stackSize;
    //    cbInventoryChanged(inventory);
    //}


    public bool PlaceInventory (Tile tile, Inventory inv, int amount = -1)
    {
        bool tileWasEmpty = tile.inventory == null;
        
        if (inv == null) {
            Debug.LogError("Trying to place a null inventory into a tile");
            return false;
        }
        int inv_stackSize = inv.stackSize;
        if ( ! tile.PlaceInventory(inv) ) {
            //the tile rejected the inventory.
            Debug.Log("Inventory Manager:- Tried & failed to placed inventory:-" + inv + " on tile :- " + tile);
            return false;
        }
        
        if (tile.inventory != null && inv.stackSize != inv_stackSize) {
            //how would inv change if there was no tile inventory?
            Debug.Log("Placed something on a tile.");
            cbInventoryChanged(tile.inventory);
        }

        // At this point, "inv" might be an empty stack if it was merged to another stack.
        CleanUpInventory(inv);

        // We may also created a new stack on the tile, if the tile was previously empty.
        if (tileWasEmpty) {
            if (inventories.ContainsKey(tile.inventory.objectType) == false) {
                //first time placing this kind of object.
                inventories[tile.inventory.objectType] = new List<Inventory>(); 
            }
            inventories[tile.inventory.objectType].Add(tile.inventory);
            Debug.Log("Inventory Manager:- Placed new inventory:-" + tile.inventory + " on tile :- " + tile);
            //callbacks
            if (cbInventoryCreated != null) {
                cbInventoryCreated(tile.inventory);
            }
        }
        //Debug.Log("inventories:-" + PrintInventories());
        return true;
    }

    public bool PlaceInventory(Job job, Inventory inv, int amount=-1) {

        if (job.DesiresInventory(inv) <= 0) {
            Debug.LogError("Tried to give a job an inventory item it didn;t need.");
        }

        //put all of inventory into job site.
        job.inventoryRequirements[inv.objectType].stackSize += inv.stackSize;


        if (job.inventoryRequirements[inv.objectType].maxStackSize < job.inventoryRequirements[inv.objectType].stackSize) {
            inv.stackSize = job.inventoryRequirements[inv.objectType].stackSize - job.inventoryRequirements[inv.objectType].maxStackSize;
            job.inventoryRequirements[inv.objectType].stackSize = job.inventoryRequirements[inv.objectType].maxStackSize;
        } else {
            //already put stuff into job inventory so clear the other!
            inv.stackSize = 0;
        }

        CleanUpInventory(inv);
        
        return true;
    }

    public bool PlaceInventory(Character character, Inventory srcInv, int amount = -1)
    {
        if (amount == -1) {
            amount = srcInv.stackSize;
        } else {
            amount = Mathf.Min(amount, srcInv.stackSize);
        }


        if (character.inventory == null) {
            Debug.Log("Character has no inventory to place into. Character's Job should have made one for it.");
            return false;
        } else if (character.inventory.objectType != srcInv.objectType) {
            //FIXME: make it so the character takes it's uneeded items and stores them elsewhere. Stack based FSM FTW here.
            Debug.LogError("Character already has an inventory of a different type. Cannot pick up items");
            return false;
        }

        character.inventory.stackSize += amount;

        if(character.inventory.maxStackSize < character.inventory.stackSize) {
            srcInv.stackSize = character.inventory.stackSize - character.inventory.maxStackSize;
            character.inventory.stackSize = character.inventory.maxStackSize;
        } else {
            srcInv.stackSize -= amount;
        }
       
        CleanUpInventory(srcInv);
        return true;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="objectType"></param>
    /// <param name="t"></param>
    /// <param name="desiredAmount">Desired Amount. If no stack has enough it returns the largest</param>
    /// <returns></returns>
    public Inventory GetClosestInventoryOfType(string objectType, Tile t, int desiredAmount=-1)
    {
        if (desiredAmount == -1) {
            desiredAmount = 1; //FIXME: A bit silly hardcode so i can just check if there's ANY pile around
        }
        //FIXME: lies aout the closest item
        if (inventories.ContainsKey(objectType) == false)
            return null;

        foreach (Inventory inv in inventories[objectType])
	    {
            if (inv.tile != null) {
                //if it's ona tile.
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

    public void RegisterInventoryRemoved(Action<Inventory> callbackfunc)
    {
        cbInventoryRemoved += callbackfunc;
    }

    public void UnregisterInventoryRemoved(Action<Inventory> callbackfunc)
    {
        cbInventoryRemoved -= callbackfunc;
    }
}
