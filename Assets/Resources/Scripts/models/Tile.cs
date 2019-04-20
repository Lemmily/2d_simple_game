
using UnityEngine;
using System.Collections;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public enum TileType { Empty, Floor };

public enum ENTERABILITY {  Yes, Never, Soon }

public class Tile : IXmlSerializable{

    TileType type = TileType.Empty;

    Action<Tile> cbTileTypeChanged;
    public World world { get; protected set; }
    public Furniture furniture { get; protected set; }

    public Job pendingFurnitureJob;

    public Room room;

    float baseTileMovementCost = 1; // HARDCODED
    public Inventory inventory;

    public float movementCost  {
        get
        {
            if (type == TileType.Empty)
                return 0; //unwalkable
            if (furniture == null)
                return 1f;

            return baseTileMovementCost * furniture.movementCost;
        }
    }


    public int X
    {
        get;

        protected set;
    }

    public int Y
    {
        get;

        protected set;
    }


    public TileType Type
    {
        get
        {
            return type;
        }

        set
        {
            TileType oldtype = type;
            type = value;
            if (cbTileTypeChanged != null && oldtype != type) {
                cbTileTypeChanged(this);
            }
        }
    }

    public Tile(World world)
    {
        this.world = world;
    }

    public Tile(World world, int x, int y) {
        this.world = world;
        X = x;
        Y = y;

    }

    override public String ToString()
    {

        string message = "(" + X + "," + Y + ") isEnterable:" + IsEnterable();
        if(this.hasFurniture()) {
            message += "has furn:-" + this.furniture.objectType;
        }
        return message;
    }

    internal void RegisterTileChangedCallback(Action<Tile> callback) {
        cbTileTypeChanged += callback;
    }

    internal void UnregisterTileChangeCallback(Action<Tile> callback) {
        cbTileTypeChanged -= callback;
    }


    public bool hasFurniture()
    {
        return furniture != null;
    }

    public bool UnplaceFurniture()
    {
        if (furniture == null)
            return false;

        Furniture f = furniture;

        if (furniture.Width > 1 || furniture.Height > 1)
        {

            for (int x_off = X; x_off < X + f.Width; x_off++)
            {
                for (int y_off = Y; y_off < Y + f.Height; y_off++)
                {
                    Tile t = world.GetTileAt(x_off, y_off);
                    t.furniture = null;
                }

            }
        }
        furniture = null;
        
        return true;
    }

    public void Deconstruct()
    {
        Debug.Log("Deconstruct");
        furniture = null;

    }

    public bool InstallFurniture(Furniture objInstance) {
        if (objInstance == null)
        {
            Debug.LogError("Trying to install null furniture??? NOPE");
            //Just uninstalling FIXME: What if we have multi tile furniture?
            return false;
        }
        if (objInstance.IsValidPosition(this) == false) {
            Debug.LogError("Trying to assign a furniture to a tile that isn't valid.");
            return false;
        }

        for (int x_off = X; x_off < X + objInstance.Width; x_off++)
        {
            for (int y_off = Y; y_off < Y + objInstance.Height; y_off++)
            {
                Tile t = world.GetTileAt(x_off, y_off);
                t.furniture = objInstance;
            }

        }
        return true;
                 
    }


    public bool PlaceInventory(Inventory other_inv) {


        if (other_inv == null) {
            //inventory = null;
            return false;
        }

        if (inventory != null) {
            // already inventory, amybe combine stacks?
            if (inventory.objectType != other_inv.objectType && inventory.stackSize > 0) {
                Debug.LogError("trying to assign inventory to tile that has a DIFFERENT type");
                return false;
            }
            else if (inventory.objectType != other_inv.objectType && inventory.stackSize == 0)
            {
                //has other type, but nothing occupying. Might be used as stockpile filter?
                return false;
            }

            if (inventory.stackSize + other_inv.stackSize > inventory.maxStackSize) {
                Debug.LogError("Trying to add too many items to inventory! Squeezing as much as i can onto the tile and keeping hold of the rest.");
                int total = inventory.stackSize + other_inv.stackSize;
                int dif = total - inventory.maxStackSize;
                
                inventory.stackSize = inventory.maxStackSize;
                other_inv.stackSize = dif;
                return true;
            }
            else {
                //just add them all to the tile inventory.
                inventory.stackSize += other_inv.stackSize;
                other_inv.stackSize = 0;
                return true;
            }
        }
        //getting here means the tile had no inventory.
        inventory = other_inv;
        inventory.tile = this;
        return true;
    }
    public bool IsNeighbour(Tile tile) {
        if (tile == this)
            return false;

        if (Mathf.Abs(Mathf.Max(tile.X, X) - Mathf.Min(tile.X, X)) <= 1 && Mathf.Abs(Mathf.Max(tile.Y, Y) - Mathf.Min(tile.Y, Y)) <= 1) {
            return true;
        }

        return false;
    }


    public Tile[] GetNeighbours(bool diagOkay=false) {
        Tile[] tiles;
        if (diagOkay)
            tiles = new Tile[8];
        else
            tiles = new Tile[4];


        tiles[0] = world.GetTileAt(X, Y + 1); //N
        tiles[1] = world.GetTileAt(X + 1, Y); //E
        tiles[2] = world.GetTileAt(X, Y - 1); //S
        tiles[3] = world.GetTileAt(X - 1, Y); //W

        if (diagOkay) {
            tiles[4] = world.GetTileAt(X+1, Y + 1); //NE
            tiles[5] = world.GetTileAt(X+1, Y - 1); //SE
            tiles[6] = world.GetTileAt(X-1, Y - 1); //SW
            tiles[7] = world.GetTileAt(X-1, Y + 1); //NW
        }
        
        //for (int i = X - 1; i < X+1; i++) {
        //    for (int j = 0; j < Y + 1; j++) {

        //    }
        //}
        
        return tiles;
    }

    public XmlSchema GetSchema()
    {
        return null;
    }


    public void WriteXml(XmlWriter writer) {
        writer.WriteAttributeString("X", X.ToString());
        writer.WriteAttributeString("Y", Y.ToString());
        writer.WriteAttributeString("Type", ((int)Type).ToString());
    }

    public void ReadXml(XmlReader reader) {
        //x & y already been set.
//        X = int.Parse(reader.GetAttribute("X"));
//        Y = int.Parse(reader.GetAttribute("Y"));
        Type = (TileType) int.Parse(reader.GetAttribute("Type"));
    }

    public ENTERABILITY IsEnterable() {

        if (movementCost == 0)
            return ENTERABILITY.Never;

        if (furniture != null && furniture.IsEnterable != null) {
            return furniture.IsEnterable(furniture);
        }

        return ENTERABILITY.Yes;
    }


    public Tile GetTileAt(Direction direction) {
        switch(direction) {
            case Direction.NORTH:
                return world.GetTileAt(X, Y + 1);
            case Direction.SOUTH:
                return world.GetTileAt(X, Y - 1);
            case Direction.EAST:
                return world.GetTileAt(X + 1, Y);
            case Direction.WEST:
                return world.GetTileAt(X - 1, Y);
        }
        return null;
    }


}
