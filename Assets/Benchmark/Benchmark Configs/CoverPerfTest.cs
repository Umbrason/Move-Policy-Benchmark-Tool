using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverPerfTest : MonoBehaviour
{
    [SerializeField] TextAsset Map;
    private Graph graph;

    void Awake()
    {
        graph = Graph.FromMapFile(Map);
    }
    void Update()
    {
        for (int i = 0; i < 1; i++)
            graph.CalculateTargetCoverSize(0, new[] { 100, 200, 400 });
    }
}
