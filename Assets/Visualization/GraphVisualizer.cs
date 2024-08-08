using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GraphVisualizer : MonoBehaviour
{
    private Texture2D graphTexture;
    Cached<MeshFilter> cached_filter;
    MeshFilter Filter => cached_filter[this];

    Cached<MeshRenderer> cached_renderer;
    MeshRenderer Renderer => cached_renderer[this];

    public Graph m_graph;
    public Graph Graph { get => m_graph; set => SetGraph(value); }

    Cached<ManualGame> cached_ManualGame = new(Cached<ManualGame>.GetOption.Parent);
    ManualGame ManualGame => cached_ManualGame[this];
    void Awake()
    {
        ManualGame.GameStart += OnGameStart;
    }

    void OnDestroy()
    {
        ManualGame.GameStart -= OnGameStart;
    }

    void OnGameStart() => this.Graph = ManualGame.Game.graph;

    private void SetGraph(Graph graph)
    {
        if (m_graph == graph) return;

        var positions = graph.Nodes.Select(n => n.position).ToArray();
        var min = new Vector2Int(positions.Min(p => p.x), positions.Min(p => p.y));
        var max = new Vector2Int(positions.Max(p => p.x), positions.Max(p => p.y));
        var w = max.x - min.x + 1;
        var h = max.y - min.y + 1;
        GenerateTexture(graph, min, max);
        Renderer.material.mainTexture = graphTexture;
        transform.localScale = new(w, h, 1);
        transform.position = ((Vector2)(min + max) / 2f + new Vector2(w % 2, h % 2))._x0y();
    }

    private void GenerateTexture(Graph graph, Vector2Int min, Vector2Int max)
    {
        Destroy(graphTexture);

        var w = max.x - min.x + 1;
        var h = max.y - min.y + 1;

        graphTexture = new(w, h)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point
        };
        graphTexture.SetPixelData(new Color[w * h], 0);
        foreach (var node in graph.Nodes)
        {
            var local = node.position - new Vector2Int(min.x, min.y);
            graphTexture.SetPixel(local.x, local.y, Color.white);
        }
        graphTexture.Apply();
    }

}
