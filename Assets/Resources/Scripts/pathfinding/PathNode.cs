using UnityEngine;
using System.Collections;

public class PathNode<T> {
    public T data;

    public PathEdge<T>[] edges;


    public PathNode(T data) {
        this.data = data;
    }

    public override string ToString()
    {
        return "PathNode: " + data;
    }
}
