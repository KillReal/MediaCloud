using SixLabors.ImageSharp;

namespace MediaCloud.Services
{
    public interface IPictureService
    {
        public byte[] LowerResolution(Image image, byte[] sourceBytes);
        public byte[] LowerResolution(byte[] pictureBytes);
        public byte[] Rotate(byte[] pictureBytes, int rotationgDegrees);
        public byte[] ChangeBrightnessImage(byte[] pictureBytes, float amount);
        public bool SaveImageToPath(byte[] pictureBytes, string path);
    }
}
