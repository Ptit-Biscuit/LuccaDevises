using System;

namespace LuccaDevises.Exceptions
{
    public class GraphException : Exception
    {
        public readonly string FilePath;
        public readonly int ErrorLine;

        public GraphException(string filePath, int errorLine, string message) : base(message)
        {
            FilePath = filePath;
            ErrorLine = errorLine;
        }
    }
}