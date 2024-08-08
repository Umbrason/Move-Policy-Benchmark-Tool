using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ManualGame : MonoBehaviour
{
    public TextAsset MapAsset;
    public CopsNRobberGame Game { get; private set; }
    public event Action GameStart;
    public event Action GameTick;
    public event Action GameStop;

    void Start()
    {
        var graph = Graph.FromMapFile(MapAsset);

        var strategies = new Dictionary<Team, ITeamStrategy>();
        Game = new CopsNRobberGame(graph, 1, 3, strategies);
        strategies[Game.Cops] = new CoverGradientDescent(Game, new CoverMinimizeAssignment(CoverMinimizeAssignment.Metric.Max, Game));
        StartCoroutine(GameRoutine());
    }

    private bool playerMoved;
    public IEnumerator GameRoutine()
    {
        while (true)
        {
            Game.InitAgents();
            Game.InitStrategies();
            GameStart?.Invoke();
            while (Game.Robbers.agents.Any(agent => !(agent as Robber).Caught))
            {
                yield return new WaitUntil(() => playerMoved);
                playerMoved = false;
                Game.TickStrategies();
                GameTick?.Invoke();
            }
            GameStop?.Invoke();
            yield return new WaitForSeconds(1f);
        }
    }

    public void MovePlayer(Node node)
    {
        Game.Robbers.agents[0].Move(node);
        playerMoved = true;
    }

    public void OnDrawGizmosSelected()
    {
        if (Game?.graph?.Nodes == null) return;
        foreach (var node in Game.graph.Nodes)
            Gizmos.DrawCube((Vector2)node.position, Vector3.one * .5f);
    }
}