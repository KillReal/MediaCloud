using MediaCloud.WebApp.Services;
using System.Drawing;
using System.Drawing.Imaging;

namespace MediaCloud.Services
{
    public class PictureService
    {
        private static IConfiguration Configuration;

        public static void Init(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static byte[] LowerResolutionToPreview(byte[] pictureBytes)
        {
            var maxSize = ConfigurationService.Preview.GetMaxHeight();

            var stream = new MemoryStream(pictureBytes);
            var image = new Bitmap(stream);
            var size = image.Size;
            var width = maxSize;
            var height = maxSize;
            var div = new float[] { size.Width / (float)width, size.Height / (float)height };
            var maxDiv = Math.Max(div[0], div[1]);

            if (maxDiv > 1.0)
            {
                var bitmap = new Bitmap(image, new(Convert.ToInt32(size.Width / maxDiv), Convert.ToInt32(size.Height / maxDiv)));
                var ms = new MemoryStream();
                bitmap.Save(ms, ImageFormat.Jpeg);
                pictureBytes = ms.ToArray();
            }

            return pictureBytes;
        }

        public static string FormatSize(long bytes, bool useUnit = true)
        {
            string[] Suffix = { " B", " kB", " MB", " GB", " TB" };
            double dblSByte = bytes;
            int i;
            for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }
            return $"{dblSByte:0}{(useUnit ? Suffix[i] : null)}";
        }
    }
}
