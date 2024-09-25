using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CopsNRobberGame
{
    public readonly Graph graph = new(new());
    public readonly List<Team> teams = new();
    public Team Robbers => teams[0];
    public Team Cops => teams[1];
    public ITeamStrategy CopStrategy { get => strategies[Cops]; set => strategies[Cops] = value; }
    public ITeamStrategy RobberStrategy { get => strategies[Robbers]; set => strategies[Robbers] = value; }
    public readonly Dictionary<Team, ITeamStrategy> strategies = new();
    public readonly Dictionary<Team, int> teamSpeed = new();
    public ISpawnpointProvider spawnpointProvider;

    public int RobbersCaught => Robbers.Agents.Count(agent => (agent as Robber).Caught);
    public bool Finished => Robbers.Agents.All(agent => (agent as Robber).Caught);

    public CopsNRobberGame(Graph graph, int copCount = 1, int robberCount = 1, ISpawnpointProvider spawnpointProvider = null)
    {
        this.graph = graph;
        this.teams = new List<Team>() {
            new(new(), Color.red, "Robbers"),
            new(new(), Color.blue, "Cops")
        };
        this.strategies = new() {
            { Robbers, new ManualStrategy(Robbers) },
            { Cops, new ManualStrategy(Cops) }
        };
        this.teamSpeed = new()
        {
            { Robbers, 1 },
            { Cops, 1 }
        };
        for (int r = 0; r < robberCount; r++) Robbers.Agents.Add(new Robber());
        for (int c = 0; c < copCount; c++) Cops.Agents.Add(new Cop());
        this.spawnpointProvider = spawnpointProvider ?? new RandomSpawnpointProvider();
    }


    public void InitAgents()
    {
        foreach (var team in teams)
            foreach (var agent in team.Agents)
            {
                agent.Reset();
                agent.SetPosition(spawnpointProvider.GetNextSpawnpoint(graph, team));
            }
    }

    public void InitStrategies()
    {
        foreach (var team in teams) strategies.GetValueOrDefault(team)?.Init();
    }

    public void TickStrategies()
    {
        foreach (var team in teams)
            for (int i = 0; i < teamSpeed[team]; i++)
                strategies.GetValueOrDefault(team)?.Tick();
    }

    public void ResetAgents()
    {
        foreach (var team in teams)
            foreach (var agent in team.Agents)
                agent.Reset();
    }
}