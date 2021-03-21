using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace LuccaDevises
{
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
            List<Tuple<string, string, decimal>> graph;

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
                graph = new List<Tuple<string, string, decimal>>(currenciesLines * 2);

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
                        
                        // TODO: Error if exchange rate is less than 0
                        
                        // Add nodes to graph
                        graph.Add(new Tuple<string, string, decimal>(line[0], line[1],
                            decimal.Round(exchangeRate, 4)));
                        graph.Add(new Tuple<string, string, decimal>(line[1], line[0],
                            decimal.Round(1 / exchangeRate, 4)));
                    }
                    catch (FormatException fe)
                    {
                        WriteError(filePath, lineIndex, fe.Message);
                        return -1;
                    }

                    lineIndex++;
                }
            }

            // TODO: Error if given number of exchange rate does not equals last lines of given file

            // Adjacency list
            var adjacency = new Dictionary<string, List<KeyValuePair<string, decimal>>>();

            graph.ForEach(tuple =>
                adjacency[tuple.Item1] =
                    graph.Where(tuple1 => tuple.Item1 == tuple1.Item1)
                        .Select(tuple2 => new KeyValuePair<string, decimal>(tuple2.Item2, tuple2.Item3))
                        .ToList()
            );
            
            // TODO: Error if start and target currencies are not in adjacency list

            // Breadth-first-search algorithm
            var search = new List<string> { start.Item1 };
            var paths = new List<string> { start.Item1 };
            
            // Useful to transform to directed graph
            var predecessors = new List<KeyValuePair<string, string>> { new("", start.Item1) };

            while (search.Count > 0)
            {
                var rootNode = search.Last();
                search.Remove(rootNode);

                if (rootNode == start.Item3) break;

                // Visit all adjacent nodes that are not already seen
                adjacency[rootNode].ForEach(pair =>
                {
                    if (paths.Contains(pair.Key)) return;

                    search.Add(pair.Key);
                    paths.Add(pair.Key);
                    predecessors.Add(new KeyValuePair<string, string>(rootNode, pair.Key));
                });
            }
            
            // TODO: Error if target currency not in paths

            var tmp = start.Item3;
            var shortestPath = new List<string> { start.Item3 };

            while (tmp != "")
            {
                tmp = predecessors.First(pair => pair.Value == tmp).Key;
                shortestPath.Add(tmp);
            }

            shortestPath.Reverse();

            Console.Write(decimal.Ceiling(start.Item2 * shortestPath
                .Zip(shortestPath.GetRange(1, shortestPath.Count - 1),
                    (s, s1) => new KeyValuePair<string, string>(s, s1))
                .Select(pair =>
                    graph.FirstOrDefault(tuple => tuple.Item1 == pair.Key && tuple.Item2 == pair.Value)?.Item3)
                .Where(arg => arg != null)
                .Aggregate(decimal.One, (arg1, arg2) => arg1 * arg2.Value)));

            return 0;
        }

        private static void WriteError(string filePath, int errorLine, string message)
        {
            Console.Error.WriteLine($"Line {errorLine} of file `{filePath}` invalid:");
            Console.Error.WriteLine(message);
        }
    }
}