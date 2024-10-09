using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class AnalyseBenchmarkRuns : MonoBehaviour
{
    public string defaultPath;
    void Start()
    {
        SimpleFileBrowser.FileBrowser.ShowLoadDialog(Analyze, () => { }, SimpleFileBrowser.FileBrowser.PickMode.Folders, true, defaultPath);
    }

    public void Analyze(string[] folders)
    {
        var runs = ParseFolders(folders);
        var builder = new StringBuilder();
        foreach (var run in runs)
        {
            var runAnalysis = new RunAnalysis(run.Key, run.Value);
            builder.AppendLine(runAnalysis.ToString());
        }
        Debug.Log(builder);
    }

    private struct RunAnalysis
    {
        public RunAnalysis(string filePath, BenchmarkGame.Result[] results)
        {
            this.filePath = filePath;
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            string pattern = @".*\\(?<mapName>[A-Z0-9]+)_(?<turnLimit>\d+)\\(?<copCount>\d+)-Cops_vs_(?<robberCount>\d+)-Robbers_(?<copSpeed>\d+)։(?<robberSpeed>\d+)-speed_(?<copStrategy>\w+)";
            var match = Regex.Match(filePath, pattern);
            if (!match.Success) throw new Exception("");
            var mapName = match.Groups["mapName"].Value;
            var turnLimit = int.Parse(match.Groups["turnLimit"].Value);
            var copCount = int.Parse(match.Groups["copCount"].Value);
            var robberCount = int.Parse(match.Groups["robberCount"].Value);
            var copSpeed = int.Parse(match.Groups["copSpeed"].Value);
            var robberSpeed = int.Parse(match.Groups["robberSpeed"].Value);
            var copStrategy = Enum.Parse<BenchmarkGame.CopStrategy>(match.Groups["copStrategy"].Value);
            config = new(new(new Node[0], mapName, false), robberCount, copCount, copSpeed, robberSpeed, copStrategy, turnLimit);

            averageTurns = results.Sum(r => r.turns) / (float)results.Length;
            averageCaught = results.Sum(r => r.caughtAmount) / (float)results.Length;
            successRate = results.Sum(r => r.caughtAmount == robberCount ? 1 : 0) / (float)results.Length * 100f;
        }
        public string filePath;
        public BenchmarkConfig.GameConfig config;

        public float averageTurns;
        public float averageCaught;
        public float successRate;

        public override string ToString()
        {
            return $"map:{config.OriginalMap.name}, teams:{config.copCount} Cops vs {config.robberCount} Robbers, speed: {config.copSpeed} cop - {config.robberSpeed} robber, strategy: {config.copStrategy}, turnLimit:{config.timeout}, avgTurns: {averageTurns:0.00}, avgCaught: {averageCaught:0.00}, successRate: {successRate:0.00}";
        }
    }


    public Dictionary<string, BenchmarkGame.Result[]> ParseFolders(string[] folders)
    {
        var directories = new Queue<DirectoryInfo>();
        var results = new Dictionary<string, BenchmarkGame.Result[]>();
        foreach (var folder in folders) directories.Enqueue(new(folder));
        while (directories.Count > 0)
        {
            var dir = directories.Dequeue();
            foreach (var file in dir.EnumerateFiles()) results[file.FullName] = ParseBenchmarkFile(file.FullName);
            foreach (var subdir in dir.EnumerateDirectories()) directories.Enqueue(subdir);
        }
        return results;
    }

    public BenchmarkGame.Result[] ParseBenchmarkFile(string file)
    {
        if (!File.Exists(file)) return new BenchmarkGame.Result[0];
        var lines = File.ReadAllLines(file);
        lines = lines.Where(line => line != "").ToArray();
        var results = lines.Select(BenchmarkGame.Result.Parse).ToArray();
        return results;
    }
}
