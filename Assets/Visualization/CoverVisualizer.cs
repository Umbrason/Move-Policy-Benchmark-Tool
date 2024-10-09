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
        if (Graph == null) return;
        int copSpeed = UnityGame.Game.teamSpeed[UnityGame.Game.Cops];
        int robberSpeed = UnityGame.Game.teamSpeed[UnityGame.Game.Robbers];

        var targetNodes = UnityGame.Game.Robbers.Agents.Where(a => !(a as Robber).Caught).Select(r => r.OccupiedNode.index).ToArray();
        var pursuerNodes = UnityGame.Game.Cops.Agents.Select(r => r.OccupiedNode.index).ToArray();

        var targetCover = UnityGame.Game.graph.CalculateTargetCovers(targetNodes, pursuerNodes, robberSpeed, copSpeed);
        foreach (var node in Graph.Nodes)
        {
            var local = ToTextureCoordinate(node.position);

            var isRobberCover = false;
            for (int i = 0; i < targetCover.Length; i++)
            {
                int[] arr = targetCover[i];
                for (int j = 0; j < arr.Length; j++)
                {
                    int n = arr[j];
                    if (node.index != n) continue;
                    isRobberCover = true;
                    break;
                }
                if (isRobberCover) break;
            }
            var team = isRobberCover ? UnityGame.Game.Robbers : UnityGame.Game.Cops;
            coverTexture.SetPixel(local.x, local.y, team.Color * new Color(1f, 1f, 1f, .5f));
        }
        coverTexture.Apply();
    }

    void OnGameStart() => this.Graph = UnityGame.Game.graph;

    private void SetGraph(Graph graph)
    {
        if (m_graph == graph) return;
        m_graph = graph;
        var positions = graph.Nodes.Select(n => n.position).ToArray();
        graphNodeMin = new Vector2Int(positions.Min(p => p.x), positions.Min(p => p.y));
        graphNodeMax = new Vector2Int(positions.Max(p => p.x), positions.Max(p => p.y));
        var w = graphNodeMax.x - graphNodeMin.x + 1;
        var h = graphNodeMax.y - graphNodeMin.y + 1;
        GenerateTexture(graph, graphNodeMin, graphNodeMax);
        Renderer.material.mainTexture = coverTexture;
        transform.localScale = new(w, h, 1);
        transform.position = ((Vector2)(graphNodeMin + graphNodeMax) / 2f)._x0y();
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
