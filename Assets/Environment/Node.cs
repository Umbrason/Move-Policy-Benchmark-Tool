using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector2Int position;
    public readonly List<Node> Neighbours = new();
    public readonly List<Agent> Occupants = new();
    public Node(Vector2Int position)
    {
        this.position = position;
    }
}

public static class NodeExtensions
{
    public static float Distance(this Node node, Node other)
    {
        if (node.Neighbours.Contains(other)) return 1f; // should hopefully save on some mem allocs
        var dfsQueue = new SortedList<float, Node>() { { 0, node } };
        var visited = new HashSet<Node>();
        while (dfsQueue.Count > 0)
        {
            var curNode = dfsQueue.Values[0];
            var curCost = dfsQueue.Keys[0];
            if (curNode == other) return curCost;
            visited.Add(curNode);
            dfsQueue.RemoveAt(0);
            foreach (var nb in curNode.Neighbours)
                dfsQueue.Add(curCost + 1f, nb);
        }
        return float.PositiveInfinity; //infinity = not reachable
    }
}