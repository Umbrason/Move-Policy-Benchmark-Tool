using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentVisualizer : MonoBehaviour
{
    private Agent m_agent;
    public Agent Agent { get => m_agent; set => SetAgent(value); }

    private Cached<UnityGame> cached_ManualGame = new(Cached<UnityGame>.GetOption.Parent);
    private UnityGame ManualGame => cached_ManualGame[this];

    private Cached<MeshRenderer> cached_MeshRenderer;
    private MeshRenderer MeshRenderer => cached_MeshRenderer[this];

    void Start()
    {
        ManualGame.GameTick += OnTick;
    }

    void OnDestroy()
    {
        ManualGame.GameTick -= OnTick;
    }
    void OnTick()
    {
        transform.position = ((Vector2)Agent.OccupiedNode.position)._x0y() + Vector3.up * 2f;
        if (Agent is Robber robber && robber.Caught) MeshRenderer.material.color = Color.black;
    }
    private void SetAgent(Agent agent)
    {
        m_agent = agent;
        if (Agent is Robber) MeshRenderer.material.color = Color.red;
        if (Agent is Cop) MeshRenderer.material.color = Color.blue;
        if (agent != null) transform.position = ((Vector2)Agent.OccupiedNode.position)._x0y() + Vector3.up * 2f;
    }
}
