using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace LuccaDevises
{
    public class GraphNode
    {
        public readonly string From;
        public readonly string To;
        public readonly decimal ExchangeRate;

        public GraphNode(string from, string to, decimal exchangeRate)
        {
            From = from;
            To = to;
            ExchangeRate = exchangeRate;
        }
    }

    public static class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                // Critical failure: file path does not exists
                Console.Error.WriteLine("A file path is required as sole argument");
                return -1;
            }

            var filePath = args[0];

            if (!File.Exists(filePath))
            {
                // Critical failure: file path does not exists
                Console.Error.WriteLine($"File path `{filePath}` does not exists");
                return -1;
            }

            Tuple<string, int, string> start;
            List<GraphNode> graph;

            using (var sr = new StreamReader(filePath))
            {
                // Process first line
                var firstLine = sr.ReadLine()?.Split(';');

                if (firstLine == null || firstLine.Length < 3)
                {
                    // Critical failure: nothing to process
                    WriteError(filePath, 0,
                        "Required format is 'startCurrency;amountToConvert;targetCurrency' (e.g 'EUR;550;JPY')");
                    return -1;
                }

                try
                {
                    var amount = int.Parse(firstLine[1]);

                    if (amount <= 0)
                    {
                        // Error: amount must be greater than 0
                        throw new FormatException("Amount must be greater than 0");
                    }

                    start = Tuple.Create(firstLine[0], amount, firstLine[2]);
                }
                catch (FormatException fe)
                {
                    WriteError(filePath, 0, fe.Message);
                    return -1;
                }

                // Process second line
                var secondLine = sr.ReadLine();

                if (string.IsNullOrEmpty(secondLine))
                {
                    // Error: invalid format
                    WriteError(filePath, 1, "Required format is 'numberOfExchangeRate' (e.g '6')");
                    return -1;
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
                    WriteError(filePath, 1, fe.Message);
                    return -1;
                }

                // Undirected graph: currencies as nodes, exchange rate as vertices
                graph = new List<GraphNode>(currenciesLines * 2);

                // Process remaining lines
                string[] line;
                var lineIndex = 2;

                while ((line = sr.ReadLine()?.Split(';')) != null)
                {
                    if (line.Length < 3)
                    {
                        WriteError(filePath, lineIndex,
                            "Required format is 'startCurrency;targetCurrency;exchangeRate' (e.g 'AUD;CHF;0.9661')");
                        return -1;
                    }

                    try
                    {
                        var exchangeRate = decimal.Parse(line[2], CultureInfo.InvariantCulture);

                        graph.Add(new GraphNode(line[0], line[1], decimal.Round(exchangeRate, 4)));
                        graph.Add(new GraphNode(line[1], line[0], decimal.Round(1 / exchangeRate, 4)));
                    }
                    catch (FormatException fe)
                    {
                        WriteError(filePath, lineIndex, fe.Message);
                        return -1;
                    }

                    lineIndex++;
                }
            }

            Console.Write(ConvertCurrency(graph, start.Item1, start.Item3, start.Item2));

            return 0;
        }

        private static decimal ConvertCurrency(IReadOnlyCollection<GraphNode> graph, string from, string to, int amount)
        {
            return decimal.Ceiling(amount * ExchangeRate(graph, ShortestPath(graph, from, to)));
        }

        private static decimal ExchangeRate(IReadOnlyCollection<GraphNode> graph, IEnumerable<GraphNode> shortestPath)
        {
            return shortestPath
                .Select(node => graph.FirstOrDefault(node1 => node.From == node1.From && node.To == node1.To)?.ExchangeRate)
                .Where(arg => arg != null)
                .Aggregate(decimal.One, (arg1, arg2) => arg1 * arg2.Value);
        }

        private static IEnumerable<GraphNode> ShortestPath(IReadOnlyCollection<GraphNode> graph, string from, string to)
        {
            var predecessors = Predecessors(graph, from, to);
            var tmp = new GraphNode(to, "", decimal.One);
            var shortestPath = new List<GraphNode> { tmp };

            while (tmp.From != "")
            {
                tmp = predecessors.First(node => node.To == tmp.From);
                shortestPath.Add(tmp);
            }

            shortestPath.Reverse();

            return shortestPath;
        }

        private static List<GraphNode> Predecessors(IReadOnlyCollection<GraphNode> graph, string from, string to)
        {
            var adjacency = Adjacency(graph);
            var search = new List<string> { from };
            var paths = new List<string> { from };

            // Useful to transform to directed graph
            var predecessors = new List<GraphNode> { new("", from, decimal.One) };

            while (search.Count > 0)
            {
                var rootNode = search.Last();
                search.Remove(rootNode);

                if (rootNode == to) break;

                // Visit all adjacent nodes that are not already seen
                adjacency[rootNode].ForEach(adjacent =>
                {
                    if (paths.Contains(adjacent.To)) return;

                    search.Add(adjacent.To);
                    paths.Add(adjacent.From);
                    predecessors.Add(adjacent);
                });
            }

            return predecessors;
        }

        private static Dictionary<string, List<GraphNode>> Adjacency(IReadOnlyCollection<GraphNode> graph)
        {
            return graph
                .ToLookup(node => node.From, node => graph.Where(node1 => node.From == node1.From).ToList())
                .ToDictionary(grouping => grouping.Key, grouping => grouping.First());
        }

        private static void WriteError(string filePath, int errorLine, string message)
        {
            Console.Error.WriteLine($"Line {errorLine} of file `{filePath}` invalid:");
            Console.Error.WriteLine(message);
        }
    }
}