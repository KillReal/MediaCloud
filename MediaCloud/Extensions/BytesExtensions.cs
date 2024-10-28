namespace MediaCloud.Extensions
{
    public static class LongExtensions
    {
         public static string FormatSize(this long bytes, bool useUnit = true, int precision = 1)
        {
            string[] Suffix = [" B", " kB", " MB", " GB", " TB"];

            var sign = bytes < 0 
                ? "-" 
                : "";

            double dblSByte = Math.Abs(bytes);
            int i;
            for (i = 0; i < Suffix.Length && dblSByte >= 1024; i++)
            {
                dblSByte /= 1024.0;
            }
            return $"{sign}{dblSByte.ToString($"n{precision}")}{(useUnit ? Suffix[i] : null)}";
        }
    }
}