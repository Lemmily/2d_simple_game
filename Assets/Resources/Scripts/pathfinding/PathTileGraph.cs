using UnityEngine;
using System.Collections.Generic;

public class PathTileGraph {


    public Dictionary<Tile, PathNode<Tile>> tileToNode;


    public PathTileGraph(World world) {

        tileToNode = new Dictionary<Tile, PathNode<Tile>>();

        for (int x = 0; x < world.Width; x++) {
            for (int y = 0; y < world.Height; y++) {
                Tile t = world.GetTileAt(x, y);
                //if(t.movementCost > 0) {
                    tileToNode.Add(t, new PathNode<Tile>(t));
                //}
            }
        }
        Debug.Log("Created " + tileToNode.Count + " nodes");

        int edgeCount = 0;
        foreach (Tile tile in tileToNode.Keys) {
            PathNode<Tile> n = tileToNode[tile];
            List<PathEdge<Tile>> edges = new List<PathEdge<Tile>>();

            Tile[] neighbs = tile.GetNeighbours(true);
            for (int i = 0; i < neighbs.Length; i++) {
                if(neighbs[i] != null && neighbs[i].movementCost > 0) {


                    //make sure we aren't clipping diag.
                    if(IsClippingCorner(tile, neighbs[i])) {
                        continue;
                        //don't make any edges.
                    }

                    PathEdge<Tile> e = new PathEdge<Tile>
                    {
                        cost = neighbs[i].movementCost,
                        node = tileToNode[neighbs[i]]
                    };
                    edges.Add(e);
                    edgeCount++;
                }
            }
            n.edges = edges.ToArray();
        }
        Debug.Log("Created " + edgeCount + " edges");
    }


    bool IsClippingCorner(Tile currTile, Tile neigh) {
        //if this movement is diagonal eg NE
        //and if the two adjacentneighbours are empty eg N & E
        //return true 
        if (Mathf.Abs(currTile.X - neigh.X) == 1 && Mathf.Abs(currTile.Y - neigh.Y) == 1) {
            int dX = currTile.X - neigh.X;
            int dY = currTile.Y - neigh.Y;

            if (currTile.world.GetTileAt(currTile.X - dX, currTile.Y).movementCost == 0f) {
                return true;
            }
            if (currTile.world.GetTileAt(currTile.X , currTile.Y - dY).movementCost == 0f) {
                return true;
            }

        }


        return false;
    }

}
