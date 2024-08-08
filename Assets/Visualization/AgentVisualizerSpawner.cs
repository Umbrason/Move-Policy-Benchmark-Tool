using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AgentVisualizerSpawner : MonoBehaviour
{
    Cached<ManualGame> cached_ManualGame = new(Cached<ManualGame>.GetOption.Parent);
    ManualGame ManualGame => cached_ManualGame[this];
    [field: SerializeField] private AgentVisualizer AgentTemplate { get; set; }

    void Awake()
    {
        ManualGame.GameStart += OnGameStart;
        ManualGame.GameStop += OnGameStop;
    }
    void OnDestroy()
    {
        ManualGame.GameStart -= OnGameStart;
        ManualGame.GameStop -= OnGameStop;
    }
    private void OnGameStop()
    {
        for (int i = 0; i < transform.childCount; i++)
            Destroy(transform.GetChild(i).gameObject);
    }

    private void OnGameStart()
    {
        foreach (var agent in ManualGame.Game.teams.SelectMany(team => team.agents))
        {
            var agentVisualizer = Instantiate(AgentTemplate, transform);
            agentVisualizer.Agent = agent;
            
        }
    }


}
