using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Benchmark", menuName = "Benchmark Config")]
public class BenchmarkConfig : ScriptableObject
{
    [SerializeField] MapConfig[] Maps;
    [SerializeField] TeamSpeedConfig[] TeamSpeeds;
    [SerializeField] TeamSizeConfig[] TeamSizes;
    [SerializeField] CopStrategyConfig[] Strategies;

    public GameConfig[] GameConfigs => GenerateGameConfigs();

    private GameConfig[] GenerateGameConfigs()
    {
        var configs = new GameConfig[Maps.Count(c => c.enabled) * TeamSizes.Count(c => c.enabled) * TeamSpeeds.Count(c => c.enabled) * Strategies.Count(c => c.enabled)];
        int id = 0;
        foreach (var map in Maps.Where(c => c.enabled))
        {
            var graph = Graph.FromMapFile(map.mapFile);
            graph.PrecalcAStarPaths();
            foreach (var size in TeamSizes.Where(c => c.enabled))
                foreach (var speed in TeamSpeeds.Where(c => c.enabled))
                    foreach (var strategy in Strategies.Where(c => c.enabled))
                    {
                        configs[id++] = new(graph, size.RobberCount, size.CopCount, speed.CopSpeed, speed.RobberSpeed, strategy.Strategy, map.timeout);
                    }
        }
        return configs;
    }

    public readonly struct GameConfig
    {
        public readonly Graph OriginalMap;
        public readonly int timeout;
        public readonly int robberCount;
        public readonly int copCount;
        public readonly int copSpeed;
        public readonly int robberSpeed;
        public readonly BenchmarkGame.CopStrategy copStrategy;

        public GameConfig(Graph originalMap, int robberCount, int copCount, int copSpeed, int robberSpeed, BenchmarkGame.CopStrategy copStrategy, int timeout)
        {
            OriginalMap = originalMap;
            this.timeout = timeout;
            this.robberCount = robberCount;
            this.copCount = copCount;
            this.copSpeed = copSpeed;
            this.robberSpeed = robberSpeed;
            this.copStrategy = copStrategy;
        }
    }

    [System.Serializable]
    private class MapConfig
    {
        public bool enabled;
        public TextAsset mapFile;
        public int timeout;
    }

    [System.Serializable]
    private class TeamSpeedConfig
    {
        public bool enabled;
        public int CopSpeed;
        public int RobberSpeed;
    }

    [System.Serializable]
    private class TeamSizeConfig
    {
        public bool enabled;
        public int CopCount;
        public int RobberCount;
    }
    [System.Serializable]
    private class CopStrategyConfig
    {
        public bool enabled;
        public BenchmarkGame.CopStrategy Strategy;
    }
}
