using System.Collections.Generic;

public class ArraySpawnpointProvider : ISpawnpointProvider
{
    private readonly Dictionary<Team, int[]> startPositions;
    private readonly Dictionary<Team, int> startPositionIndex = new();

    public ArraySpawnpointProvider(Dictionary<Team, int[]> startPositions)
    {
        this.startPositions = startPositions;
    }

    public Node GetNextSpawnpoint(Graph graph, Team team)
    {
        var i = startPositionIndex.GetValueOrDefault(team);
        startPositionIndex[team] = i + 1;
        return graph.Nodes[startPositions[team][i]];
    }
}
