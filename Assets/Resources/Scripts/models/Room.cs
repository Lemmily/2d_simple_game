using UnityEngine;
using System.Collections.Generic;
using System;

public class Room  {


    public float atmosO2 = 0;

    Dictionary<string, float> atmosphericGasses;

    private List<Tile> tiles;
    public World world;

    public Room(World world) {
        this.world = world;
        tiles = new List<Tile>();
        atmosphericGasses = new Dictionary<string, float>();
    }


    public void AssignTile( Tile t) {
        if (tiles.Contains(t) )
            return;

        if (t.room != null)
            t.room.tiles.Remove(t);

        t.room = this;
        tiles.Add(t);
    }


    public void ChangeGas(string name, float amount)
    {
        if(IsOutside())
        {
            return;
        }
        if (atmosphericGasses.ContainsKey(name))
        {
            atmosphericGasses[name] += amount;
        }
        else
        {
            atmosphericGasses[name] = amount;
        }

        if (atmosphericGasses[name] < 0)
            atmosphericGasses[name] = 0;
    }

    internal IEnumerable<string> GetGasNames()
    {
        return atmosphericGasses.Keys;
    }

    public bool IsOutside()
    {
        return this == WorldController.Instance.world.GetOutsideRoom();
    }

    public float GetGasAmount(string name)
    {
        if (!atmosphericGasses.ContainsKey(name))
        {
            return 0f;
        }
        return atmosphericGasses[name];
    }

    public float GetGasPercentage(string name)
    {
        if (!atmosphericGasses.ContainsKey(name) || atmosphericGasses[name] <= 0)
        {
            return 0f;
        }

        float t = 0;
        //cycle through all gases, tally, and find out percentage.
        foreach (string n in atmosphericGasses.Keys)
        {
            t += atmosphericGasses[n];
        }


        
        return atmosphericGasses[name] / t;
    }

    public void UnassignAllTiles() {
        for (int i = 0; i < tiles.Count; i++) {
            tiles[i].room = tiles[i].world.GetOutsideRoom();
        }
        tiles = new List<Tile>();
    }

    public static void ReallocateRooms(Tile sourceTile) {
        //sourceFurniture is the piece of furniture that is causing the "change" in room topography

        //no flood fill ON source tile.
        //start points on N S W E

        World world = sourceTile.world;

        Room oldRoom = sourceTile.room;

        //try building new room from north;
        foreach (Tile t in sourceTile.GetNeighbours() ) {
            FloodFill( t, oldRoom);
        }

        sourceTile.room = null;
        oldRoom.tiles.Remove(sourceTile);


        if (oldRoom.IsOutside() == false) {
            //shouldnt have any tiles left in it now - if it does - it's still a room?

            world.DeleteRoom(oldRoom);
        }


    }



    public static void FloodFill(Tile tile, Room oldRoom) {
        if (tile == null)
            return;

        if (tile.room != oldRoom) {
            //tile has already been processed?
            //what if you removed a room?
            return;
        }

        //tile has a furniture that is room enclosing - ignore!
        if (tile.furniture != null && tile.furniture.roomEnclosing) 
            return;

        if (tile.Type == TileType.Empty) {
            // This tile is empty space and must remain part of the outside.
            return;
        }


        Room newRoom = new Room(oldRoom.world);
        Queue<Tile> tilesToCheck = new Queue<Tile>();
        tilesToCheck.Enqueue(tile);

        while (tilesToCheck.Count > 0) {
            Tile t = tilesToCheck.Dequeue();
            if (t.room == oldRoom) {
                newRoom.AssignTile(t);
                foreach (Tile t2 in t.GetNeighbours()) {
                    if (t2 == null || t2.Type == TileType.Empty) {
                        newRoom.UnassignAllTiles();
                        return;
                    }

                    if ( t2.room == oldRoom && (t2.furniture == null || t2.furniture.roomEnclosing == false))
                        tilesToCheck.Enqueue(t2);
                }
            }
        }

        if (newRoom.tiles.Count > 0)
            tile.world.rooms.Add(newRoom);
    }


    void CopyGas(Room other)
    {
        foreach (string n in other.atmosphericGasses.Keys)
        {
            this.atmosphericGasses[n] = other.atmosphericGasses[n];
        }
    }
}
