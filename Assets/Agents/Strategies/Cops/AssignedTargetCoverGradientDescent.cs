using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AssignedTargetCoverGradientDescent : ITeamStrategy
{
    private readonly Dictionary<Agent, Agent> TargetAssignment = new();
    private readonly ITargetAssignmentStrategy targetAssignmentStrategy;
    private readonly CopsNRobberGame game;

    public AssignedTargetCoverGradientDescent(CopsNRobberGame game, ITargetAssignmentStrategy targetAssignmentStrategy)
    {
        this.game = game;
        this.targetAssignmentStrategy = targetAssignmentStrategy;
    }

    public void Init()
    {
        ReassignTargets();
    }

    public void Tick()
    {
        if (game.Finished) return; // no ticking necessary
        if (TargetAssignment.Any(pair => (pair.Value as Robber).Caught)) ReassignTargets(); //TODO: think about when should I reassign targets? sometimes a robber is cornered and the cop just leaves due to new assignment. what is the assignment criterion?
        foreach (var cop in game.Cops.Agents)
        {
            var targetNode = TargetAssignment[cop].OccupiedNode;
            var copNodes = game.Cops.Agents.Where(c => c != cop).Select(c => c.OccupiedNode).ToList();

            var bestMove = cop.OccupiedNode;
            copNodes.Insert(0, cop.OccupiedNode);
            var score = game.graph.CalculateTargetCover(targetNode, copNodes).Count; //baseline - don't move to an inferior node            

            foreach (var moveOption in cop.OccupiedNode.Neighbours)
            {
                copNodes[0] = moveOption;
                var newScore = game.graph.CalculateTargetCover(targetNode, copNodes).Count; //calc cover of available moves
                //TODO: something is off here... pursuers get stuck in two repetitive moves even when I dont move at all...
                // mayhaps the cover score is wrong?
                if (newScore < score) //choose best cover
                {
                    bestMove = moveOption;
                    score = newScore;
                }
                else if (newScore == score) //tiebreaker = AStar path length to target
                {
                    var oldDistance = game.graph.FromTo(bestMove, targetNode).Count;
                    var newDistance = game.graph.FromTo(moveOption, targetNode).Count;
                    if (oldDistance > newDistance) //only move when new node is better than old node
                    {
                        bestMove = moveOption;
                        score = newScore;
                    }
                }
            }
            cop.Move(bestMove);
        }
    }

    private void ReassignTargets()
    {
        var assignment = targetAssignmentStrategy.AssignAll(game);
        if (assignment == null) throw new System.Exception("No assignment found");
        foreach (var pair in assignment) TargetAssignment[pair.Key] = pair.Value;
    }
}