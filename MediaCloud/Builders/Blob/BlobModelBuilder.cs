using MediaCloud.Data.Models;
using MediaCloud.Extensions;
using MediaCloud.Services;
using SixLabors.ImageSharp;

namespace MediaCloud.WebApp.Builders.Blob
{
    public class BlobModelBuilder(IPictureService pictureService)
    {
        private readonly IPictureService _pictureService = pictureService;

        public FileModel Build(UploadedFile file)
        {
            MediaCloud.Data.Models.Blob blob;

            switch (file.Type)
            {
                case "image/jpeg":
                case "image/png":
                case "image/gif":
                case "image/tiff":
                case "image/webp":
                case "image/jpg":
                    var convertedImage = Image.Load(file.Content);

                    blob = new MediaCloud.Data.Models.Blob(file.Content, convertedImage.Width, convertedImage.Height);
                    file.Content = _pictureService.LowerResolution(convertedImage, blob.Content);
                    return new(blob, new Preview(blob, file));
                case "text/plain":
                    blob = new MediaCloud.Data.Models.Blob(file.Content);
                    file.Content = File.ReadAllBytes("wwwroot/img/types/text.png");
                    return new(blob, new Preview(blob, file));
                default:
                    blob = new MediaCloud.Data.Models.Blob(file.Content);
                    file.Content = File.ReadAllBytes("wwwroot/img/types/file.png");
                    return new(blob, new Preview(blob, file));
            }
        }
    }
}
