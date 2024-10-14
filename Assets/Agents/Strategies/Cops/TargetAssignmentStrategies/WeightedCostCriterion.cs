
using System;
using System.Collections.Generic;
using System.Linq;

public class WeightedCostCriterion : ITargetAssignmentStrategy
{
    public Dictionary<Cop, Robber> AssignAll(CopsNRobberGame game)
    {
        var options = ((ITargetAssignmentStrategy)this).AssignmentOptions(game);
        if (options.Length == 0) throw new Exception("no assignment options!");
        var AssignmentScores = new Dictionary<Dictionary<Cop, Robber>, int>();
        foreach (var assignment in options)
        {
            var distanceSum = 0;
            var maxDistance = 0;
            foreach (var pair in assignment)
            {
                var distance = game.graph.Distance(pair.Key.OccupiedNode, pair.Value.OccupiedNode);
                distanceSum += distance;
                maxDistance = Math.Max(distance, maxDistance);
            }
            AssignmentScores[assignment] = distanceSum + maxDistance;
        };
        var bestAssignment = AssignmentScores.OrderBy(assignment => assignment.Value).First();
        return bestAssignment.Key;
    }
}
