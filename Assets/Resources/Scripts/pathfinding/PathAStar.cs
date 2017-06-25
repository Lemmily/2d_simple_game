using System;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
using System.Linq;

public class PathAStar {

    public Queue<Tile> path;

    public PathAStar(World world, Tile tileStart, Tile tileEnd) {
        if (world.tileGraph == null)
            world.tileGraph = new PathTileGraph(world);


        Dictionary<Tile, PathNode<Tile>> nodes = world.tileGraph.tileToNode;
        if (!nodes.ContainsKey(tileStart) || !nodes.ContainsKey(tileEnd)) {
            Debug.Log("Start or end tile isnt in the graph!");
            return;
        }

        PathNode<Tile> start = nodes[tileStart];
        PathNode<Tile> goal = nodes[tileEnd];


        List<PathNode<Tile>> closedSet = new List<PathNode<Tile>>();
        SimplePriorityQueue<PathNode<Tile>> openSet = new SimplePriorityQueue<PathNode<Tile>>();
        openSet.Enqueue(start, 0);

        Dictionary<PathNode<Tile>, PathNode<Tile>> cameFrom = new Dictionary<PathNode<Tile>, PathNode<Tile>>();


        Dictionary<PathNode<Tile>, float> g_score = new Dictionary<PathNode<Tile>, float>();

        foreach (PathNode<Tile>  n in nodes.Values) {
            g_score[n] = Mathf.Infinity;
        }
        g_score[start] = 0;


        Dictionary<PathNode<Tile>, float> f_score = new Dictionary<PathNode<Tile>, float>();

        foreach (PathNode<Tile> n in nodes.Values) {
            f_score[n] = Mathf.Infinity;
        }
        f_score[nodes[tileStart]] = heuristic(start, goal);


        while (openSet.Count > 0) {
            PathNode<Tile> current = openSet.Dequeue();
            if (current == goal) {
                ReconstructPath(cameFrom, current);
                return;
            }
            closedSet.Add(current);

            foreach (PathEdge<Tile> neighbour in current.edges) {
                if (closedSet.Contains(neighbour.node) == true )
                    continue;

                float movementCostToNeighbor = neighbour.node.data.movementCost * dist_between(current, neighbour.node);
                float tentativeGScore = g_score[current] + movementCostToNeighbor;
                if (openSet.Contains(neighbour.node) && tentativeGScore >= g_score[neighbour.node])
                    continue;

                cameFrom[neighbour.node] = current;
                g_score[neighbour.node] = tentativeGScore;
                f_score[neighbour.node] = g_score[neighbour.node] + heuristic(neighbour.node, goal);

                if (openSet.Contains(neighbour.node) == false) {
                    openSet.Enqueue(neighbour.node, f_score[neighbour.node]);
                } else {
                    openSet.UpdatePriority(neighbour.node, f_score[neighbour.node]);
                }
            }

        }

        //No path!
        Debug.Log("No path found, searched " + closedSet.Count + "tiles");




    }

    private void ReconstructPath(Dictionary<PathNode<Tile>, PathNode<Tile>> cameFrom, PathNode<Tile> current) {
        
        Queue<Tile> properPath = new Queue<Tile>();
        properPath.Enqueue(current.data);
        
        while ( current != null) {
            if (! cameFrom.ContainsKey(current)) {
                current = null;
                continue;
            }
            current = cameFrom[current];
            properPath.Enqueue(current.data);
        }

        
        path = new Queue<Tile>( properPath.Reverse());
        Debug.Log("Created a path that was " + path.Count + " tiles long") ;
    }


    private float heuristic(PathNode<Tile> start, PathNode<Tile> end) {
        return Mathf.Sqrt(Mathf.Pow(start.data.X - end.data.X, 2) + Mathf.Pow(start.data.Y - end.data.Y, 2));
    }

    private float dist_between(PathNode<Tile> a, PathNode<Tile> b) {
        if (Mathf.Abs(a.data.X - b.data.X) + Mathf.Abs(a.data.Y - b.data.Y) == 1)
            return 1;

        if(Mathf.Abs(a.data.X - b.data.X) == 1 && Mathf.Abs(a.data.Y - b.data.Y) == 1) {
            return 1.4142f;
        }

        return heuristic(a, b);
    }

    public Tile Dequeue() {
        return path.Dequeue();
    }

    public int Length() {
        if (path == null)
            return 0;
        return path.Count;
    }

}