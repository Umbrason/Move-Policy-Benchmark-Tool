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
            var targetNode = TargetAssignment[cop].OccupiedNode.index;
            var copNodes = game.Cops.Agents.Where(c => c != cop).Select(c => c.OccupiedNode.index).Prepend(cop.OccupiedNode.index).ToArray();

            var bestMove = cop.OccupiedNode;
            var score = game.graph.CalculateTargetCoverSize(targetNode, copNodes); //baseline - don't move to an inferior node            

            //build move options for nodes in range x
            var speed = game.teamSpeed[game.Cops];
            HashSet<int> MoveOptions = new((speed + 1) * (speed + 1)) { cop.OccupiedNode.index };
            for (int i = 0; i < speed; i++)
            {
                foreach (var moveOption in MoveOptions.ToArray())
                    for (int n = 0; n < game.graph.Nodes[moveOption].neighbourCount; n++)
                    {
                        Node neighbour = game.graph.Nodes[moveOption].Neighbours[n];
                        MoveOptions.Add(neighbour.index);
                    }
            }
            MoveOptions.Remove(cop.OccupiedNode.index);

            foreach (var moveOption in MoveOptions)
            {
                copNodes[0] = moveOption;
                var newScore = game.graph.CalculateTargetCoverSize(targetNode, copNodes); //calc cover of available moves
                //TODO: something is off here... pursuers get stuck in two repetitive moves even when I dont move at all...
                // mayhaps the cover score is wrong?
                if (newScore < score) //choose best cover
                {
                    bestMove = game.graph.Nodes[moveOption];
                    score = newScore;
                }
                else if (newScore == score) //tiebreaker = AStar path length to target
                {
                    var oldDistance = game.graph.Distance(bestMove.index, targetNode);
                    var newDistance = game.graph.Distance(moveOption, targetNode);
                    if (oldDistance > newDistance) //only move when new node is better than old node
                    {
                        bestMove = game.graph.Nodes[moveOption];
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
