namespace MediaCloud.Extensions
{
    public static class IFormFileExtensions
    {
        public static byte[] GetBytes(this IFormFile formFile)
        {
            var memoryStream = new MemoryStream();
            formFile.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        public static MemoryStream GetStream(this IFormFile formFile)
        {
            var memoryStream = new MemoryStream();
            formFile.CopyTo(memoryStream);
            return memoryStream;
        }

        public static async Task<byte[]> GetBytesAsync(this IFormFile formFile)
        {
            var memoryStream = new MemoryStream();
            await formFile.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
