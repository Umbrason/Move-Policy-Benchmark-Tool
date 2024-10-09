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

public class MultiagentTrailmax : ITeamStrategy
{
    private List<Agent> robbers => game.Robbers.Agents;
    private List<Agent> cops => game.Cops.Agents;
    private Graph graph => game.graph;
    private readonly CopsNRobberGame game;
    int robberSpeed => game.teamSpeed[game.Robbers];
    int copSpeed => game.teamSpeed[game.Cops];

    private readonly FastNodeQueue robberQueue;
    private readonly FastNodeQueue copQueue;

    public MultiagentTrailmax(CopsNRobberGame game)
    {
        this.game = game;
        robberQueue = new(game.graph.Nodes.Length);
        copQueue = new(game.graph.Nodes.Length);
    }

    public void Init() { }

    //TODO: behaviour looks fine, except for the easy situations, where robber doesnt fully commit into dead ends to maximize capture time
    private int[] GetRobberPath(Robber robber, Graph graph)
    {
        if (robber.Caught) return null;
        var graphSize = graph.Nodes.Length;
        var robberClosed = new bool[graphSize];
        var robberClosedCount = 0;
        var copsClosed = new bool[graphSize];
        var predecessors = new int[graphSize];
        for (int i = 0; i < predecessors.Length; i++) predecessors[i] = -1;
        predecessors[robber.OccupiedNode.index] = robber.OccupiedNode.index;

        robberQueue.Reset();
        copQueue.Reset();

        robberQueue.Enqueue(0, robber.OccupiedNode.index);
        foreach (var cop in cops) copQueue.Enqueue(0, cop.OccupiedNode.index);

        var cached_RobberSpeed = robberSpeed;
        var cached_CopSpeed = copSpeed;
        var lastCopClosed = robber.OccupiedNode;
        var robberCaughtStates = 0;
        while (robberQueue.NextIndex >= 0)
        {
            var minCostRobber = robberQueue.NextCost;
            var minCostCops = copQueue.NextCost;
            if (minCostRobber < minCostCops)
            {
                robberQueue.Dequeue(out var cost, out var index);
                if (robberClosed[index] || copsClosed[index] || copsClosed[predecessors[index]]) continue;
                var node = graph.Nodes[index];
                robberClosed[index] = true;
                robberClosedCount++;
                for (int i = node.neighbourCount - 1; i >= 0; i--)
                {
                    Node neighbour = node.Neighbours[i];
                    if (robberClosed[neighbour.index]) continue;
                    if (predecessors[neighbour.index] == -1) predecessors[neighbour.index] = index;
                    robberQueue.Enqueue(minCostRobber + cached_CopSpeed, neighbour.index);
                }
            }
            else
            {
                copQueue.Dequeue(out var cost, out var index);
                if (copsClosed[index]) continue;
                var node = graph.Nodes[index];
                copsClosed[index] = true;
                if (robberClosed[index])
                {
                    lastCopClosed = node;
                    robberCaughtStates++;
                    if (robberCaughtStates == robberClosedCount) break;
                }
                for (int i = 0; i < node.neighbourCount; i++)
                    copQueue.Enqueue(minCostCops + cached_RobberSpeed, node.Neighbours[i].index);
            }
        }
        return ReconstructPath(predecessors, lastCopClosed.index);
    }

    private int[] ReconstructPath(int[] predecessors, int to)
    {
        var path = new List<int>();
        while (predecessors[to] != to)
        {
            path.Insert(0, to);
            to = predecessors[to];
        }
        return path.ToArray();
    }

    public void Tick()
    {
        foreach (var robber in robbers)
        {
            if ((robber as Robber).Caught) continue;
            var path = GetRobberPath(robber as Robber, graph);
            for (int i = 0; i < robberSpeed; i++)
            {
                if (path?.Length <= i) continue;
                robber.Move(game.graph.Nodes[path[i]]);
            }
        }
    }
}