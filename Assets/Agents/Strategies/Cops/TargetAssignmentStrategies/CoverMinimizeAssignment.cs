using System;
using System.Collections.Generic;
using System.Linq;

public class CoverMinimizeAssignment : ITargetAssignmentStrategy
{
    public enum Metric
    {
        Sum,
        Max
    }
    private readonly Metric metric;
    private readonly CopsNRobberGame game;

    public CoverMinimizeAssignment(Metric metric, CopsNRobberGame game)
    {
        this.metric = metric;
        this.game = game;
    }

    public Dictionary<Cop, Robber> AssignAll(CopsNRobberGame game)
    {
        var options = ((ITargetAssignmentStrategy)this).AssignmentOptions(game);
        if (options.Length == 0) throw new Exception("no assignment options!");
        var AssignmentScores = new Dictionary<Dictionary<Cop, Robber>, int>();
        foreach (var assignment in options)
        {
            var score = 0;
            foreach (var robber in game.Robbers.Agents)
            {
                var robberCover = RobberCover((Robber)robber, game.Cops.Agents.Select(a => (Cop)a).Where(cop => assignment[cop] == robber).ToList());
                score = metric switch
                {
                    Metric.Sum => score + robberCover.Count,
                    Metric.Max => Math.Max(score, robberCover.Count),
                    _ => score
                };
            }
            AssignmentScores[assignment] = score;
        };
        var bestAssignment = AssignmentScores.OrderBy(assignment => assignment.Value).First();
        return bestAssignment.Key;
    }

    //TODO: dont assume full connectivity of the graph
    public HashSet<Node> RobberCover(Robber robber, List<Cop> cops)
    {
        HashSet<Node> robberCover = new();
        if (cops.Count == 0) return new(game.graph.Nodes);
        foreach (var node in game.graph.Nodes)
        {
            var robberPath = game.graph.FromTo(robber.OccupiedNode, node);
            var bestCopPath = cops.Select(cop => game.graph.FromTo(cop.OccupiedNode, node)).OrderBy(path => path.Count).First();
            if (robberPath.Count < bestCopPath.Count) robberCover.Add(node);
        }
        return robberCover;
    }
}