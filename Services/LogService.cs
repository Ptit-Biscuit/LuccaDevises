using System;

namespace LuccaDevises.Services
{
    public static class LogService
    {
        public static void WriteError(string error)
        {
            Console.Error.WriteLine(error);
        }

        public static void WriteFileError(string filePath, int errorLine, string message)
        {
            Console.Error.WriteLine($"Line {errorLine} of file `{filePath}` invalid:");
            Console.Error.WriteLine(message);
        }
    }
}