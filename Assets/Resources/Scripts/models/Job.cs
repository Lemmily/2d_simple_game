using UnityEngine;
using System.Collections.Generic;
using System;

public class Job{


    public Tile tile;
    public float JobTime
    {
        get;
        protected set;
    }

    //Action<T> cbJob;
    Action<Job> cbJobComplete;
    Action<Job> cbJobWorked;
    Action<Job> cbJobCancel;
    public Dictionary<string, Inventory> inventoryRequirements;

    public int proximityToComplete = 1; //how close should a character be to complete 0 - on tile, 1 adjacent tile.
    public bool acceptsAnyInventoryItem;

    public string jobObjectType { get; protected set;  }

    public bool canTakeFromStockpile = true;

    public Job (Tile tile, 
            string jobObjectType, 
            Action<Job> cbJobComplete,
            //Action<Job> cbJobWorked,
            float jobTime, 
            Inventory[] inventoryRequirements
        )
    {
        this.tile = tile;
        this.cbJobComplete += cbJobComplete;
        //this.cbJobWorked += cbJobWorked;
        this.JobTime = jobTime;
        this.jobObjectType = jobObjectType;

        this.inventoryRequirements = new Dictionary<string, Inventory>();
        if (inventoryRequirements != null) {
            foreach (Inventory inv in inventoryRequirements) {
                this.inventoryRequirements[inv.objectType] = inv.Clone();
            }
        }
        //Inventory inv = new Inventory();
        //inv.objectType = "Steel PLate"; //type required.
        //inv.maxStackSize = 5; //num required to complete job
        //inventoryRequirements["Steel Plate"] = inv;
        
    }

    protected Job(Job job) {
        this.tile = job.tile;
        
        this.cbJobComplete += job.cbJobComplete;
        this.JobTime = job.JobTime;
        this.jobObjectType = job.jobObjectType;

        this.inventoryRequirements = new Dictionary<string, Inventory>();
        if (job.inventoryRequirements != null) {
            foreach (Inventory inv in job.inventoryRequirements.Values) {
                this.inventoryRequirements[inv.objectType] = inv.Clone();
            }
        }
    }

    virtual public Job Clone() {
        return new Job(this);
    }

    public void RegisterJobCompleteCallback(Action<Job> cb) {
        cbJobComplete += cb;
    }

    public void UnregisterJobCompleteCallback(Action<Job> cb) {
        cbJobComplete -= cb;
    }

    public void RegisterJobCancelCallback(Action<Job> cb)
    {
        cbJobCancel += cb;
    }

    public void UnregisterJobCancelCallback(Action<Job> cb)
    {
        cbJobCancel -= cb;
    }
    public void RegisterJobWorkedCallback(Action<Job> cb)
    {
        cbJobWorked += cb;
    }


    public void UnregisterJobWorkedCallback(Action<Job> cb)
    {
        cbJobWorked -= cb;
    }

    public void DoWork(float workTime) {


        if (HasAllMaterials() == false) {
            // if we've tried to do work on a job that doesnt have all materials.
            // it's probably an insta-completable one!
            if (cbJobWorked != null)
                cbJobWorked(this);
            return;
        }


        JobTime -= workTime;

        if (cbJobWorked != null) {
            cbJobWorked(this);
        }

        if (JobTime <= 0) {
            if(cbJobComplete != null) 
                cbJobComplete(this);
        }
    }

    public void CancelJob() {
        if (cbJobCancel != null)
            cbJobCancel(this);

        tile.world.jobQueue.Remove(this);
    }

    public bool HasAllMaterials() {
        foreach (Inventory inv in inventoryRequirements.Values) {
            if (inv.stackSize < inv.maxStackSize) {
                return false;
            }
        }

        return true;
    }


    public int DesiresInventory(Inventory inv)
    {
        if (inv == null)
            return 0;
        if (acceptsAnyInventoryItem) {
            return inv.maxStackSize;
        }

        if (inventoryRequirements.ContainsKey(inv.objectType) == false) {
            return 0;
        }

        if (inventoryRequirements[inv.objectType].stackSize >= inventoryRequirements[inv.objectType].maxStackSize) {
            return 0;
        }
         
        //returns the amount still wanted of this particular inventory type.
        return inventoryRequirements[inv.objectType].maxStackSize - inventoryRequirements[inv.objectType].stackSize;

    }

    public Inventory GetFirstDesiredInventory()
    {
        foreach (Inventory inv in inventoryRequirements.Values) {
            if(inv.stackSize < inv.maxStackSize) {
                return inv;
            }
        }

        return null;
    }

}
