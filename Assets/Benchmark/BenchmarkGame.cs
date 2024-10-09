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
        TRAP_Max_Tiebreak_Sum,
        TRAP_OMNI_MIN,
        TRAP_OMNI_MAX,
        TRAP_OMNI_SUM,
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
            CopStrategy.TRAP_Max_Tiebreak_Sum => new AssignedTargetCoverGradientDescent(Game, new CoverMinimizeAssignment(CoverMinimizeAssignment.Metric.Max_Tiebreak_Sum, Game)),
            CopStrategy.TRAP_OMNI_MAX => new CoverGradientDescent(Game, CoverGradientDescent.Metric.Max),
            CopStrategy.TRAP_OMNI_MIN => new CoverGradientDescent(Game, CoverGradientDescent.Metric.Min),
            CopStrategy.TRAP_OMNI_SUM => new CoverGradientDescent(Game, CoverGradientDescent.Metric.Sum),            
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

        public Result(int turns, int caughtAmount, int[] copStartPositions, int[] robberStartPositions)
        {
            this.turns = turns;
            this.caughtAmount = caughtAmount;
            this.copStartPositions = copStartPositions;
            this.robberStartPositions = robberStartPositions;
        }

        public override readonly string ToString()
        {
            return $"{turns}, {caughtAmount}, C:[{string.Join(";", copStartPositions)}], R:[{string.Join(";", robberStartPositions)}]";
        }
        public static Result Parse(string data)
        {
            var cells = data.Split(",").Select(str => str.Trim().Replace(" ", "")).ToArray();
            if (cells.Length != 4) throw new FormatException($"Could not parse \"{data}\"");
            var turnsCell = cells[0];
            var caughtCell = cells[1];
            var copPositionsCell = cells[2];
            var robberPositionsCell = cells[3];

            var turns = int.Parse(turnsCell);
            var caught = int.Parse(caughtCell);
            var copPositions = copPositionsCell[3..^1].Split(';').Select(int.Parse).ToArray();
            var robberPositions = robberPositionsCell[3..^1].Split(';').Select(int.Parse).ToArray();
            return new(turns, caught, copPositions, robberPositions);
        }
    }
}
