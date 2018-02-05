using UnityEngine;
using System.Collections;
using System;

public class FurnitureActions {


    public static void Door_UpdateAction(Furniture furn, float deltaTime) {
       // Debug.Log("Door_UpdateAction");

        if (furn.furnParameters["is_opening"] >= 1) {
            furn.furnParameters["openness"] += deltaTime * 4;

            if (furn.furnParameters["openness"] >= 1) {
                furn.furnParameters["is_opening"] = 0;
            }
        } else {
            furn.furnParameters["openness"] -= deltaTime * 4;
        }

        furn.furnParameters["openness"] = Mathf.Clamp01(furn.furnParameters["openness"]);

        if(furn.cbOnChanged != null)
            furn.cbOnChanged(furn);
    }


    public static ENTERABILITY Door_IsEnterable(Furniture furn) {
        Debug.Log("Door Is Enterable");

        furn.furnParameters["is_opening"] = 1;

        if (furn.furnParameters["openness"] >= 1) {
            return ENTERABILITY.Yes;
        }

        return ENTERABILITY.Soon;
    }


    public static void JobComplete_FurnitureBuilding(Job j) {
        {
            WorldController.Instance.world.PlaceFurniture(j.jobObjectType, j.tile);
        }
    }


    public static void Stockpile_UpdateAction(Furniture furn, float deltaTime)
    {

        //TODO: we will want to stop this running EVERY update. Lots of furniture will mean job spam!
        // Instead it will need to only run when:-
        //   -- It gets created
        //   -- A good gets delivered. (job reset)
        //   -- A good gets picked up. (job reset)
        //   -- UI "filter" gets modified.

        if(furn.tile.inventory != null && furn.tile.inventory.stackSize >= furn.tile.inventory.maxStackSize) {
            //we're full!
            furn.ClearJobs();
            return; //GTFO
        }

        if(furn.tile.inventory != null && furn.tile.inventory.stackSize == 0) {
            Debug.LogError("Stockpile:- Something went horribly wrong. the stockpile has a ZERO stacksize.");
            furn.ClearJobs();
            return; //GTFO
        }


        if (furn.JobCount() > 0) {
            return;
        }

        //Two routes from this point onwards, we have SOME/ALL Inventory.
        // or we have NO inventory.

        Inventory[] itemsDesired;
        if (furn.tile.inventory == null) {
            Debug.Log("Stockpile:- Creating job for new stack.");
            itemsDesired = Stockpile_GetItemsFromFilter();

        } else {
            Debug.Log("Stockpile:-Creating job for existing stack.");
            Inventory desInv = furn.tile.inventory.Clone();
            desInv.maxStackSize -= desInv.stackSize;
            desInv.stackSize = 0;
            itemsDesired = new Inventory[] { desInv };
        }


        
        Job j = new Job(
            furn.tile,
            null,
            null,
            0,
            itemsDesired
            );

        //TODO: add stockpile priorities, so we can take from lower priority -> higher priority stockpile.
        j.canTakeFromStockpile = false;
        j.RegisterJobWorkedCallback(Stockpile_JobWorked);
        furn.AddJob(j);
    }

    public static Inventory[] Stockpile_GetItemsFromFilter()
    {

        //this will be reading from elsewhere.
        //FIXME: Make this not hard-coded.
        return new Inventory[1] { new Inventory("steel plate", 50, 0) };

    }

    static void Stockpile_JobComplete(Job j)
    {
        //TODO: Sort this out so that the job's inventory goes into the tile properly.
        j.tile.world.inventoryManager.PlaceInventory(j.tile, j.inventoryRequirements["steel plate"]);
        j.tile.furniture.ClearJobs();
        j.UnregisterJobWorkedCallback(Stockpile_JobWorked);
        
    }

    static void Stockpile_JobWorked(Job j)
    {
        j.tile.furniture.RemoveJob(j);

        foreach (Inventory inv in j.inventoryRequirements.Values) {
            if (inv.stackSize > 0) {
                j.tile.world.inventoryManager.PlaceInventory(j.tile, inv);
                return;
            }
        }
    }
}
