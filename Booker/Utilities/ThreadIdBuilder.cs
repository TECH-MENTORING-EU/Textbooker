namespace Booker.Utilities
{
    public static class ThreadIdBuilder
    {
        public static string Create(int u1, int u2)
        {
            int a = Math.Min(u1, u2);
            int b = Math.Max(u1, u2);
            return $"{a}-{b}";
        }
    }
}
