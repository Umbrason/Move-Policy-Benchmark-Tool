using System.Collections.Generic;

public static class AStar
{
    public static List<Node> GetPath(Graph graph, Node from, Node to)
    {
        SortedList<float, Node> ExploreQueue = new(new DuplicateKeyComparer<float>()) { { 0, from } };
        Dictionary<Node, Node> predecessors = new() { { from, from } };

        Dictionary<Node, float> Costs = new();
        foreach (var node in graph.Nodes) Costs[node] = float.PositiveInfinity;
        Costs[from] = 0;

        while (ExploreQueue.Count > 0)
        {
            var current = ExploreQueue.Values[0];
            ExploreQueue.RemoveAt(0);
            if (current == to) return Reconstruct(predecessors, current);
            foreach (var neighbour in current.Neighbours)
            {
                var nbCost = Costs[current] + CalcCost(current, neighbour);
                if (nbCost >= Costs[neighbour]) continue;
                predecessors[neighbour] = current;
                Costs[neighbour] = nbCost;
                var estimation = nbCost + EstimateDistance(neighbour, to);
                if (!ExploreQueue.ContainsValue(neighbour)) ExploreQueue.Add(estimation, neighbour);
            }
        }
        return null;
    }

    private static float EstimateDistance(Node from, Node to)
    {
        return (from.position - to.position).magnitude;
    }

    private static float CalcCost(Node from, Node to)
    {
        return from.Distance(to);
    }

    private static List<Node> Reconstruct(Dictionary<Node, Node> predecessors, Node node)
    {
        var path = new List<Node>();
        do
        {
            path.Insert(0, node);
            node = predecessors[node];
        } while (predecessors[node] != node);
        return path;
    }
}