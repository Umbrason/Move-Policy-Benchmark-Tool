using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

public class BenchmarkRunner : MonoBehaviour
{
    public string OutputDirectory;
    public int benchmarkRunsPerCombination = 100;
    private List<Thread> benchmarkWorkerThreads = new();


    private int totalBenchmarks = 0;
    Dictionary<BenchmarkRunConfig, ConcurrentQueue<BenchmarkRunTemplate>> openBenchmarks = new();
    Dictionary<BenchmarkRunConfig, ConcurrentBag<BenchmarkGame.Result>> finishedBenchmarks = new();

    private readonly struct BenchmarkRunTemplate
    {
        public readonly Graph graph;
        private readonly BenchmarkRunConfig config;
        public readonly int CopCount => config.CopCount;
        public readonly int RobberCount => config.RobberCount;
        public readonly int CopSpeed => config.CopSpeed;
        public readonly int RobberSpeed => config.RobberSpeed;
        public readonly BenchmarkGame.CopStrategy CopStrategy => config.Strategy;
        public readonly int seed;

        public BenchmarkRunTemplate(Graph graph, BenchmarkRunConfig config, int seed)
        {
            this.graph = graph;
            this.config = config;
            this.seed = seed;
        }
    }

    [SerializeField] private List<BenchmarkRunConfig> Configs = new();

    [System.Serializable]
    class BenchmarkRunConfig
    {
        [SerializeField] public Texture2D Map;
        [SerializeField] public int CopCount;
        [SerializeField] public int RobberCount;
        [SerializeField] public int CopSpeed;
        [SerializeField] public int RobberSpeed;
        [SerializeField] public BenchmarkGame.CopStrategy Strategy;
    }

    void Start() => RunBenchmark();
    void RunBenchmark()
    {
        int threads = Environment.ProcessorCount / 2;

        foreach (var config in Configs)
        {
            var graph = Graph.FromTexture(config.Map);
            graph.PrecalcAStarPaths();
            var random = new System.Random();
            openBenchmarks[config] = new();
            finishedBenchmarks[config] = new();
            for (int i = 0; i < benchmarkRunsPerCombination; i++)
                openBenchmarks[config].Enqueue(new BenchmarkRunTemplate(graph, config, random.Next()));
            totalBenchmarks += openBenchmarks[config].Count;
        }
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
        foreach (var config in Configs)
            while (openBenchmarks[config].TryDequeue(out var template))
            {
                if (!graphCopies.TryGetValue(template.graph, out var graphCopy)) graphCopy = template.graph.DeepCopy();
                graphCopies[template.graph] = graphCopy;
                template = new(graphCopies[template.graph], config, template.seed);
                finishedBenchmarks[config].Add(RunSingleBenchmarkGame(template));
            }
    }

    private BenchmarkGame.Result RunSingleBenchmarkGame(BenchmarkRunTemplate template)
    {
        var random = new System.Random(template.seed);
        var copPositions = Enumerable.Range(0, template.CopCount).Select(_ => random.Next(template.graph.Nodes.Count)).ToArray();
        var robberPositions = Enumerable.Range(0, template.RobberCount).Select(_ => random.Next(template.graph.Nodes.Count)).ToArray();
        BenchmarkGame game = new(template.graph, copPositions, robberPositions, copSpeed: template.CopSpeed, robberSpeed: template.RobberSpeed, copStrategy: template.CopStrategy);
        game.Run();
        return game.result;
    }

    void OnGUI()
    {
        GUI.Label(new(15, 15, 1000, 30), $"Running {benchmarkWorkerThreads.Count(thread => thread.IsAlive)} Threads");
        GUI.Label(new(15, 45, 1000, 50), $"{finishedBenchmarks.Select(a => a.Value.Count).Sum()} / {totalBenchmarks}".PadLeft(totalBenchmarks * 2 + 3));
    }

    void Update()
    {
        if (finishedBenchmarks.Select(a => a.Value.Count).Sum() == totalBenchmarks)
        {
            SaveResults();
            enabled = false;
        }
    }

    void SaveResults()
    {
        foreach (var config in Configs)
        {
            StringBuilder builder = new();
            foreach (var result in finishedBenchmarks[config])
                builder.AppendLine(result.ToString());                
            var fileName = $"{config.Map.name}_{config.CopCount}-Cops_vs_{config.RobberCount}-Robbers_{config.CopSpeed}։{config.RobberSpeed}-speed_{config.Strategy}_{DateTime.Now.ToString("HH-mm-MM-dd-yy")}";
            var stream = File.Create($"{OutputDirectory}\\{fileName}.csv");
            stream.Close();
            var content = builder.ToString();
            File.WriteAllText($"{OutputDirectory}\\{fileName}.csv", content);
        }
    }
}
