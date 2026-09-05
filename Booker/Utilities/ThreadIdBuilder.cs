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

        /// <summary>
        /// Deterministic id for a conversation about a specific listing.
        /// Distinct namespace from plain user-to-user ids ("item-{item}-{a}-{b}")
        /// so an item thread can never collide with a legacy bare thread.
        /// </summary>
        public static string CreateForItem(int requesterId, int sellerId, int itemId)
        {
            int a = Math.Min(requesterId, sellerId);
            int b = Math.Max(requesterId, sellerId);
            return $"item-{itemId}-{a}-{b}";
        }
    }
}
