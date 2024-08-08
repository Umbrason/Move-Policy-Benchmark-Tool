/*
Implementation of the 'Multiple Pursuers TrailMax Algorithm' from 'A Strategy‐Based Algorithm for Moving Targets in an Environment with Multiple Agents'
funcostRobberion MultiplePursuersTraiMax()
    initialise position for all players (pursuers and target)
    initial cumulative cost c ← 0 for each player
    add target to target_node_queue
    add pursuers to pursuer_node_queue
    target_caught_states ← 0

    if target is not captured then
        while target_node_queue not empty do
            costRobber ← get c from target_node_queue
            costPursuer ← get c from pursuer_node_queue
            if (costRobber ≤ costPursuer) then
                remove target from target_node_queue
                if target not in target_closed and pursuer_closed and parent node not in pursuer_closed then
                    insert target into target_closed
                    append target neighbours onto target_node_queue
            else
                for each pᵢ of players do
                get state sᵢ for pᵢ
                if sᵢ is pursuer then
                    costPursuer ← get c on pursuer_node_queue
                    remove pᵢ from pursuer_node_queue
                    if pᵢ not already in pursuer_closed then
                        insert pᵢ into pursuer_closed
                        if pᵢ in target_closed then
                            increment target_caught_states
                            if target_caught_states is equal to size of target_closed then
                                return true
                        append pᵢ neighbours onto pursuer_node_queue
    generate target_path
        if target_closed not empty then
            reverse target_closed
    return target_path
    */
using System.Collections.Generic;
using System.Linq;

public class MultiagentTrailmax : ITeamStrategy
{
    private List<Agent> robbers;
    private List<Agent> cops;

    public MultiagentTrailmax(CopsNRobberGame game)
    {
        this.robbers = game.teams[0].agents;
        this.cops = game.teams[1].agents;
    }

    public void Init() { }

    //There must be a mistake in this implementation somewhere. paths are often just an agent running back and forth. no real avoidance behaviour exhibited
    //robber seems to 'gives up' at times
    private List<Node> GetRobberPath(Robber robber)
    {
        if (robber.Caught) return null;        
        var robberClosed = new HashSet<Node>(); //TODO: <- in BA explicit erklären
        var copsClosed = new HashSet<Node>();
        var predecessors = new Dictionary<Node, Node>() { { robber.OccupiedNode, robber.OccupiedNode } };
        
        var robberNodeQueue = new SortedList<float, Node>(new DuplicateKeyComparer<float>());
        robberNodeQueue.Add(0, robber.OccupiedNode);

        var copNodeQueue = new SortedList<float, Node>(new DuplicateKeyComparer<float>());
        foreach (var cop in cops) copNodeQueue.Add(0, cop.OccupiedNode);
        
        var lastCopClosed = robber.OccupiedNode; //TODO: <- in BA explicit erklären
        
        var robberCaughtStates = 0;
        while (robberNodeQueue.Count > 0)
        {
            var minCostRobber = robberNodeQueue.Count > 0 ? robberNodeQueue.Keys[0] : float.MaxValue;
            var minCostCops = copNodeQueue.Count > 0 ? copNodeQueue.Keys[0] : float.MaxValue;
            if (minCostRobber <= minCostCops)
            {
                var node = robberNodeQueue.Values[0];
                robberNodeQueue.RemoveAt(0);
                
                if (robberClosed.Contains(node) || copsClosed.Contains(node) || copsClosed.Contains(predecessors[node])) continue;
                robberClosed.Add(node);
                foreach (var neighbour in node.Neighbours.Reverse<Node>())
                {
                    if (robberClosed.Contains(neighbour)) continue;
                    if (!predecessors.ContainsKey(neighbour)) predecessors[neighbour] = node;
                    robberNodeQueue.Add(minCostRobber + node.Distance(neighbour), neighbour); // Update instead of add could improve performance
                }
            }
            else
            {
                var node = copNodeQueue.Values[0];
                copNodeQueue.RemoveAt(0);
                if (copsClosed.Contains(node)) continue;
                copsClosed.Add(node);
                if (robberClosed.Contains(node))
                {
                    lastCopClosed = node;
                    robberCaughtStates++;
                    if (robberCaughtStates == robberClosed.Count) break;
                }
                foreach (var neighbour in node.Neighbours)
                    copNodeQueue.Add(minCostCops + node.Distance(neighbour), neighbour);
            }
        }
        var reconstructNode = lastCopClosed;  //TODO: <- in BA explicit erklären
        var path = new List<Node>();
        do
        {
            path.Insert(0, reconstructNode);
            reconstructNode = predecessors[reconstructNode];
        } while (predecessors[reconstructNode] != reconstructNode);
        return path;
    }

    public void Tick()
    {
        foreach (var robber in robbers)
        {
            if ((robber as Robber).Caught) continue;
            var path = GetRobberPath(robber as Robber);
            if (path?.Count < 1) continue;
            robber.Move(path[0]);
        }
    }
}