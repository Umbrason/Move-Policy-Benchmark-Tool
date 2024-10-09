using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;

public class Graph
{
    public readonly Node[] Nodes;
    private readonly bool[] pursuerExplored;
    readonly FastNodeQueue pursuerQueue;
    private readonly List<bool[]> targetCoverPool;
    readonly List<FastNodeQueue> targetNodeQueuePool;
    public string name;

    public Graph(Node[] nodes, string name, bool precalcAStar = true)
    {
        Nodes = nodes;

        pursuerExplored = new bool[Nodes.Length];
        pursuerQueue = new(Nodes.Length);
        targetNodeQueuePool = new();
        targetCoverPool = new();

        this.name = name;
        if (precalcAStar) PrecalcAStarPaths();
    }

    static readonly (int, int)[] Neighbourhood = new (int, int)[]
    {
            //N4
            (-1, 0),
            ( 1, 0),
            ( 0,-1),
            ( 0, 1),
            //N8
            (-1,-1),
            (-1, 1),
            ( 1,-1),
            ( 1, 1),
    };

    public static Graph FromMapFile(TextAsset textAsset)
    {
        var textReader = new StringReader(textAsset.text);
        var typeLine = textReader.ReadLine();
        var heightLine = textReader.ReadLine();
        var widthLine = textReader.ReadLine();
        var mapLine = textReader.ReadLine();

        if (!typeLine.StartsWith("type")) throw new FormatException();
        if (!heightLine.StartsWith("height")) throw new FormatException();
        if (!widthLine.StartsWith("width")) throw new FormatException();
        if (!mapLine.StartsWith("map")) throw new FormatException();

        var width = int.Parse(widthLine["width".Length..]);
        var height = int.Parse(heightLine["height".Length..]);
        Node[,] nodes = new Node[width, height];
        HashSet<(int, int)> nodeKeys = new();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var nextChar = textReader.Read();
                var isWalkable = nextChar == '.';
                if (!isWalkable) continue;
                nodes[x, y] = new(new(x, y), nodeKeys.Count);
                nodeKeys.Add((x, y));
            }
            var lineEnd = textReader.Read();
            if (lineEnd != '\n' && lineEnd != -1) throw new FormatException($"expected '\\n' or '-1' but found {lineEnd}");
        }

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var node = nodes[x, y];
                if (node == null) continue;
                int nbID = 0;
                foreach (var nb in Neighbourhood)
                {
                    var nbx = x + nb.Item1;
                    var nby = y + nb.Item2;
                    if (nbx < 0 || nbx >= width || nby < 0 || nby >= height) continue;
                    var nbNode = nodes[nbx, nby];
                    if (nbNode == null) continue;
                    node.Neighbours[nbID++] = nbNode;
                    node.neighbourCount = nbID;
                }
            }
        return new(nodeKeys.Select(key => nodes[key.Item1, key.Item2]).ToArray(), textAsset.name);
    }

    public static Graph FromTexture(Texture2D texture)
    {
        var w = texture.width;
        var h = texture.height;
        Node[,] nodes = new Node[w, h];
        HashSet<(int, int)> nodeKeys = new();
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                var pixel = texture.GetPixel(x, y);
                var isWalkable = pixel.grayscale > .5f;
                if (!isWalkable) continue;
                nodes[x, y] = new(new(x, y), nodeKeys.Count);
                nodeKeys.Add((x, y));
            }
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                var node = nodes[x, y];
                if (node == null) continue;
                int nbID = 0;
                foreach (var nb in Neighbourhood)
                {
                    var nbx = x + nb.Item1;
                    var nby = y + nb.Item2;
                    if (nbx < 0 || nbx >= w || nby < 0 || nby >= h) continue;
                    var nbNode = nodes[nbx, nby];
                    if (nbNode == null) continue;
                    node.Neighbours[nbID++] = nbNode;
                    node.neighbourCount = nbID;
                }
            }
        return new(nodeKeys.Select(key => nodes[key.Item1, key.Item2]).ToArray(), texture.name);
    }

    public int CalculateTargetCoverSize(int targetNode, int[] pursuerNodes) => CalculateTargetCoverSizes(new[] { targetNode }, pursuerNodes)[0];
    public int[] CalculateTargetCoverSizes(int[] targetNodes, int[] pursuerNodes, int targetSpeed = 1, int pursuerSpeed = 1)
    {
        UnityEngine.Profiling.Profiler.BeginSample("GCAlloc Arrays");
        var targetQueues = new FastNodeQueue[targetNodes.Length];
        var targetCovers = new bool[targetNodes.Length][];
        var targetCoverCounts = new int[targetNodes.Length];
        UnityEngine.Profiling.Profiler.EndSample();

        while (targetCoverPool.Count <= targetNodes.Length) targetCoverPool.Add(new bool[Nodes.Length]);
        while (targetNodeQueuePool.Count <= targetNodes.Length) targetNodeQueuePool.Add(new(Nodes.Length));

        for (int targetID = 0; targetID < targetNodes.Length; targetID++)
        {
            int target = targetNodes[targetID];
            targetCovers[targetID] = targetCoverPool[targetID];
            targetQueues[targetID] = targetNodeQueuePool[targetID];
            targetQueues[targetID].Reset();
            targetQueues[targetID].Enqueue(0, target);
            targetCoverCounts[targetID] = 1;
            for (int i = 0; i < Nodes.Length; i++)
                targetCovers[targetID][i] = false;
            targetCovers[targetID][target] = true;
        }

        #region init pursuers
        pursuerQueue.Reset();
        for (int node = 0; node < Nodes.Length; node++) pursuerExplored[node] = false;
        for (int pursuerID = 0; pursuerID < pursuerNodes.Length; pursuerID++)
        {
            int pursuer = pursuerNodes[pursuerID];
            if (pursuerExplored[pursuer]) continue;
            pursuerExplored[pursuer] = true;
            pursuerQueue.Enqueue(0, pursuer);
        }
        #endregion

        while (true)
        {
            var bestTargetID = -1;
            var minCostTarget = int.MaxValue;
            for (int targetID = 0; targetID < targetNodes.Length; targetID++)
            {
                var cost = targetQueues[targetID].NextCost;
                if (cost >= minCostTarget) continue;
                bestTargetID = targetID;
                minCostTarget = cost;
            }
            if (minCostTarget == int.MaxValue) break;
            var minCostPursuers = pursuerQueue.NextCost;

            if (minCostTarget < minCostPursuers)
            {
                targetQueues[bestTargetID].Dequeue(out var cost, out var nodeIndex);
                for (int i = 0; i < Nodes[nodeIndex].neighbourCount; i++)
                {
                    var nb = Nodes[nodeIndex].Neighbours[i];
                    if (targetCovers[bestTargetID][nb.index] || pursuerExplored[nb.index]) continue;
                    targetCovers[bestTargetID][nb.index] = true;
                    targetCoverCounts[bestTargetID]++;
                    targetQueues[bestTargetID].Enqueue(minCostTarget + pursuerSpeed, nb.index);
                }
            }
            else
            {
                pursuerQueue.Dequeue(out var cost, out var nodeIndex);
                for (int i = 0; i < Nodes[nodeIndex].neighbourCount; i++)
                {
                    var nb = Nodes[nodeIndex].Neighbours[i];
                    if (pursuerExplored[nb.index]) continue;
                    pursuerExplored[nb.index] = true;
                    pursuerQueue.Enqueue(minCostPursuers + targetSpeed, nb.index);
                }
            }
        }
        return targetCoverCounts;
    }
    public int[][] CalculateTargetCovers(int[] targetNodes, int[] pursuerNodes, int targetSpeed = 1, int pursuerSpeed = 1)
    {
        var targetQueues = new FastNodeQueue[targetNodes.Length];
        var targetCovers = new bool[targetNodes.Length][];
        var targetCoverCounts = new int[targetNodes.Length];

        while (targetCoverPool.Count <= targetNodes.Length) targetCoverPool.Add(new bool[Nodes.Length]);
        while (targetNodeQueuePool.Count <= targetNodes.Length) targetNodeQueuePool.Add(new(Nodes.Length));

        for (int targetID = 0; targetID < targetNodes.Length; targetID++)
        {
            int target = targetNodes[targetID];
            targetCovers[targetID] = targetCoverPool[targetID];
            targetQueues[targetID] = targetNodeQueuePool[targetID];
            targetQueues[targetID].Reset();
            targetQueues[targetID].Enqueue(0, target);
            targetCoverCounts[targetID] = 1;
            for (int i = 0; i < Nodes.Length; i++)
                targetCovers[targetID][i] = false;
            targetCovers[targetID][target] = true;
        }

        #region init pursuers
        pursuerQueue.Reset();
        for (int node = 0; node < Nodes.Length; node++) pursuerExplored[node] = false;
        for (int pursuerID = 0; pursuerID < pursuerNodes.Length; pursuerID++)
        {
            int pursuer = pursuerNodes[pursuerID];
            if (pursuerExplored[pursuer]) continue;
            pursuerExplored[pursuer] = true;
            pursuerQueue.Enqueue(0, pursuer);
        }
        #endregion

        while (true)
        {
            var bestTargetID = -1;
            var minCostTarget = int.MaxValue;
            for (int targetID = 0; targetID < targetNodes.Length; targetID++)
            {
                var cost = targetQueues[targetID].NextCost;
                if (cost >= minCostTarget) continue;
                bestTargetID = targetID;
                minCostTarget = cost;
            }
            if (minCostTarget == int.MaxValue) break;
            var minCostPursuers = pursuerQueue.NextCost;

            if (minCostTarget < minCostPursuers)
            {
                targetQueues[bestTargetID].Dequeue(out var cost, out var nodeIndex);
                for (int i = 0; i < Nodes[nodeIndex].neighbourCount; i++)
                {
                    var nb = Nodes[nodeIndex].Neighbours[i];
                    if (targetCovers[bestTargetID][nb.index] || pursuerExplored[nb.index]) continue;
                    targetCovers[bestTargetID][nb.index] = true;
                    targetCoverCounts[bestTargetID]++;
                    targetQueues[bestTargetID].Enqueue(minCostTarget + pursuerSpeed, nb.index);
                }
            }
            else
            {
                pursuerQueue.Dequeue(out var cost, out var nodeIndex);
                for (int i = 0; i < Nodes[nodeIndex].neighbourCount; i++)
                {
                    var nb = Nodes[nodeIndex].Neighbours[i];
                    if (pursuerExplored[nb.index]) continue;
                    pursuerExplored[nb.index] = true;
                    pursuerQueue.Enqueue(minCostPursuers + targetSpeed, nb.index);
                }
            }
        }
        var targetCoverArrays = new int[targetNodes.Length][];
        for (int targetID = 0; targetID < targetNodes.Length; targetID++)
        {
            var arr = new int[targetCoverCounts[targetID]];
            int j = 0;
            for (int i = 0; i < Nodes.Length; i++)
                if (targetCovers[targetID][i])
                {
                    arr[j] = i;
                    j++;
                }
            targetCoverArrays[targetID] = arr;
        }
        return targetCoverArrays;
    }

    readonly struct CoverExplorationNode
    {
        public readonly int cost;
        public readonly int index;

        public CoverExplorationNode(int cost, int index)
        {
            this.cost = cost;
            this.index = index;
        }
    }


    //TODO let this be an Indx->Indx dict instead. also make the path an array not list
    internal int[,] pathCache;
    internal int[,] distanceCache;

    public void PrecalcAStarPaths()
    {
        pathCache = new int[Nodes.Length, Nodes.Length];
        distanceCache = new int[Nodes.Length, Nodes.Length];
        int[] predecessors = new int[Nodes.Length];
        int[] Costs = new int[Nodes.Length];
        SortedList<int, int> ExploreQueue = new(new DuplicateIntKeyComparer());
        for (int from = 0; from < Nodes.Length; from++)
        {
            for (int i = 0; i < Nodes.Length; i++)
            {
                Costs[i] = int.MaxValue;
                predecessors[i] = -1;
            }
            Costs[from] = 0;
            predecessors[from] = from;
            ExploreQueue.Add(0, from);

            while (ExploreQueue.Count > 0)
            {
                var current = ExploreQueue.Values[0];
                ExploreQueue.RemoveAt(0);
                var currentNode = Nodes[current];
                for (int i = 0; i < currentNode.neighbourCount; i++)
                {
                    Node neighbour = currentNode.Neighbours[i];
                    var nbCost = Costs[current] + 1;
                    if (nbCost >= Costs[neighbour.index]) continue;
                    predecessors[neighbour.index] = current;
                    Costs[neighbour.index] = nbCost;
                    if (!ExploreQueue.ContainsValue(neighbour.index)) ExploreQueue.Add(nbCost, neighbour.index);
                }
            }
            for (int to = 0; to < Nodes.Length; to++)
            {
                distanceCache[from, to] = Costs[to];
                pathCache[from, to] = predecessors[to];
            }
            ExploreQueue.Clear();
        }
    }

    private int[] Reconstruct(int from, int to)
    {
        if (distanceCache[from, to] == int.MaxValue) return null;
        var path = new int[distanceCache[from, to]];
        for (int i = path.Length - 1; i >= 0; i--)
        {
            path[i] = to;
            to = pathCache[from, to];
        }
        return path;
    }

    public int Distance(Node from, Node to) => Distance(from.index, to.index);
    public int Distance(int from, int to) => distanceCache[from, to];

    public int[] FromTo(int start, int end) => Reconstruct(start, end);
    public int[] FromTo(Node start, Node end) => FromTo(start.index, end.index);

    public Node[] FromToNodes(Node start, Node end)
    {
        return FromTo(start, end).Select(indx => Nodes[indx]).ToArray();
    }


    //TODO remove graph copying alltogether. use one readonly graph and only the agents know what node they are on
    public Graph DeepCopy()
    {
        var newNodes = Nodes.Select(node => new Node(node.position, node.index)).ToArray();
        for (int i = 0; i < Nodes.Length; i++)
        {
            newNodes[i].neighbourCount = Nodes[i].neighbourCount;
            for (int j = 0; j < 8; j++)
                newNodes[i].Neighbours[j] = Nodes[i].Neighbours[j];
        }
        var copy = new Graph(newNodes, name, false)
        {
            pathCache = pathCache,
            distanceCache = distanceCache
        };
        return copy;
    }
}
