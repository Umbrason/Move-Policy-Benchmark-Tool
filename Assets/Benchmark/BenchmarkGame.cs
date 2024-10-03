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
    public readonly CopStrategy copStrategy;
    public Result result;


    public CopsNRobberGame Game { get; private set; }

    public enum CopStrategy
    {
        STMTAStar,
        TRAP_Max,
        TRAP_Sum,
    }


    public BenchmarkGame(Graph graph, int[] copPositions, int[] robberPositions, int turnLimit = 100, int copSpeed = 1, int robberSpeed = 1, CopStrategy copStrategy = CopStrategy.STMTAStar)
    {
        this.graph = graph;
        this.turnLimit = turnLimit;
        this.copSpeed = copSpeed;
        this.robberSpeed = robberSpeed;
        result.copStartPositions = copPositions;
        result.robberStartPositions = robberPositions;
        Game = new(graph, copPositions.Length, robberPositions.Length);
        Game.strategies[Game.Cops] = copStrategy switch
        {
            CopStrategy.STMTAStar => new PrecalculatedAStarWithAssignedTargets(Game),
            CopStrategy.TRAP_Max => new AssignedTargetCoverGradientDescent(Game, new CoverMinimizeAssignment(CoverMinimizeAssignment.Metric.Max, Game)),
            CopStrategy.TRAP_Sum => new AssignedTargetCoverGradientDescent(Game, new CoverMinimizeAssignment(CoverMinimizeAssignment.Metric.Sum, Game)),
            _ => null
        };
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
        result.caughtAmount = Game.Robbers.Agents.Sum(agent => (agent as Robber).Caught ? 1 : 0);
        Game.ResetAgents();
    }

    public struct Result
    {
        public int turns;
        public int caughtAmount;
        public int[] copStartPositions;
        public int[] robberStartPositions;
        public override readonly string ToString()
        {
            return $"{turns}, {caughtAmount}, C:[{string.Join(",", copStartPositions)}], R:[{string.Join(",", robberStartPositions)}]";
        }
    }
}
