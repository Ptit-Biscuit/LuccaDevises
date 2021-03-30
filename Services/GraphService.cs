using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using LuccaDevises.Exceptions;
using LuccaDevises.Models;

namespace LuccaDevises.Services
{
    public static class GraphService
    {
        public static List<GraphNode> CreateGraph(string filePath)
        {
            using var sr = new StreamReader(filePath);

            // Skip first line as it is not useful for the graph creation
            sr.ReadLine();

            // Process second line
            var secondLine = sr.ReadLine();

            if (string.IsNullOrEmpty(secondLine))
            {
                // Error: invalid format
                throw new GraphException(filePath, 1, "Required format is 'numberOfExchangeRate' (e.g '6')");
            }

            int currenciesLines;

            try
            {
                currenciesLines = int.Parse(secondLine);

                if (currenciesLines <= 0)
                {
                    // Warn: number of exchange rate given must be greater than 0
                    throw new FormatException("Number of exchange rate given must be greater than 0");
                }
            }
            catch (FormatException fe)
            {
                throw new GraphException(filePath, 1, fe.Message);
            }

            // Undirected graph: currencies as nodes, exchange rate as vertices
            var graph = new List<GraphNode>(currenciesLines * 2);

            // Process remaining lines
            string[] line;
            var lineIndex = 2;

            while ((line = sr.ReadLine()?.Split(';')) != null)
            {
                if (line.Length < 3)
                {
                    throw new GraphException(filePath, lineIndex,
                        "Required format is 'startCurrency;targetCurrency;exchangeRate' (e.g 'AUD;CHF;0.9661')");
                }

                try
                {
                    var exchangeRate = decimal.Parse(line[2], CultureInfo.InvariantCulture);

                    graph.Add(new GraphNode(line[0], line[1], decimal.Round(exchangeRate, 4)));
                    graph.Add(new GraphNode(line[1], line[0], decimal.Round(1 / exchangeRate, 4)));
                }
                catch (FormatException fe)
                {
                    throw new GraphException(filePath, lineIndex, fe.Message);
                }

                lineIndex++;
            }

            return graph;
        }

        public static decimal ExchangeRate(List<GraphNode> graph, string from, string to) =>
            ShortestPath(Children(Adjacency(graph), from), to).Aggregate(decimal.One,
                (acc, graphNode) => decimal.Round(acc * graphNode.ExchangeRate, 4));

        public static IEnumerable<GraphNode> ShortestPath(IReadOnlyCollection<GraphNode> children, string to)
        {
            var predecessor = children.First(node => node.To == to);

            return predecessor.From != ""
                ? ShortestPath(children, predecessor.From).Append(predecessor)
                : new List<GraphNode>();
        }

        public static List<GraphNode> Children(IReadOnlyDictionary<string, List<GraphNode>> adjacency, string root)
        {
            var search = new List<string> { root };

            // Useful to transform to directed graph
            var children = new List<GraphNode> { new("", root, decimal.One) };

            while (search.Count > 0)
            {
                var rootNode = search.Last();
                search.Remove(rootNode);

                // Visit all adjacent nodes that are not already seen
                adjacency[rootNode].ForEach(adjacent =>
                {
                    if (children.Select(child => child.To).Contains(adjacent.To)) return;

                    search.Add(adjacent.To);
                    children.Add(adjacent);
                });
            }

            return children;
        }

        public static Dictionary<string, List<GraphNode>> A(IReadOnlyCollection<GraphNode> graph) =>
            graph.ToLookup(node => node.From, node => graph.Where(node1 => node.From == node1.From).ToList())
                .ToDictionary(grouping => grouping.Key, grouping => grouping.First());

        public static Dictionary<string, List<GraphNode>> Adjacency(IReadOnlyCollection<GraphNode> graph)
        {
            return graph.Aggregate(
                new Dictionary<string, List<GraphNode>>(), (acc, graphNode) =>
                {
                    acc[graphNode.From] = graph.Where(node1 => graphNode.From == node1.From).ToList();
                    return acc;
                });
        }
    }
}