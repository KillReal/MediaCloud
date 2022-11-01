using System.Drawing;
using System.Drawing.Imaging;

namespace MediaCloud.Services
{
    public class PictureService : IPictureService
    {
        private static IConfiguration _configuration;

        public PictureService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static void PictureServicelazyInit(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static byte[] LowerResolution(byte[] pictureBytes)
        {
            var maxSize = Convert.ToInt32(_configuration["MaxPreviewHeight"]);

            Stream stream = new MemoryStream(pictureBytes);
            Image image = new Bitmap(stream);
            Size size = image.Size;
            int width = maxSize;
            int height = maxSize;
            float[] div = new float[] { size.Width / (float)width, size.Height / (float)height };
            float maxDiv = Math.Max(div[0], div[1]);

            if (maxDiv > 1.0)
            {
                Bitmap bitmap = new Bitmap(image, new Size(Convert.ToInt32(size.Width / maxDiv), Convert.ToInt32(size.Height / maxDiv)));
                MemoryStream ms = new MemoryStream();
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
