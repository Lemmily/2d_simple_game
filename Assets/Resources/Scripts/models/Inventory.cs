using UnityEngine;
using System.Collections;

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
        }
    }



    ////inventory is either on the floor in tile or on a character!
    public Tile tile;
    public Character character;


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
        number = nextInv;
    }

    public virtual Inventory Clone() {
        return new Inventory(this);
    }


    override public string ToString()
    {
        return "" + number + ":" + objectType + ":" + stackSize + "/" + maxStackSize;
    }
}
