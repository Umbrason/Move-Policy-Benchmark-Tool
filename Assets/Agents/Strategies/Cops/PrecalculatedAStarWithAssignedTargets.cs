using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class PrecalculatedAStarWithAssignedTargets : ITeamStrategy
{
    private readonly Dictionary<Agent, Agent> TargetAssignment = new();
    private readonly CopsNRobberGame game;
    private readonly Team team;

    public PrecalculatedAStarWithAssignedTargets(CopsNRobberGame game)
    {
        this.game = game;
        this.team = game.teams[1];
    }

    public void Init()
    {
        game.graph.PrecalcAStarPaths();
    }

    public void Tick()
    {
        foreach (var agent in team.Agents)
        {
            var target = (Agent)null;
            AssignTargetIfMissing(agent, ref target);
            if (target == null) break; //all robbers caught
            var path = game.graph.FromTo(agent.OccupiedNode, target.OccupiedNode);
            agent.Move(path[0]);
        }
    }

    //TODO: use ITargetAssignmentStrategy
    private void AssignTargetIfMissing(Agent agent, ref Agent assignedTarget)
    {
        if (!TargetAssignment.TryGetValue(agent, out assignedTarget) || ((TargetAssignment[agent] as Robber)?.Caught ?? true)) TargetAssignment[agent] = assignedTarget = GetClosestAgent(agent);
    }
    private Agent GetClosestAgent(Agent agent)
    {
        var byDistance = game.teams.Where(t => t != team).SelectMany(team => team.Agents).Where(agent => !(agent as Robber).Caught).OrderBy(other => (other.OccupiedNode.position - agent.OccupiedNode.position).sqrMagnitude);
        return byDistance.FirstOrDefault();
    }
}
