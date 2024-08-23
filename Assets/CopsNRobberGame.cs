using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopsNRobberGame
{
    public readonly Graph graph = new(new());
    public readonly List<Team> teams = new();
    public Team Robbers => teams[0];
    public Team Cops => teams[1];
    public readonly Dictionary<Team, ITeamStrategy> strategies = new();
    public readonly ISpawnpointProvider provider = new RandomSpawnpointProvider();

    public CopsNRobberGame(Graph graph, int robberCount, int copCount, Dictionary<Team, ITeamStrategy> strategies)
    {
        this.graph = graph;
        this.teams = new List<Team>() {
            new(new(), Color.red, "Robbers"),
            new(new(), Color.blue, "Cops")
        };
        for (int r = 0; r < robberCount; r++) Robbers.Agents.Add(new Robber());
        for (int c = 0; c < copCount; c++) Cops.Agents.Add(new Cop());
        this.strategies = strategies;
    }

    public void InitAgents()
    {
        foreach (var team in teams)
            foreach (var agent in team.Agents)
            {
                agent.Reset();
                agent.SetPosition(provider.GetNextSpawnpoint(graph));
            }
    }

    public void InitStrategies()
    {
        foreach (var team in teams) strategies.GetValueOrDefault(team)?.Init();
    }

    public void TickStrategies()
    {
        foreach (var team in teams) strategies.GetValueOrDefault(team)?.Tick();
    }
}