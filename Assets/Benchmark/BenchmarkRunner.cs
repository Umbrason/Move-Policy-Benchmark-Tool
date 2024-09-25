using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

public class BenchmarkRunner : MonoBehaviour
{
    public List<int> CopCounts = new();
    public List<int> RobberCounts = new();
    public List<int> CopSpeeds = new();
    public List<int> RobberSpeeds = new();
    public List<Texture2D> Maps = new();

    public int benchmarkRunsPerCombination = 100;

    private List<Thread> benchmarkWorkerThreads = new();


    private int totalBenchmarks = 0;
    ConcurrentQueue<BenchmarkRunTemplate> openBenchmarks = new();
    ConcurrentBag<BenchmarkGame.Result> finishedBenchmarks = new();

    private readonly struct BenchmarkRunTemplate
    {
        public readonly Graph graph;
        public readonly int copCount;
        public readonly int robberCount;
        public readonly int copSpeed;
        public readonly int robberSpeed;
        public readonly int seed;

        public BenchmarkRunTemplate(Graph graph, int copCount, int robberCount, int copSpeed, int robberSpeed, int seed)
        {
            this.graph = graph;
            this.copCount = copCount;
            this.robberCount = robberCount;
            this.copSpeed = copSpeed;
            this.robberSpeed = robberSpeed;
            this.seed = seed;
        }
    }

    void Start() => RunBenchmark();
    void RunBenchmark()
    {
        int threads = Environment.ProcessorCount;

        foreach (var map in Maps)
        {
            var graph = Graph.FromTexture(map);
            graph.PrecalcAStarPaths();
            var random = new System.Random();
            foreach (var cc in CopCounts)
                foreach (var rc in RobberCounts)
                    foreach (var cs in CopSpeeds)
                        foreach (var rs in RobberSpeeds)
                            for (int i = 0; i < benchmarkRunsPerCombination; i++)
                                openBenchmarks.Enqueue(new BenchmarkRunTemplate(graph, cc, rc, cs, rs, random.Next()));
        }
        totalBenchmarks = openBenchmarks.Count;
        for (int i = 0; i < threads; i++)
        {
            Thread thread = new Thread(RunQueuedBenchmarks);
            thread.Start();
            benchmarkWorkerThreads.Add(thread);
        }
    }


    private void RunQueuedBenchmarks()
    {
        var graphCopies = new Dictionary<Graph, Graph>();
        while (openBenchmarks.TryDequeue(out var template))
        {
            if (!graphCopies.TryGetValue(template.graph, out var graphCopy)) graphCopy = template.graph.DeepCopy();
            graphCopies[template.graph] = graphCopy;
            template = new(graphCopies[template.graph], template.copCount, template.robberCount, template.copSpeed, template.robberSpeed, template.seed);
            finishedBenchmarks.Add(RunSingleBenchmarkGame(template));
        }
    }

    private BenchmarkGame.Result RunSingleBenchmarkGame(BenchmarkRunTemplate template)
    {
        var random = new System.Random(template.seed);
        var copPositions = Enumerable.Range(0, template.copCount).Select(_ => random.Next(template.graph.Nodes.Count)).ToArray();
        var robberPositions = Enumerable.Range(0, template.robberCount).Select(_ => random.Next(template.graph.Nodes.Count)).ToArray();
        BenchmarkGame game = new(template.graph, copPositions, robberPositions, copSpeed: template.copSpeed, robberSpeed: template.robberSpeed);
        game.Run();
        return game.result;
    }

    void OnGUI()
    {
        GUI.Label(new(15, 15, 1000, 30), $"Running {benchmarkWorkerThreads.Count(thread => thread.IsAlive)} Threads");
        GUI.Label(new(15, 45, 1000, 50), $"{finishedBenchmarks.Count} / {totalBenchmarks}".PadLeft(totalBenchmarks * 2 + 3));
    }

    void Update()
    {
        if (finishedBenchmarks.Count == totalBenchmarks)
        {
            SaveResults();
            enabled = false;
        }
    }

    void SaveResults()
    {
        StringBuilder builder = new();
        foreach (var result in finishedBenchmarks)
            builder.AppendLine(result.ToString());
        Debug.Log(builder.ToString());
    }
}
