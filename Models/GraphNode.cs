namespace LuccaDevises.Models
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
}