using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnityGame : MonoBehaviour
{
    public TextAsset MapAsset;
    public CopsNRobberGame Game { get; private set; }
    public event Action GameStart;
    public event Action GameTick;
    public event Action GameStop;

    public int copSpeed = 1;
    public int robberSpeed = 1;
    public int[] CopNodes;
    public int[] RobberNodes;
    public bool randomizePositions;

    [SerializeField] BenchmarkGame.CopStrategy copStrategy;

    void Start()
    {
        var graph = Graph.FromMapFile(MapAsset);
        var spawnpoints = new Dictionary<Team, int[]>();
        Game = new CopsNRobberGame(graph, CopNodes.Length, RobberNodes.Length, randomizePositions ? new RandomSpawnpointProvider() : new ArraySpawnpointProvider(spawnpoints));
        spawnpoints[Game.Cops] = CopNodes;
        spawnpoints[Game.Robbers] = RobberNodes;
        Game.teamSpeed[Game.Cops] = copSpeed;
        Game.teamSpeed[Game.Robbers] = robberSpeed;
        Game.CopStrategy = copStrategy switch
        {
            BenchmarkGame.CopStrategy.STMTAStar => new PrecalculatedAStarWithAssignedTargets(Game),
            BenchmarkGame.CopStrategy.TRAP_Max => new AssignedTargetCoverGradientDescent(Game, new CoverMinimizeAssignment(CoverMinimizeAssignment.Metric.Max, Game)),
            BenchmarkGame.CopStrategy.TRAP_Sum => new AssignedTargetCoverGradientDescent(Game, new CoverMinimizeAssignment(CoverMinimizeAssignment.Metric.Sum, Game)),
            BenchmarkGame.CopStrategy.TRAP_Max_Tiebreak_Sum => new AssignedTargetCoverGradientDescent(Game, new CoverMinimizeAssignment(CoverMinimizeAssignment.Metric.Max_Tiebreak_Sum, Game)),
            BenchmarkGame.CopStrategy.TRAP_OMNI_MAX => new CoverGradientDescent(Game, CoverGradientDescent.Metric.Max),
            BenchmarkGame.CopStrategy.TRAP_OMNI_MIN => new CoverGradientDescent(Game, CoverGradientDescent.Metric.Min),
            BenchmarkGame.CopStrategy.TRAP_OMNI_SUM => new CoverGradientDescent(Game, CoverGradientDescent.Metric.Sum),
            BenchmarkGame.CopStrategy.TRAP_OMNI_MIN_TIEBREAK_SUM => new CoverGradientDescent(Game, CoverGradientDescent.Metric.Min_Tiebreak_Sum),
            BenchmarkGame.CopStrategy.TRAP_OMNI_MIN_TIEBREAK_MAX => new CoverGradientDescent(Game, CoverGradientDescent.Metric.Min_Tiebreak_Max),
            _ => null
        };
        Game.RobberStrategy = new MultiagentTrailmax(Game);
        StartCoroutine(GameRoutine());
    }
    [SerializeField] bool autoPlay = true;
    public IEnumerator GameRoutine()
    {
        while (true)
        {
            Game.InitAgents();
            Game.InitStrategies();
            GameStart?.Invoke();
            while (Game.Robbers.Agents.Any(agent => !(agent as Robber).Caught))
            {
                Game.TickStrategies();
                yield return new WaitUntil(() => !ManualModeInputHandler.HasPendingSelection);
                GameTick?.Invoke();
                if(autoPlay) yield return new WaitForSeconds(.1f);
                else yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
            }
            GameStop?.Invoke();
            yield return new WaitForSeconds(1f);
        }
    }

    public void OnDrawGizmosSelected()
    {
        if (Game?.graph?.Nodes == null) return;
        foreach (var node in Game.graph.Nodes)
            Gizmos.DrawCube((Vector2)node.position, Vector3.one * .5f);
    }
}