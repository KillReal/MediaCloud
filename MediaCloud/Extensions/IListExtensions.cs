namespace MediaCloud.Extensions
{
    public static class IListExtensions
    {
        /// <summary>
        /// Shuffles the element order of the specified list.
        /// </summary>
        public static IList<T> Shuffle<T>(this IList<T> ts, int seed = 0)
        {
            if (seed == 0)
            {
                seed = DateTime.Now.Millisecond;
            }

            var count = ts.Count;
            var last = count - 1;
            var random = new Random(seed);
            for (var i = 0; i < last; ++i)
            {
                var r = random.Next(i, count);
                (ts[r], ts[i]) = (ts[i], ts[r]);
            }

            return ts;
        }
    }
}
