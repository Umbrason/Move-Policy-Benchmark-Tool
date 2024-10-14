using System.Collections.Generic;
using System.Linq;

public class PrecalculatedAStarWithAssignedTargets : ITeamStrategy
{
    private Dictionary<Cop, Robber> targetAssignment = new();
    private readonly CopsNRobberGame game;
    private readonly Team team;
    private readonly ITargetAssignmentStrategy assignmentStrategy = new WeightedCostCriterion();

    public PrecalculatedAStarWithAssignedTargets(CopsNRobberGame game)
    {
        this.game = game;
        this.team = game.teams[1];
    }

    public void Init()
    {
        game.graph.PrecalcAStarPaths();
        targetAssignment = assignmentStrategy.AssignAll(game);
    }

    public void Tick()
    {
        for (int i = 0; i < game.teamSpeed[game.Cops]; i++)
            foreach (var agent in team.Agents)
            {
                var target = (Robber)null;
                AssignTargetIfMissing(agent as Cop, ref target);
                if (target == null) break; //all robbers caught
                var path = game.graph.FromTo(agent.OccupiedNode, target.OccupiedNode);
                if (path.Length > 0) agent.Move(game.graph.Nodes[path[0]]);
            }
    }

    private void AssignTargetIfMissing(Cop agent, ref Robber assignedTarget)
    {
        if (!targetAssignment.TryGetValue(agent, out assignedTarget) || ((targetAssignment[agent] as Robber)?.Caught ?? true)) targetAssignment[agent] = assignedTarget = GetClosestAgent(agent);
    }
    private Robber GetClosestAgent(Cop cop)
    {
        var byDistance = game.teams.Where(t => t != team).SelectMany(team => team.Agents).Select(agent => agent as Robber).Where(agent => !agent.Caught).OrderBy(other => game.graph.Distance(other.OccupiedNode, cop.OccupiedNode));
        return byDistance.FirstOrDefault();
    }
}
