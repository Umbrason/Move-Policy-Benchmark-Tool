public class RandomSpawnpointProvider : ISpawnpointProvider
{
    public Node GetNextSpawnpoint(Graph graph, Team team)
    {
        var random = new System.Random();
        return graph.Nodes[random.Next(0, graph.Nodes.Length)];
    }
}