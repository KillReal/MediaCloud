using MediaCloud.WebApp.Services;
using MediaCloud.WebApp.Services.ConfigurationProvider;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace MediaCloud.Services
{
    public interface IPictureService
    {
        public byte[] LowerResolution(Image image, byte[] sourceBytes);
        public byte[] LowerResolution(byte[] pictureBytes);
        public byte[] RotateImage(byte[] pictureBytes, int rotationgDegrees);
        public byte[] ChangeBrightnessImage(byte[] pictureBytes, float amount);
    }
}
