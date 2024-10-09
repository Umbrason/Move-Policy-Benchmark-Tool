
using UnityEngine;

public class RandomSpawnpointProvider : ISpawnpointProvider
{
    public Node GetNextSpawnpoint(Graph graph, Team team)
    {
        return graph.Nodes[Random.Range(0, graph.Nodes.Length)];
    }
}