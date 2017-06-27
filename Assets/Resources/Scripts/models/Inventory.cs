using UnityEngine;
using System.Collections;

public class Inventory {

    public string objectType = "steel plate";
    public int maxStackSize = 50;
    public int stackSize = 1;


    ////inventory is either on the floor in tile or on a character!
    public Tile tile;
    public Character character;


    public Inventory() {

    }

    public Inventory (string objectType, int maxStackSize, int stackSize) {
        this.objectType = objectType;
        this.maxStackSize = maxStackSize;
        this.stackSize = stackSize;
    }

    protected Inventory(Inventory other) {
        objectType = other.objectType;
        maxStackSize = other.maxStackSize;
        stackSize = other.stackSize;
        if (other.tile != null)
            tile = other.tile;
        if (other.character != null)
            character = other.character;
    }

    public virtual Inventory Clone() {
        return new Inventory(this);
    }


    override public string ToString()
    {
        return "" + objectType + ":" + stackSize + "/" + maxStackSize;
    }
}
