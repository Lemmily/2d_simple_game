using UnityEngine;
using System.Collections.Generic;
using System;

public class Job{


    public Tile tile;
    float jobTime;

    //Action<T> cbJob;
    Action<Job> cbJobComplete;
    Action<Job> cbJobCancel;
    public Dictionary<string, Inventory> inventoryRequirements;    

    public string theFurniture { get; protected set;  }

    public Job (Tile tile, string jobObjectType, Action<Job> cbJobComplete, 
                                    float jobTime, Inventory[] inventoryRequirements) {
        this.tile = tile;
        this.cbJobComplete += cbJobComplete;
        this.jobTime = jobTime;
        this.theFurniture = jobObjectType;

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
        this.jobTime = job.jobTime;
        this.theFurniture = job.theFurniture;

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

    public void RegisterJobCancelCallback(Action<Job> cb) {
        cbJobCancel += cb;
    }


    public void UnregisterJobCancelCallback(Action<Job> cb) {
        cbJobCancel -= cb;
    }

    public void DoWork(float workTime) {
        jobTime -= workTime;

        if (jobTime <= 0) {
            if(cbJobComplete != null) 
                cbJobComplete(this);
        }
    }

    public void CancelJob() {
        if (cbJobCancel != null)
            cbJobCancel(this);
    }

    public bool HasAllMaterials() {
        foreach (Inventory inv in inventoryRequirements.Values) {
            if (inv.stackSize < inv.maxStackSize) {
                return false;
            }
        }

        return true;
    }


    public bool DesiresInventoryType(Inventory inv) {
        if (inventoryRequirements.ContainsKey(inv.objectType) == false) {
            return false;
        }

        if (inventoryRequirements[inv.objectType].stackSize >= inventoryRequirements[inv.objectType].maxStackSize) {
            return false;
        }
         
        //if we're here we have enough fo this type of materials.
        return true;

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
