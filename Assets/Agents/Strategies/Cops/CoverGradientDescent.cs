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
        Min_Tiebreak_Max,
        Min_Tiebreak_Sum,
    }

    public CoverGradientDescent(CopsNRobberGame game, Metric metric = Metric.Min)
    {
        this.game = game;
        this.metric = metric;
    }

    public void Init() { }

    public void Tick()
    {
        if (game.Finished) return; // no ticking necessary
        UnityEngine.Profiling.Profiler.BeginSample("Init");
        var targetNodes = game.Robbers.Agents.Where(r => !(r as Robber).Caught).Select(r => r.OccupiedNode.index).ToArray();
        if (targetNodes.Length == 0) return; //no agents left ot move towards
        var copUpdateOrder = game.Cops.Agents.OrderBy(cop => targetNodes.Select(target => game.graph.Distance(target, cop.OccupiedNode.index)).Min());
        UnityEngine.Profiling.Profiler.EndSample();
        
        
        foreach (var cop in copUpdateOrder)
        {
            UnityEngine.Profiling.Profiler.BeginSample("Compute Cop");
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
                    var newDistance = TiebreakScore(copNodes, targetNodes);
                    copNodes[0] = bestMove.index;
                    var oldDistance = TiebreakScore(copNodes, targetNodes);
                    if (oldDistance > newDistance)
                    {
                        bestMove = moveOption;
                        score = newScore;
                    }
                }
            }
            cop.Move(bestMove);
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }

    //Theory: 
    //best strategy is
    // minimize smallest cover (leads to fastest capture)
    // tiebreak -> minimize cover sum / max cover (helps prepare more captures)
    // supertiebreaker -> distance to closest target / distance to target that is furthest from any cop (helps move towards relevant locations for captures)
    private int TiebreakScore(int[] pursuerNodes, int[] targetNodes)
    {
        UnityEngine.Profiling.Profiler.BeginSample("TiebreakScore");
        var relevantTarget = targetNodes[0];
        var relevantTargetCover = game.graph.CalculateTargetCoverSize(targetNodes[0], pursuerNodes);
        for (int i = 1; i < targetNodes.Length; i++)
        {
            var cover = game.graph.CalculateTargetCoverSize(targetNodes[i], pursuerNodes);
            if (relevantTargetCover < cover) continue;
            relevantTarget = targetNodes[i];
            relevantTargetCover = cover;
        }
        var minDistance = game.graph.Distance(relevantTarget, pursuerNodes[0]);
        UnityEngine.Profiling.Profiler.EndSample();
        return minDistance;
    }

    private int MoveScore(int[] copNodes, int[] targetNodes)
    {
        UnityEngine.Profiling.Profiler.BeginSample("MoveScore");
        var score = game.graph.CalculateTargetCoverSize(targetNodes[0], copNodes);
        if (metric == Metric.Min_Tiebreak_Max || metric == Metric.Min_Tiebreak_Sum) score = score * game.graph.Nodes.Length + score;
        for (int i = 1; i < targetNodes.Length; i++)
        {
            int target = targetNodes[i];
            UnityEngine.Profiling.Profiler.BeginSample("calc cover size");
            var cover = game.graph.CalculateTargetCoverSize(target, copNodes);
            UnityEngine.Profiling.Profiler.EndSample();
            var maxCoverSum = game.graph.Nodes.Length * targetNodes.Length;
            UnityEngine.Profiling.Profiler.BeginSample("switch");
            score = metric switch
            {
                Metric.Max => score > cover ? score : cover,
                Metric.Min => score < cover ? score : cover,
                Metric.Sum => score + cover,
                Metric.Min_Tiebreak_Max => Math.Min(score / maxCoverSum, cover) * maxCoverSum + Math.Max(cover, score % maxCoverSum),
                Metric.Min_Tiebreak_Sum => Math.Min(score / maxCoverSum, cover) * maxCoverSum + score % maxCoverSum + cover,
                _ => throw new System.NotImplementedException()
            };
            UnityEngine.Profiling.Profiler.EndSample();
        }
        UnityEngine.Profiling.Profiler.EndSample();
        return score;
    }
}
