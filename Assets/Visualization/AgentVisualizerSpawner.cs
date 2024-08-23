using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AgentVisualizerSpawner : MonoBehaviour
{
    Cached<UnityGame> cached_UnityGame = new(Cached<UnityGame>.GetOption.Parent);
    UnityGame UnityGame => cached_UnityGame[this];
    [field: SerializeField] private AgentVisualizer AgentTemplate { get; set; }

    void Awake()
    {
        UnityGame.GameStart += OnGameStart;
        UnityGame.GameStop += OnGameStop;
    }
    void OnDestroy()
    {
        UnityGame.GameStart -= OnGameStart;
        UnityGame.GameStop -= OnGameStop;
    }
    private void OnGameStop()
    {
        for (int i = 0; i < transform.childCount; i++)
            Destroy(transform.GetChild(i).gameObject);
    }

    private void OnGameStart()
    {
        foreach (var agent in UnityGame.Game.teams.SelectMany(team => team.agents))
        {
            var agentVisualizer = Instantiate(AgentTemplate, transform);
            agentVisualizer.Agent = agent;
            
        }
    }


}
