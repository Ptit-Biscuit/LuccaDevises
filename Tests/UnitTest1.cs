using LuccaDevises;
using System;
using System.IO;
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
            Assert.Equal(-1, Program.Main(new []{ "DoesNotExists.toto" }));
        }

        [Theory]
        [InlineData("empty")]
        [InlineData("emptyFirstLine")]
        [InlineData("missingStartCurrency")]
        [InlineData("emptySecondLine")]
        public void InvalidFileData(string path)
        {
            Assert.Equal(-1, Program.Main(new[] { Path.GetFullPath(@$"..\\..\\..\\Resources\\${path}.txt") }));
        }
    }
}