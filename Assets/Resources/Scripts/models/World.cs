using UnityEngine;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class World : IXmlSerializable
{   

    public int Width { get; protected set; }


    public int Height{  get; protected set; }

    Tile[,] tiles;
    public List<Character> characters;
    public List<Furniture> furnitures;
    public List<Room> rooms;

    public InventoryManager inventoryManager;

    public PathTileGraph tileGraph;

    public Dictionary<string, Furniture> furnitureProto;
    public Dictionary<string, Job> furnitureJobPrototypes;
    private Action<Furniture> cbInstalledObjectCreated;
    private Action<Tile> cbTileChanged;
    private Action<Character> cbCharacterCreated;
    //private Action<Inventory> cbInventoryCreated;

    public JobQueue jobQueue;


    public Character character;

    public World(int width, int height) {
        SetupWorld(width, height);
        Tile t;
        if (characters.Count < 1) {
            t = tiles[Width / 2, Height / 2];
            CreateCharacter(t);
        }
        // make one character
        //Character c = CreateCharacter(GetTileAt(Width / 2, Height / 2));


        t = tiles[Width / 2 + 1, Height / 2 + 1];
        //inventoryManager.PlaceInventory(t, new Inventory());
    }


    public void SetupWorld(int w, int h)
    {
        jobQueue = new JobQueue();
        characters = new List<Character>();
        furnitures = new List<Furniture>();

        inventoryManager = new InventoryManager();

        rooms = new List<Room>();
        rooms.Add(new Room(this)); 
        Width = w;
        Height = h;

        tiles = new Tile[Width, Height];

        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                tiles[x, y] = new Tile(this, x, y);
                tiles[x, y].RegisterTileChangedCallback(OnTileChanged);
                tiles[x, y].room = GetOutsideRoom(); //room "0" is always outside
            }
        }
        tileGraph = new PathTileGraph(this);
        Debug.Log("world created with " + (Width * Height) + " tiles.");

        CreateFurniturePrototypes();


    }

    public void DeleteRoom( Room r) {
        if (r == GetOutsideRoom()) {
            Debug.LogError("Tried to delete outside room");
            return;
        }
        if (r == null) {
            Debug.LogError("Deleting a null room, WHAT AM I DOING?");
        }
        r.UnassignAllTiles();
        rooms.Remove(r);
    }

    public Room GetOutsideRoom() {
        return rooms[0];
    }

    public void Update(float deltaTime) {
        foreach (Character c in characters) {
            c.Update(deltaTime);
        }
        foreach (Furniture f in furnitures) {
            if(f.updateActions != null)
                f.Update(deltaTime);
        }
    }


    public Character CreateCharacter( Tile t) {
        Character c = new Character(t);
        character = c;
        characters.Add(c);
        if(cbCharacterCreated != null)
            cbCharacterCreated(c);

        return c;
    }


    void CreateFurniturePrototypes() {
        furnitureProto = new Dictionary<string, Furniture>();
        furnitureJobPrototypes = new Dictionary<string, Job>();
        
        furnitureProto.Add("wall", new Furniture("wall", 0f, 1, 1, true, true));
        furnitureJobPrototypes.Add("wall", 
            new Job(null, "wall", 
            FurnitureActions.JobComplete_FurnitureBuilding, 
            //null,
            1f, 
            new Inventory[] { new Inventory("steel plate", 5, 0) })
            );



        furnitureProto.Add("door", new Furniture("door", 1.5f, 1, 1, false, true));

        furnitureProto["door"].furnParameters["openness"] = 0;
        furnitureProto["door"].furnParameters["is_opening"] = 0;
        furnitureProto["door"].updateActions += FurnitureActions.Door_UpdateAction;

        furnitureProto["door"].IsEnterable = FurnitureActions.Door_IsEnterable;

        furnitureJobPrototypes.Add("door",
             new Job(null, "door", 
             FurnitureActions.JobComplete_FurnitureBuilding, 
             //null,
             1f, 
             new Inventory[] { new Inventory("steel plate", 5, 0)
             })
             );
        

        furnitureProto.Add("oxygen generator", new Furniture("oxygen generator", 
            10f, 
            2, 
            2, 
            false, 
            false));

        furnitureProto["oxygen generator"].updateActions += FurnitureActions.OxygenGenerator_UpdateAction;

        furnitureJobPrototypes.Add("oxygen generator",
             new Job(null, "oxygen generator",
             FurnitureActions.JobComplete_FurnitureBuilding,
             //null,
             1f,
           
             new Inventory[] { new Inventory()
             })
             );


        furnitureProto.Add("stockpile", 
            new Furniture(
                "stockpile", 
                0f, 
                1, 
                1, 
                true,
                false));

        furnitureProto["stockpile"].updateActions += FurnitureActions.Stockpile_UpdateAction;
        furnitureProto["stockpile"].movementCost = 1f;
        furnitureProto["stockpile"].tint = Color.blue;

        furnitureJobPrototypes.Add("stockpile",
            new Job(null, "stockpile", 
            FurnitureActions.JobComplete_FurnitureBuilding,
            //null,
            -1f, 
            null)
            );

    }

    public Tile GetTileAt(int x, int y) {
        if(x >= Width || x < 0 || y >= Height || y < 0 ) {
            return null;
        }
        return tiles[x, y];
    }

    public Tile GetNearestEmptyTile(Tile sourceTile)
    {
        Tile tile;

        //get all immediately nearby open tiles
        List<Tile> tiles = new List<Tile>();
        for (int i = sourceTile.X - 1; i < sourceTile.X + 1; i++)
        {
            for (int j = sourceTile.Y - 1; j < sourceTile.Y + 1; j++)
            {

                tile = GetTileAt(i, j);
                if (tile.IsEnterable() != ENTERABILITY.Never)
                {
                    tiles.Add(tile);
                }
            }
        }
        //TODO: Make it pick the safer tile (like the one inside instead of outside)
        if (tiles.Count == 0)
        {
            return null;
        }
        tile = tiles[0];
        return tile;
    }
       
    public Tile GetHomeSafeTile()
    {
        throw new NotImplementedException();
    }


    public void SetupPathFindingTest() {
        int l = Width / 2 - 5;
        int b = Height / 2 - 5;

        for (int x = l-5; x < l+15; x++) {
            for (int y = b-5; y < b+15; y++) {
                tiles[x, y].Type = TileType.Floor;

                if (x== 1 || x == (l+9) || y == 1 || y == b || y == (b + 9)) {
                    if(x != (l + 9) && y != (b + 4)) {
                        PlaceFurniture("wall", tiles[x,y]);
                    }
                }
            }

        }
        
    }


    public void RandomiseTiles() {
        Debug.Log("Randomising Tiles");
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                if (UnityEngine.Random.Range(0, 2) == 0) {
                    tiles[x, y].Type = TileType.Empty;
                }
                else {
                    tiles[x, y].Type = TileType.Floor;
                }
            }
        }
    }


    public Furniture PlaceFurniture(string objectBuildType, Tile t) {
        //Debug.Log("Place Installed Object");
        if (furnitureProto.ContainsKey(objectBuildType) == false) {
            Debug.LogError("installedObjectProto does not contain key: " + objectBuildType);
            return null;
        }

        Furniture furniture = Furniture.PlaceInstance(furnitureProto[objectBuildType], t);

        if(furniture == null) {
            //failed to place object
           // Debug.LogError("PlaceInstalledObject - failed to place object");
            return null;
        }
        furniture.RegisterOnRemovedCallback(OnFurnitureRemoved);
        furnitures.Add(furniture);



        if(cbInstalledObjectCreated != null) {
            cbInstalledObjectCreated(furniture);
            if (furniture.movementCost != 1) {
                // because furniture movement cost multplies tile cost, anything other than 1 will change graph
                InvalidateTileGraph();
            }
            //do we need to recalculate rooms?
            if (furniture.roomEnclosing) {
                //do flood fill for rooms!
                Room.ReallocateRooms(furniture.tile);
            }
        }

        return furniture;
    }



    public void PlaceInventory()
    {
        //return 
        PlaceInventory(tiles[50, 50], new Inventory("steel plate", 50, 10));
    }

    public bool PlaceInventory(Tile tile, Inventory inventory)
    {

        if (inventoryManager.PlaceInventory(tile, inventory)) {
            //cbInventoryCreated(inventory);
            return true;
        }
        else
            return false;
    }
    public bool PlaceInventory(Job job, Inventory inventory)
    {

        if (inventoryManager.PlaceInventory(job, inventory)) {
            //cbInventoryCreated(inventory);
            return true;
        }else 
            return false;
    }



    public void RegisterFurnitureCreated(Action<Furniture> callbackFunc) {
        cbInstalledObjectCreated += callbackFunc;
    }

    public void UnregisterFurnitureCreated(Action<Furniture> callbackFunc) {
        cbInstalledObjectCreated -= callbackFunc;
    }

    public void RegisterCharacterCreated(Action<Character> callbackFunc) {
        cbCharacterCreated += callbackFunc;
    }

    public void UnregisterCharacterCreated(Action<Character> callbackFunc) {
        cbCharacterCreated -= callbackFunc;
    }

    public void RegisterTileChanged(Action<Tile> onTileChanged) {
        cbTileChanged += onTileChanged;
    }
    public void UnregisterTileChanged(Action<Tile> onTileChanged) {
        cbTileChanged -= onTileChanged;
    }
    
    public void OnFurnitureRemoved(Furniture furn)
    {
        furn.UnregisterOnRemovedCallback(OnFurnitureRemoved);
        furnitures.Remove(furn);
    }

    private void OnTileChanged(Tile tile) {
        if (cbTileChanged == null) {
            return;
        }

        cbTileChanged(tile);

        InvalidateTileGraph();
    }

    public void InvalidateTileGraph() {
        tileGraph = null;
    }

    public bool IsFurniturePlacementValid(string furnitureType, Tile t) {
        return furnitureProto[furnitureType].IsValidPosition(t);
    }


    public Furniture GetFurniturePrototype(string objectType) {
        if(furnitureProto.ContainsKey(objectType) == false) {

            return null;
        }
        return furnitureProto[objectType];
    }


    /// /////////////////
    /// 
    /// SAVING AND LOADING
    /// 
    /// /////////////////

    public World() {
    }

    public XmlSchema GetSchema()
    {
        return null;
    }


    public void WriteXml(XmlWriter writer)
    {
        //saves
        writer.WriteAttributeString("Width", Width.ToString());
        writer.WriteAttributeString("Height", Height.ToString());

        writer.WriteStartElement("Tiles");
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++)
            {
                writer.WriteStartElement("Tile");
                tiles[x,y].WriteXml(writer);
                writer.WriteEndElement();
            }

        }
        writer.WriteEndElement();


        writer.WriteStartElement("Furnitures");
        foreach (Furniture furniture in furnitures) {
            writer.WriteStartElement("Furniture");
            furniture.WriteXml(writer);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();

        writer.WriteStartElement("Characters");
        foreach (Character charac in characters)
        {
            writer.WriteStartElement("Character");
            character.WriteXml(writer);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();

    }

    public void ReadXml(XmlReader reader) {
        //loads
        int w = int.Parse(reader.GetAttribute("Width"));
        int h = int.Parse(reader.GetAttribute("Height"));
        SetupWorld(w,h);
        
        while (reader.Read()) {
            switch (reader.Name) {
                case "Tiles":
                    ReadXml_Tiles(reader);
                    break;
                case "Furnitures":
                    ReadXml_Furnitures(reader);
                    break;
                case "Characters":
                    ReadXml_Characters(reader);
                    break;
            }
        }
        InvalidateTileGraph();


        //// DEBUGGING ONLY!  REMOVE ME LATER!
        //// Create an Inventory Item
        //Inventory inv = new Inventory();
        //inv.stackSize = 10;
        //Tile t = GetTileAt(Width / 2, Height / 2);
        //inventoryManager.PlaceInventory(t, inv);
        //inv.tile = t;
        ////if (cbInventoryCreated != null) {
        ////    cbInventoryCreated(t.inventory);
        ////}

        //inv = new Inventory();
        //inv.stackSize = 18;
        //t = GetTileAt(Width / 2 + 2, Height / 2);
        //inventoryManager.PlaceInventory(t, inv);
        //inv.tile = t;
        ////if (inventoryManager != null) {
        ////    cbInventoryCreated(t.inventory);
        ////}

        //inv = new Inventory();
        //inv.stackSize = 45;
        //t = GetTileAt(Width / 2 + 1, Height / 2 + 2);
        //inv.tile = t;
        //inventoryManager.PlaceInventory(t, inv);
        ////if (cbInventoryCreated != null) {
        ////    cbInventoryCreated(t.inventory);
        ////}
    }


    private void ReadXml_Characters(XmlReader reader)
    {
        if (reader.ReadToDescendant("Character"))
        {
            do {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));
                Character c = CreateCharacter(tiles[x, y]);
                c.ReadXml(reader);
            } while (reader.ReadToNextSibling("Character"));
        }
    }

    private void ReadXml_Furnitures(XmlReader reader)
    {

        if (reader.ReadToDescendant("Character")) {
            do {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));

                Furniture furn = PlaceFurniture(reader.GetAttribute("objectType"), tiles[x, y]);
                furn.ReadXml(reader);
            } while (reader.ReadToNextSibling("Character"));
        }
    }

    private void ReadXml_Tiles(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.Name != "Tile")
                return;
            int x = int.Parse(reader.GetAttribute("X"));
            int y = int.Parse(reader.GetAttribute("Y"));
            
            tiles[x, y].ReadXml(reader);
        }
    }
}