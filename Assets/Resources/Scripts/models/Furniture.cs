using UnityEngine;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public enum Direction
{
    NORTH, SOUTH, EAST, WEST
};

public class Furniture : IXmlSerializable
{

    public Action<Furniture, float> updateActions;
    public Dictionary<string, float> furnParameters;

    public Func<Furniture, ENTERABILITY> IsEnterable;

    //float openness = 0; //0 is closed door 1= fully open door. in between is partially open
    ////this represents the bas object, but a large object can occupy multiple tiles.
    //bool doorIsOpening = false;
    //float doorOpenTime = 0.25f;
    
    public Tile tile
    {
        get; protected set;
    }

    public string objectType { get; protected set; }


    //this is a multiplier of cost. so 2 would mean you cwould move twice as slow.
    //tile types and other environmental effects would stack with this.
    // if == 0, then this tile is impassible.
    public float movementCost = 1f;
    public bool roomEnclosing;

    // eg sofa 3x2, graphis 3x1 use area extra row.
    // TODO: allow for weird shapes here?
    public int Width { get; protected set; }
    public int Height { get; protected set; }

    public Color tint = Color.white;

    public Action<Furniture> cbOnChanged;
    public Action<Furniture> cbOnRemoved;

    Func<Tile, bool> funcPositionValidation;

    public bool linksToNeighbour { get; protected set; }

    List<Job> jobs;


    public Furniture() {
        this.furnParameters = new Dictionary<string, float>();
        jobs = new List<Job>();
    }

    public Furniture(Furniture other) {
        this.objectType = other.objectType;
        this.movementCost = other.movementCost;
        this.roomEnclosing = other.roomEnclosing;
        this.Width = other.Width;
        this.Height = other.Height;
        this.tint = other.tint;
        this.linksToNeighbour = other.linksToNeighbour;

        this.jobs = new List<Job>();

        this.furnParameters = new Dictionary<string, float>(other.furnParameters);

        if (other.updateActions != null )
            this.updateActions = (Action<Furniture, float>)other.updateActions;

        if (other.funcPositionValidation != null)
            this.funcPositionValidation = (Func<Tile, bool>)other.funcPositionValidation.Clone();


        this.IsEnterable = other.IsEnterable;

    }

    public Furniture(string objectType, float movementCost = 1f, int width = 1, int height = 1, bool linksToNeighbour = false, bool roomEnclosing= false) {
        //Furniture obj = new Furniture();
        this.objectType = objectType;
        this.movementCost = movementCost;
        this.roomEnclosing = roomEnclosing;
        this.Width = width;
        this.Height = height;
        this.linksToNeighbour = linksToNeighbour;
        
        this.furnParameters = new Dictionary<string, float>();
        this.funcPositionValidation += this.__IsValidPosition;
        
    }


    public virtual Furniture Clone() {
        return new Furniture(this);
    }

    public void Update(float deltaTime)
    {
        //if (doorIsOpening) {
        //    openness += deltaTime / doorOpenTime;
        //} else {
        //    openness -= deltaTime / doorOpenTime; 
        //}

        //openness = Mathf.Clamp01(openness);

        if (updateActions != null)
            updateActions(this, deltaTime);

    }

    public static Furniture PlaceInstance(Furniture proto, Tile tile) {

        if( ! proto.funcPositionValidation(tile)) {
            Debug.LogError("Place instance - couldn't place item here");
            return null;
        }

        //now know it's valid.
        Furniture obj = (Furniture)proto.Clone();
        //obj.objectType = proto.objectType;
        //obj.movementCost = proto.movementCost;
        //obj.width = proto.width;
        //obj.height = proto.height;
        //obj.linksToNeighbour = proto.linksToNeighbour;

        obj.tile = tile;

        if (!tile.InstallFurniture(obj)) {
            //we couldn't place the object here. Probs already occupied


            return null;
        }

        if (obj.linksToNeighbour) {
            int x = tile.X;
            int y = tile.Y;
            Tile t = tile.world.GetTileAt(x, y + 1);

            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType) {
                t.furniture.cbOnChanged(t.furniture);
            }
            t = tile.world.GetTileAt(x + 1, y);
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType) {
                t.furniture.cbOnChanged(t.furniture);
            }
            t = tile.world.GetTileAt(x, y - 1);
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType) {
                t.furniture.cbOnChanged(t.furniture);
            }
            t = tile.world.GetTileAt(x - 1, y);
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType) {
                t.furniture.cbOnChanged(t.furniture);
            }

        }

        return obj;
    }


    public void RegisterOnChangedCallback(Action<Furniture> callbackFunc)
    {
        cbOnChanged += callbackFunc;
    }
    public void UnregisterOnChangedCallback(Action<Furniture> callbackFunc)
    {
        cbOnChanged -= callbackFunc;
    }
    public void RegisterOnRemovedCallback(Action<Furniture> callbackFunc)
    {
        cbOnRemoved += callbackFunc;
    }
    public void UnregisterOnRemovedCallback(Action<Furniture> callbackFunc)
    {
        cbOnRemoved -= callbackFunc;
    }


    public bool IsValidPosition(Tile t) {
        return funcPositionValidation(t);
    }

    public bool __IsValidPosition(Tile t) {
        //make sure tileis floor.
        //make sure tile doesnt have furniture.

        for (int x_off = t.X; x_off < t.X + Width; x_off++)
        {
            for (int y_off = t.Y; y_off < t.Y + Height; y_off++)
            {
                Tile t2 = t.world.GetTileAt(x_off, y_off);
                 
                if (t2.Type != TileType.Floor)
                {
                    return false;
                }
                else if (t2.furniture != null)
                {
                    return false;
                }
            }
        }
               
        return true;
    }

    public bool IsValidPosition_Door(int x, int y) {
        //check for e-w wall or n-s wall.
        return true;
    }

    public XmlSchema GetSchema()
    {
        return null;
    }


    public void WriteXml(XmlWriter writer) {
        writer.WriteAttributeString("X", tile.X.ToString());
        writer.WriteAttributeString("Y", tile.Y.ToString());
        writer.WriteAttributeString("objectType", objectType);
        //writer.WriteAttributeString("movementCost", movementCost.ToString());

        foreach (string k in furnParameters.Keys) {
            writer.WriteStartElement("Params");
            writer.WriteAttributeString("name", k);
            writer.WriteAttributeString("value", furnParameters[k].ToString());
            writer.WriteEndElement();
        }

    }

    public void ReadXml(XmlReader reader) {
        //x & y already been set.
        //        X = int.Parse(reader.GetAttribute("X"));
        //        Y = int.Parse(reader.GetAttribute("Y"));
//        objectType = reader.GetAttribute("objectType");
        //movementCost = float.Parse(reader.GetAttribute("movementCost"));

        if(reader.ReadToDescendant("Params")) {
            do {
                string k = reader.GetAttribute("Name");
                float v = float.Parse(reader.GetAttribute("value"));
                furnParameters.Add(k, v);
            } while (reader.ReadToNextSibling("Params"));
        }

    }

    public int JobCount()
    {
        return jobs.Count;
    }

    public void AddJob(Job j)
    {
        jobs.Add(j);
        tile.world.jobQueue.Enqueue(j); //make sure wrld knows too.
    }

    public void RemoveJob(Job j)
    {
        jobs.Remove(j);
        j.CancelJob();
        //tile.world.jobQueue.Remove(j);
    }

    public void ClearJobs()
    {
        foreach (Job job in jobs) {
            RemoveJob(job);
        }
    }


    public bool IsStockpile()
    {
        return objectType.ToLower().Equals("stockpile");
    }

    public void Deconstruct()
    {
        tile.UnplaceFurniture();

        if (cbOnRemoved != null)
            cbOnRemoved(this);

        if(roomEnclosing)
        {
            Room.ReallocateRooms(tile);
        }
        
    }
}
