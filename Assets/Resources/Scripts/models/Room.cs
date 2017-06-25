using UnityEngine;
using System.Collections.Generic;

public class Room  {


    public float atmosO2 = 0;
    private List<Tile> tiles;

    public Room() {
        tiles = new List<Tile>();
    }


    public void AssignTile( Tile t) {
        if (tiles.Contains(t) )
            return;

        if (t.room != null)
            t.room.tiles.Remove(t);

        t.room = this;
        tiles.Add(t);
    }


    public void UnassignAllTiles() {
        for (int i = 0; i < tiles.Count; i++) {
            tiles[i].room = tiles[i].world.GetOutsideRoom();
        }
        tiles = new List<Tile>();
    }

    public static void ReallocateRooms(Furniture sourceFurniture) {
        //sourceFurniture is the piece of furniture that is causing the "change" in room topography

        //no flood fill ON source tile.
        //start points on N S W E

        World world = sourceFurniture.tile.world;

        Room oldRoom = sourceFurniture.tile.room;

        //try building new room from north;
        foreach (Tile t in sourceFurniture.tile.GetNeighbours() ) {
            FloodFill( t, oldRoom);
        }

        sourceFurniture.tile.room = null;
        oldRoom.tiles.Remove(sourceFurniture.tile);


        if (oldRoom != world.GetOutsideRoom()) {
            //shouldnt have any tiles left in it now - if it does - it's still a room?

            world.DeleteRoom(oldRoom);
        }


    }



    protected static void FloodFill(Tile tile, Room oldRoom) {
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


        Room newRoom = new Room();
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
}
