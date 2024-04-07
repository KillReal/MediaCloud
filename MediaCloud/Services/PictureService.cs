using MediaCloud.WebApp.Services;
using MediaCloud.WebApp.Services.ConfigurationProvider;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace MediaCloud.Services
{
    public class PictureService : IPictureService
    {
        private readonly IConfigProvider _configProvider;

        public PictureService(IConfigProvider configProvider)
        {
            _configProvider = configProvider;
        }

        public byte[] LowerResolution(Image image, byte[] sourceBytes)
        {
            var maxSize = _configProvider.EnvironmentSettings.PreviewMaxHeight;

            var size = image.Size;
            var width = maxSize;
            var height = maxSize;
            var div = new float[] 
            { 
                size.Width / (float)width, 
                size.Height / (float)height 
            };
            var maxDiv = Math.Max(div[0], div[1]);

            if (maxDiv > 1.0)
            {
                var newWidth = Convert.ToInt32(size.Width / maxDiv);
                var newHeight = Convert.ToInt32(size.Height / maxDiv);
                var targetSize = new Size(newWidth, newHeight);
                image.Mutate(x => x.Resize(targetSize, KnownResamplers.Lanczos3, true));

                return ConvertToBytes(image);
            }

            return sourceBytes;
        }

        public byte[] LowerResolution(byte[] pictureBytes)
        {
            return LowerResolution(ConvertToImage(pictureBytes), pictureBytes);
        }

        public byte[] Rotate(byte[] pictureBytes, int rotationgDegrees)
        {
            var image = ConvertToImage(pictureBytes);
            image.Mutate(x => x.Rotate(rotationgDegrees));

            return ConvertToBytes(image);
        }

        public byte[] ChangeBrightnessImage(byte[] pictureBytes, float amount)
        {
            var image = ConvertToImage(pictureBytes);
            image.Mutate(x => x.Brightness(amount));

            return ConvertToBytes(image);
        }

        private static Image ConvertToImage(byte[] pictureBytes)
        {
            var stream = new MemoryStream(pictureBytes);
            return Image.Load(stream);
        }

        private static byte[] ConvertToBytes(Image image)
        {
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
    }
}
