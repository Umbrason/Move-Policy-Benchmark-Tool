using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CoverVisualizer : MonoBehaviour
{
    private Texture2D coverTexture;
    private Cached<MeshRenderer> cached_renderer;
    private MeshRenderer Renderer => cached_renderer[this];

    public Graph m_graph;
    public Graph Graph { get => m_graph; set => SetGraph(value); }

    Cached<UnityGame> cached_UnityGame = new(Cached<UnityGame>.GetOption.Parent);
    private Vector2Int graphNodeMin;
    private Vector2Int graphNodeMax;

    UnityGame UnityGame => cached_UnityGame[this];
    void Awake()
    {
        UnityGame.GameStart += OnGameStart;
        UnityGame.GameTick += UpdateCover;
    }


    void OnDestroy()
    {
        UnityGame.GameStart -= OnGameStart;
        UnityGame.GameTick -= UpdateCover;
    }

    private void UpdateCover()
    {
        var TeamNodeQueues = new Dictionary<Team, SortedList<float, Node>>();
        var TeamExplored = new Dictionary<Team, HashSet<Node>>();

        for (int i = 0; i < UnityGame.Game.teams.Count; i++)
        {
            var team = UnityGame.Game.teams[i];
            Cover[team] = new();
            TeamExplored[team] = new();
            TeamNodeQueues[team] = new(new DuplicateKeyComparer<float>());
            foreach (var agent in team.Agents)
            {
                if (agent is Robber robber && robber.Caught) continue;
                TeamNodeQueues[team].Add(i / (float)UnityGame.Game.teams.Count, agent.OccupiedNode);
            }
        }
        HashSet<Node> AnyCover = new();
        while (true)
        {
            var minTeamQueue = TeamNodeQueues.OrderBy(nodeQueue => nodeQueue.Value.Count > 0 ? nodeQueue.Value.Keys[0] : float.MaxValue).First();
            if (minTeamQueue.Value.Count == 0) break;
            var node = minTeamQueue.Value.Values[0];
            var cost = minTeamQueue.Value.Keys[0];
            minTeamQueue.Value.RemoveAt(0);
            Cover[minTeamQueue.Key].Add(node.position);
            AnyCover.Add(node);
            foreach (var nb in node.Neighbours)
                if (!AnyCover.Contains(nb) && !TeamExplored[minTeamQueue.Key].Contains(nb))
                {
                    minTeamQueue.Value.Add(cost + 1, nb);
                    TeamExplored[minTeamQueue.Key].Add(nb);
                }
        }
        foreach (var team in UnityGame.Game.teams)
            foreach (var nodePosition in Cover[team])
            {
                var local = ToTextureCoordinate(nodePosition);
                coverTexture.SetPixel(local.x, local.y, team.Color * new Color(1f, 1f, 1f, .5f));
            }
        coverTexture.Apply();
    }

    void OnGameStart() => this.Graph = UnityGame.Game.graph;
    private readonly Dictionary<Team, HashSet<Vector2Int>> Cover = new();

    private void SetGraph(Graph graph)
    {
        if (m_graph == graph) return;

        var positions = graph.Nodes.Select(n => n.position).ToArray();
        graphNodeMin = new Vector2Int(positions.Min(p => p.x), positions.Min(p => p.y));
        graphNodeMax = new Vector2Int(positions.Max(p => p.x), positions.Max(p => p.y));
        var w = graphNodeMax.x - graphNodeMin.x + 1;
        var h = graphNodeMax.y - graphNodeMin.y + 1;
        GenerateTexture(graph, graphNodeMin, graphNodeMax);
        Renderer.material.mainTexture = coverTexture;
        transform.localScale = new(w, h, 1);
        transform.position = ((Vector2)(graphNodeMin + graphNodeMax) / 2f + new Vector2(w % 2, h % 2))._x0y();
    }
    private void GenerateTexture(Graph graph, Vector2Int min, Vector2Int max)
    {
        Destroy(coverTexture);

        var w = max.x - min.x + 1;
        var h = max.y - min.y + 1;

        coverTexture = new(w, h)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point
        };
        coverTexture.SetPixelData(new Color[w * h], 0);
        foreach (var node in graph.Nodes)
        {
            var local = ToTextureCoordinate(node.position);
            coverTexture.SetPixel(local.x, local.y, Color.white);
        }
        coverTexture.Apply();
    }

    private Vector2Int ToTextureCoordinate(Vector2Int NodePosition) => NodePosition - graphNodeMin;
}
