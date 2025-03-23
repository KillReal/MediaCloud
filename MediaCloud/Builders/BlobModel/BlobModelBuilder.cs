using MediaCloud.Data.Models;
using MediaCloud.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

namespace MediaCloud.WebApp.Builders.BlobModel
{
    public class BlobModelBuilder(IPictureService pictureService)
    {
        private readonly WebpEncoder _webpEncoder = new() 
        {
            Quality = 75,
            Method = WebpEncodingMethod.Level5,
            
        };

        private const long _lowQualitySize = 512_000; 

        private readonly WebpEncoder _highQualityWebpEncoder = new()
        {
            Quality = 90,
            Method = WebpEncodingMethod.Level5
        };

        public FileModel Build(UploadedFile file)
        {
            Blob blob;
            var extension = file.Name.Split('.').Last();

            switch (extension)
            {
                case "jpeg":
                case "jpg":
                case "png":
                case "gif":
                case "tiff":
                case "webp":
                    var image = Image.Load(file.Content);

                    if (extension != "webp")
                    {
                        var stream = new MemoryStream();

                        if (file.Content.Length < _lowQualitySize)
                        {
                            image.SaveAsWebp(stream, _highQualityWebpEncoder);
                        }
                        else
                        {
                            image.SaveAsWebp(stream, _webpEncoder);
                        }

                        if (file.KeepOriginalFormat == false)
                        {
                            image = Image.Load(stream.ToArray());
                            file.Type = "image/webp";
                            file.Name = file.Name.Split('.').First() + ".webp";
                            file.Content = stream.ToArray();
                        }
                    }

                    blob = new Blob(file.Content, image.Width, image.Height);
                    file.Content = pictureService.LowerResolution(image, blob.Content);
                    break;
                default:
                    blob = new Blob(file.Content);
                    file.Content = [];
                    break;
            }

            return new FileModel(blob, new Preview(blob, file));
        }
    }
}
