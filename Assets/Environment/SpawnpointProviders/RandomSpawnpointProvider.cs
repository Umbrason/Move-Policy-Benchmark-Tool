
using UnityEngine;

public class RandomSpawnpointProvider : ISpawnpointProvider
{
    public Node GetNextSpawnpoint(Graph graph)
    {
        return graph.Nodes[Random.Range(0, graph.Nodes.Count)];
    }
}