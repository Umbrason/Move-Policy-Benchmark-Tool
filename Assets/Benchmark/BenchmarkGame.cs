using System;
using System.Collections.Generic;
using System.Linq;

public class BenchmarkGame
{
    public readonly Graph graph;
    public readonly List<Cop> cops;
    public readonly List<Robber> robbers;
    public readonly int copSpeed;
    public readonly int robberSpeed;
    public readonly int turnLimit;
    public Result result;

    public CopsNRobberGame Game { get; private set; }


    public BenchmarkGame(Graph graph, int[] copPositions, int[] robberPositions, int turnLimit = 100, int copSpeed = 1, int robberSpeed = 1)
    {
        this.graph = graph;
        this.turnLimit = turnLimit;
        this.copSpeed = copSpeed;
        this.robberSpeed = robberSpeed;
        result.copStartPositions = copPositions;
        result.robberStartPositions = robberPositions;
        Game = new(graph, copPositions.Length, robberPositions.Length);
        Game.strategies[Game.Cops] = new AssignedTargetCoverGradientDescent(Game, new CoverMinimizeAssignment(CoverMinimizeAssignment.Metric.Max, Game));
        Game.strategies[Game.Robbers] = new MultiagentTrailmax(Game);
        Game.spawnpointProvider = new ArraySpawnpointProvider(new() {
            { Game.Cops, copPositions },
            { Game.Robbers, robberPositions }
        });
    }

    public void Run()
    {
        Game.InitAgents();
        if (!Game.Finished) Game.InitStrategies(); //avoid issues in the rare case where the starting configuration already makes the cops win
        while (!Game.Finished && result.turns <= turnLimit)
        {
            Game.TickStrategies();
            result.turns++;
        }
        result.success = Game.Finished;
        Game.ResetAgents();
    }

    public struct Result
    {
        public int turns;
        public bool success;
        public int[] copStartPositions;
        public int[] robberStartPositions;
        public override readonly string ToString()
        {
            return $"{turns}, {success}, [{string.Join(",", copStartPositions)}], [{string.Join(",", robberStartPositions)}]";
        }
    }
}
