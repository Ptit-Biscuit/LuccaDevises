using System;
using System.Collections.Generic;
using System.IO;
using LuccaDevises.Exceptions;
using LuccaDevises.Models;
using LuccaDevises.Services;

namespace LuccaDevises
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                LogService.WriteError("A file path is required as sole argument"); // Critical failure
                return -1;
            }

            var filePath = args[0];

            if (!File.Exists(filePath))
            {
                LogService.WriteError($"File path `{filePath}` does not exists"); // Critical failure
                return -1;
            }

            Tuple<string, int, string> start;

            using (var sr = new StreamReader(filePath))
            {
                // Process first line
                var firstLine = sr.ReadLine()?.Split(';');

                if (firstLine == null || firstLine.Length < 3)
                {
                    // Critical failure: nothing to process
                    LogService.WriteFileError(filePath, 0,
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
                    LogService.WriteFileError(filePath, 0, fe.Message);
                    return -1;
                }
            }

            var graph = new List<GraphNode>();

            try
            {
                graph = GraphService.CreateGraph(filePath);
            }
            catch (GraphException ge)
            {
                LogService.WriteFileError(ge.FilePath, ge.ErrorLine, ge.Message);
            }

            Console.WriteLine(
                decimal.Ceiling(start.Item2 * GraphService.ExchangeRate(graph, start.Item1, start.Item3)));

            return 0;
        }
    }
}