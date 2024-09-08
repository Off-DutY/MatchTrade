namespace MatchTrade.Utilities
{
    public static class ComparerUtility
    {
        public static int Comparer(int a, int b)
        {
            return a.CompareTo(b);
        }

        public static int Comparer(decimal a, decimal b)
        {
            return a.CompareTo(b);
        }

        public static int Comparer(long a, long b)
        {
            return a.CompareTo(b);
        }
    }
}