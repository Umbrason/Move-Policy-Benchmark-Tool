using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CoverGradientDescent : ITeamStrategy
{
    private readonly CopsNRobberGame game;
    private readonly Metric metric;
    public enum Metric
    {
        Sum,
        Max,
        Min,
    }

    private int cachedPursuerSpeed;
    private int cachedTargetSpeed;

    public CoverGradientDescent(CopsNRobberGame game, Metric metric = Metric.Min)
    {
        this.game = game;
        this.metric = metric;
        cachedTargetSpeed = game.teamSpeed[game.Robbers];
        cachedPursuerSpeed = game.teamSpeed[game.Cops];
    }

    public void Init() { }

    public void Tick()
    {
        if (game.Finished) return; // no ticking necessary        
        var targetNodes = game.Robbers.Agents.Where(r => !(r as Robber).Caught).Select(r => r.OccupiedNode.index).ToArray();
        if (targetNodes.Length == 0) return; //no agents left ot move towards
        var copUpdateOrder = game.Cops.Agents.OrderBy(cop => targetNodes.Select(target => game.graph.Distance(target, cop.OccupiedNode.index)).Min());

        foreach (var cop in copUpdateOrder)
        {
            var copNodes = game.Cops.Agents.Where(c => c != cop).Select(c => c.OccupiedNode.index).Prepend(cop.OccupiedNode.index).ToArray();
            var bestMove = cop.OccupiedNode;
            var score = MoveScore(copNodes, targetNodes);//baseline - don't move to an inferior node

            for (int i = 0; i < cop.OccupiedNode.neighbourCount; i++)
            {
                var moveOption = cop.OccupiedNode.Neighbours[i];
                copNodes[0] = moveOption.index;
                var newScore = MoveScore(copNodes, targetNodes);
                if (newScore < score) //choose best cover
                {
                    bestMove = moveOption;
                    score = newScore;
                }
                else if (newScore == score)
                {
                    var newTiebreaker = TiebreakScore(copNodes, targetNodes);
                    copNodes[0] = bestMove.index;
                    var oldTiebreaker = TiebreakScore(copNodes, targetNodes);
                    if (oldTiebreaker > newTiebreaker)
                    {
                        bestMove = moveOption;
                        score = newScore;
                    }
                }
            }
            cop.Move(bestMove);
        }
    }
    
    private int TiebreakScore(int[] pursuerNodes, int[] targetNodes)
    {
        var relevantTarget = targetNodes[0];
        var sizes = game.graph.CalculateTargetCoverSizes(targetNodes, pursuerNodes, cachedTargetSpeed, cachedPursuerSpeed);
        var relevantTargetCover = sizes[0];
        for (int i = 1; i < targetNodes.Length; i++)
        {
            var cover = sizes[i];
            if (relevantTargetCover < cover) continue;
            relevantTarget = targetNodes[i];
            relevantTargetCover = cover;
        }
        var minDistance = game.graph.Distance(relevantTarget, pursuerNodes[0]);        
        return minDistance;
    }

    private int MoveScore(int[] pursuerNodes, int[] targetNodes)
    {
        var sizes = game.graph.CalculateTargetCoverSizes(targetNodes, pursuerNodes, cachedTargetSpeed, cachedPursuerSpeed);
        var score = sizes[0];
        for (int targetID = 1; targetID < targetNodes.Length; targetID++)
        {
            var cover = sizes[targetID];
            score = metric switch
            {
                Metric.Max => score > cover ? score : cover,
                Metric.Min => score < cover ? score : cover,
                Metric.Sum => score + cover,
                _ => throw new NotImplementedException()
            };
        }     
        return score;
    }
}
