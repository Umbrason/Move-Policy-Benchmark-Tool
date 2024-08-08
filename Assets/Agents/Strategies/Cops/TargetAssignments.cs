using System.Collections.Generic;

public struct TargetAssignment
{
    private readonly ITargetAssignmentStrategy targetAssignmentStrategy;
    private Dictionary<Cop, Robber> targetAssignment;
    public TargetAssignment(ITargetAssignmentStrategy targetAssignmentStrategy, CopsNRobberGame game)
    {
        this.targetAssignmentStrategy = targetAssignmentStrategy;
        targetAssignment = targetAssignmentStrategy.AssignAll(game);
    }
}