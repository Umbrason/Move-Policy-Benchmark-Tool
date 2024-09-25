using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;

public class Graph
{
    public List<Node> Nodes;

    public Graph(List<Node> nodes)
    {
        Nodes = nodes;
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
                foreach (var nb in Neighbourhood)
                {
                    var nbx = x + nb.Item1;
                    var nby = y + nb.Item2;
                    if (nbx < 0 || nbx >= width || nby < 0 || nby >= height) continue;
                    var nbNode = nodes[nbx, nby];
                    if (nbNode == null) continue;
                    node.Neighbours.Add(nbNode);
                }
            }
        return new(nodeKeys.Select(key => nodes[key.Item1, key.Item2]).ToList());
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
                foreach (var nb in Neighbourhood)
                {
                    var nbx = x + nb.Item1;
                    var nby = y + nb.Item2;
                    if (nbx < 0 || nbx >= w || nby < 0 || nby >= h) continue;
                    var nbNode = nodes[nbx, nby];
                    if (nbNode == null) continue;
                    node.Neighbours.Add(nbNode);
                }
            }
        return new(nodeKeys.Select(key => nodes[key.Item1, key.Item2]).ToList());
    }

    private PrecalculatedAStarPaths _Paths;
    private PrecalculatedAStarPaths Paths { get => _Paths ??= new(this); }
    public List<Node> FromTo(Node start, Node end) => Paths.FromTo(start, end);
    public void PrecalcAStarPaths() => _Paths ??= new(this);

    public HashSet<Node> CalculateTargetCover(Node targetNode, List<Node> pursuerNodes)
    {
        var cover = new HashSet<Node>();
        foreach (var node in Nodes)
        {
            var targetDistance = FromTo(targetNode, node).Count;
            var bestCopDistance = pursuerNodes.Min(n => FromTo(n, node).Count);
            if (targetDistance < bestCopDistance) cover.Add(node);
        }
        return cover;
    }

    public class PrecalculatedAStarPaths
    {
        internal Dictionary<Node, Dictionary<Node, List<Node>>> cache = new();
        internal PrecalculatedAStarPaths() { }
        public PrecalculatedAStarPaths(Graph graph)
        {
            Precalculate(graph);
        }
        public void Precalculate(Graph graph)
        {
            var threads = new List<Thread>();
            foreach (var from in graph.Nodes)
            {
                cache[from] = new();
                var thread = new Thread(() =>
                {
                    SortedList<float, Node> ExploreQueue = new(new DuplicateKeyComparer<float>()) { { 0, from } };
                    Dictionary<Node, Node> predecessors = new() { { from, from } };

                    Dictionary<Node, float> Costs = new();
                    foreach (var node in graph.Nodes) Costs[node] = float.PositiveInfinity;
                    Costs[from] = 0;

                    while (ExploreQueue.Count > 0)
                    {
                        var current = ExploreQueue.Values[0];
                        ExploreQueue.RemoveAt(0);
                        foreach (var neighbour in current.Neighbours)
                        {
                            var nbCost = Costs[current] + current.Distance(neighbour);
                            if (nbCost >= Costs[neighbour]) continue;
                            predecessors[neighbour] = current;
                            Costs[neighbour] = nbCost;
                            if (!ExploreQueue.ContainsValue(neighbour)) ExploreQueue.Add(nbCost, neighbour);
                        }
                    }
                    foreach (var to in graph.Nodes) cache[from][to] = Reconstruct(predecessors, to);
                });
                threads.Add(thread);
                thread.Start();
            }
            foreach (var thread in threads)
                thread.Join();
        }

        private static List<Node> Reconstruct(Dictionary<Node, Node> predecessors, Node node)
        {
            var path = new List<Node>();
            while (predecessors[node] != node)
            {
                path.Insert(0, node);
                node = predecessors[node];
            };
            return path;
        }

        public List<Node> FromTo(Node start, Node end)
        {
            return cache[start][end];
        }
    }

    public Graph DeepCopy()
    {
        var newNodes = Nodes.Select(node => new Node(node.position, node.index)).ToList();
        for (int i = 0; i < Nodes.Count; i++)
            newNodes[i].Neighbours.AddRange(Nodes[i].Neighbours.Select(nb => newNodes[nb.index]));
        var copy = new Graph(newNodes);
        copy._Paths = new() { cache = new() };
        foreach (var (startNode, DestinationPaths) in _Paths.cache)
        {
            var PathsByDestination = new Dictionary<Node, List<Node>>();
            foreach (var (destination, path) in DestinationPaths) PathsByDestination[copy.Nodes[destination.index]] = path.Select(node => copy.Nodes[node.index]).ToList();
            copy._Paths.cache[copy.Nodes[startNode.index]] = PathsByDestination;
        }
        return copy;
    }
}
