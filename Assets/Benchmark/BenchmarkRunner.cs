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
    [SerializeField] private List<BenchmarkConfig> Configs = new();


    #region Runtime Data
    private readonly List<Thread> benchmarkWorkerThreads = new();
    private int totalBenchmarks = 0;
    private readonly Dictionary<BenchmarkConfig.GameConfig, ConcurrentQueue<BenchmarkRunTemplate>> openBenchmarks = new();
    private readonly Dictionary<BenchmarkConfig.GameConfig, ConcurrentBag<BenchmarkGame.Result>> finishedBenchmarks = new();
    #endregion 

    private readonly struct BenchmarkRunTemplate
    {
        private readonly BenchmarkConfig.GameConfig config;
        public readonly int CopCount => config.copCount;
        public readonly int RobberCount => config.robberCount;
        public readonly int CopSpeed => config.copSpeed;
        public readonly int RobberSpeed => config.robberSpeed;
        public readonly BenchmarkGame.CopStrategy CopStrategy => config.copStrategy;
        public readonly int timeout => config.timeout;
        public readonly Graph graph;
        public readonly int seed;

        public BenchmarkRunTemplate(Graph graph, BenchmarkConfig.GameConfig config, int seed)
        {
            this.graph = graph;
            this.config = config;
            this.seed = seed;
        }
    }

    void Start() => RunBenchmark();
    DateTime startTime;
    void RunBenchmark()
    {
        startTime = DateTime.Now;
        int threads = Mathf.CeilToInt(Environment.ProcessorCount / 1.5f);

        foreach (var benchmarkConfig in Configs)
            foreach (var gameConfig in benchmarkConfig.GameConfigs)
            {
                var random = new System.Random();
                openBenchmarks[gameConfig] = new();
                finishedBenchmarks[gameConfig] = new();
                for (int i = 0; i < benchmarkRunsPerCombination; i++)
                    openBenchmarks[gameConfig].Enqueue(new BenchmarkRunTemplate(gameConfig.OriginalMap, gameConfig, random.Next()));
                totalBenchmarks += openBenchmarks[gameConfig].Count;
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
        foreach (var gameConfig in openBenchmarks.Keys)
            while (openBenchmarks[gameConfig].TryDequeue(out var template))
            {
                if (!graphCopies.TryGetValue(template.graph, out var graphCopy)) graphCopy = template.graph.DeepCopy();
                graphCopies[template.graph] = graphCopy;
                template = new(graphCopies[template.graph], gameConfig, template.seed);
                finishedBenchmarks[gameConfig].Add(RunSingleBenchmarkGame(template));
            }
    }

    private BenchmarkGame.Result RunSingleBenchmarkGame(BenchmarkRunTemplate template)
    {
        var random = new System.Random(template.seed);
        var copPositions = Enumerable.Range(0, template.CopCount).Select(_ => random.Next(template.graph.Nodes.Length)).ToArray();
        var robberPositions = Enumerable.Range(0, template.RobberCount).Select(_ => random.Next(template.graph.Nodes.Length)).ToArray();
        BenchmarkGame game = new(template.graph, copPositions, robberPositions, turnLimit: template.timeout, copSpeed: template.CopSpeed, robberSpeed: template.RobberSpeed, copStrategy: template.CopStrategy);
        game.Run();
        return game.result;
    }

    void OnGUI()
    {
        var finished = finishedBenchmarks.Select(a => a.Value.Count).Sum();
        var total = totalBenchmarks;
        var percent = finished / (float)total;
        GUI.Label(new(15, 15, 1000, 30), $"Running {benchmarkWorkerThreads.Count(thread => thread.IsAlive)} Threads");
        GUI.Label(new(15, 45, 1000, 30), $"{finished.ToString().PadLeft(Mathf.FloorToInt(Mathf.Log10(totalBenchmarks)))} / {total} ({Mathf.FloorToInt(percent * 100)}%)");
        var timeSinceStart = DateTime.Now - startTime;
        var estimatedDurationSeconds = Mathf.CeilToInt((int)timeSinceStart.TotalSeconds / percent);
        var estimatedFinishTime = startTime + new TimeSpan(0, 0, estimatedDurationSeconds);
        GUI.Label(new(15, 75, 1000, 30), $"Running for: {timeSinceStart:hh\\:mm\\:ss}");
        GUI.Label(new(15, 105, 1000, 30), $"FROM: {startTime}");
        GUI.Label(new(15, 135, 1000, 30), $"TO  : {estimatedFinishTime}");
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
        foreach (var config in finishedBenchmarks.Keys)
        {
            StringBuilder builder = new();
            foreach (var result in finishedBenchmarks[config])
                builder.AppendLine(result.ToString());
            var fileName = $"{OutputDirectory}\\{DateTime.Now:MM-dd-yy HH։mm}\\{config.OriginalMap.name}\\{config.copCount}-Cops_vs_{config.robberCount}-Robbers_{config.copSpeed}։{config.robberSpeed}-speed_{config.copStrategy}.csv";
            var dirName = Path.GetDirectoryName(fileName);
            string partialDirName = "";
            foreach (var subfolder in dirName.Split(Path.DirectorySeparatorChar).Skip(1))
            {
                partialDirName += Path.DirectorySeparatorChar + subfolder;
                if (!Directory.Exists(partialDirName)) Directory.CreateDirectory(partialDirName);
            }
            var stream = File.Create(fileName);
            stream.Close();
            var content = builder.ToString();
            File.WriteAllText(fileName, content);
        }
    }
}
