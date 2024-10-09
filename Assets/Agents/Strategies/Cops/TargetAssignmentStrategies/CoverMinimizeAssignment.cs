using System;
using System.Collections.Generic;
using System.Linq;

public class CoverMinimizeAssignment : ITargetAssignmentStrategy
{
    public enum Metric
    {
        Sum,
        Max,
        Max_Tiebreak_Sum
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
                var robberCover = game.graph.CalculateTargetCoverSize(robber.OccupiedNode.index, game.Cops.Agents.Where(cop => assignment[(Cop)cop] == robber).Select(a => a.OccupiedNode.index).ToArray());
                score = metric switch
                {
                    Metric.Sum => score + robberCover,
                    Metric.Max => Math.Max(score, robberCover),
                    Metric.Max_Tiebreak_Sum => Math.Max(score / game.graph.Nodes.Length, robberCover) * game.graph.Nodes.Length + robberCover + score % game.graph.Nodes.Length,
                    _ => score
                };
            }
            AssignmentScores[assignment] = score;
        };
        var bestAssignment = AssignmentScores.OrderBy(assignment => assignment.Value).First();
        return bestAssignment.Key;
    }    
}