public class BenchmarkConfig
{
    public MapConfig[] Maps;
    public TeamSpeedConfig[] TeamSpeeds;
    public TeamSizeConfig[] TeamSizes;
    public BenchmarkGame.CopStrategy[] Strategies;

    public GameConfig[] GameConfigs => GenerateGameConfigs();

    private GameConfig[] GenerateGameConfigs()
    {
        var configs = new GameConfig[Maps.Length * TeamSizes.Length * TeamSpeeds.Length * Strategies.Length];
        int id = 0;
        foreach (var map in Maps)
        {
            var graph = Graph.FromMapFile(map.mapFile);
            graph.PrecalcAStarPaths();
            foreach (var size in TeamSizes)
                foreach (var speed in TeamSpeeds)
                    foreach (var strategy in Strategies)
                    {
                        configs[id++] = new(graph, size.RobberCount, size.CopCount, speed.CopSpeed, speed.RobberSpeed, strategy, map.timeout);
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
    public class MapConfig
    {
        public string mapFile;
        public int timeout;
    }

    [System.Serializable]
    public class TeamSpeedConfig
    {
        public int CopSpeed;
        public int RobberSpeed;
    }

    [System.Serializable]
    public class TeamSizeConfig
    {
        public int CopCount;
        public int RobberCount;
    }
}
