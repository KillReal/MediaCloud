namespace MediaCloud.Extensions
{
    public static class IEnumerableExtensions
    {
        public static T[] Shuffle<T>(this IEnumerable<T> source, int seed = 0)
        {
            var result = source.ToArray();
            if (result.Length < 2) return result;
            var random = new Random(seed);
            for (int i = result.Length - 1; i > 0; i--)
            {
                int pos = random.Next(i + 1);
                if (pos != i) (result[i], result[pos]) = (result[pos], result[i]);
               
            }
            return result;
        }
    }
}
