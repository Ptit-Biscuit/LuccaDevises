using LuccaDevises;
using System;
using System.IO;
using LuccaDevises.Exceptions;
using LuccaDevises.Services;
using Xunit;

namespace Tests
{
    public class UnitTest1
    {
        [Fact]
        public void NoFilePath()
        {
            Assert.Equal(-1, Program.Main(Array.Empty<string>()));
        }

        [Fact]
        public void EmptyFilePath()
        {
            Assert.Equal(-1, Program.Main(new[] { "" }));
        }

        [Fact]
        public void NoneExistingFile()
        {
            Assert.Equal(-1, Program.Main(new[] { "DoesNotExists.toto" }));
        }

        [Theory]
        [InlineData("empty")]
        [InlineData("emptyFirstLine")]
        [InlineData("missingStartCurrency")]
        public void InvalidFileData(string path)
        {
            Assert.Equal(-1, Program.Main(new[] { Path.GetFullPath(@$"../../../Resources/{path}.txt") }));
        }

        [Theory]
        [InlineData("empty")]
        [InlineData("emptySecondLine")]
        [InlineData("missingExchangeRate")]
        public void Graph_shouldNotGenerateAValidGraphFromInvalidFile(string path)
        {
            Assert.Throws<GraphException>(() =>
                GraphService.CreateGraph(Path.GetFullPath(@$"../../../Resources/{path}.txt")));
        }

        [Fact]
        public void Graph_shouldOutputExchangeRate()
        {
            var graph = GraphService.CreateGraph(Path.GetFullPath("../../../Resources/valid.txt"));
            Assert.Equal(59033, decimal.Ceiling(550 * GraphService.ExchangeRate(graph, "EUR", "JPY")));
        }
    }
}