using MediaCloud.WebApp.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace MediaCloud.Services
{
    public class PictureService
    {
        public static byte[] LowerResolution(Image image, byte[] sourceBytes)
        {
            var maxSize = ConfigurationService.Preview.GetMaxHeight();

            var size = image.Size;
            var width = maxSize;
            var height = maxSize;
            var div = new float[] { size.Width / (float)width, size.Height / (float)height };
            var maxDiv = Math.Max(div[0], div[1]);

            if (maxDiv > 1.0)
            {
                var targetSize = new Size(Convert.ToInt32(size.Width / maxDiv), Convert.ToInt32(size.Height / maxDiv));
                image.Mutate(x => x.Resize(targetSize, KnownResamplers.Lanczos3, true)) ;

                var ms = new MemoryStream();

                var encoder = image.Metadata.DecodedImageFormat;
                if (encoder != null)
                {
                    image.Save(ms, encoder);
                }
                else
                {
                    image.Save(ms, new JpegEncoder());
                }

                return ms.ToArray();
            }

            return sourceBytes;
        }

        public static byte[] LowerResolution(byte[] pictureBytes)
        {
            var stream = new MemoryStream(pictureBytes);
            var image = Image.Load(stream);

            return LowerResolution(image, pictureBytes);
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
