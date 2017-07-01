using UnityEngine;
using System;
public class Inventory {

    public static int _nextInv = 0;

    public int nextInv {
        get {
            int i = _nextInv;
            _nextInv++;
            return i;
        } }


    private int number;
    public string objectType = "steel plate";
    public int maxStackSize = 50;
    private int _stackSize = 1;
    public int stackSize
    {
        get
        {
            return _stackSize;
        }

        set
        {
            _stackSize = value;
            if (cbInventoryChanged != null) 
                cbInventoryChanged(this);
        }
    }



    ////inventory is either on the floor in tile or on a character!
    public Tile tile;
    public Character character;
    public Job job;
    private Action<Inventory> cbInventoryChanged;

    public Inventory() {
        number = nextInv;
    }

    public Inventory (string objectType, int maxStackSize, int stackSize) {
        this.objectType = objectType;
        this.maxStackSize = maxStackSize;
        this.stackSize = stackSize;
        number = nextInv;
    }

    protected Inventory(Inventory other) {
        objectType = other.objectType;
        maxStackSize = other.maxStackSize;
        stackSize = other.stackSize;
        if (other.tile != null)
            tile = other.tile;
        if (other.character != null)
            character = other.character;
        if (other.job != null)
            job = other.job;
        number = nextInv;
    }

    public virtual Inventory Clone() {
        return new Inventory(this);
    }


    override public string ToString()
    {
        string loc = "";
        if (character != null)
            loc += "C";
        if (job != null)
            loc += "J";
        if (tile != null)
            loc += "T";
        return "" + number + "/" + loc + " : " + objectType + ":" + stackSize + "/" + maxStackSize;
    }

    public void RegisterInventoryChanged(Action<Inventory> callbackfunc)
    {
        cbInventoryChanged += callbackfunc;
    }

    public void UnregisterInventoryChanged(Action<Inventory> callbackfunc)
    {
        cbInventoryChanged -= callbackfunc;
    }
}
