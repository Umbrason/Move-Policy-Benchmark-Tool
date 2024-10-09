using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public readonly int index;
    public readonly Vector2Int position;
    public readonly Node[] Neighbours = new Node[8];
    public int neighbourCount = 0;
    public readonly List<Agent> Occupants = new();
    public Node(Vector2Int position, int index)
    {
        this.position = position;
        this.index = index;
    }
}