using System.Text.RegularExpressions;
using MediaCloud.Data.Models;
using MediaCloud.Extensions;
using MediaCloud.Services;
using SixLabors.ImageSharp;

namespace MediaCloud.WebApp.Builders.BlobModel
{
    public class BlobModelBuilder(IPictureService pictureService)
    {
        private readonly IPictureService _pictureService = pictureService;

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
                    var convertedImage = Image.Load(file.Content);
                    blob = new(file.Content, convertedImage.Width, convertedImage.Height);
                    file.Content = _pictureService.LowerResolution(convertedImage, blob.Content);
                    break;
                case "xlsx":
                case "xls":
                    blob = new(file.Content);
                    file.Content = File.ReadAllBytes("wwwroot/img/types/excel.png");
                    break;
                default:
                    blob = new(file.Content);
                    if (File.Exists($"wwwroot/img/types/{extension}.png"))
                    {
                        File.Exists($"wwwroot/img/types/{extension}.png");
                        file.Content = File.ReadAllBytes($"wwwroot/img/types/{extension}.png");
                    }
                    else 
                    {
                        file.Content = File.ReadAllBytes("wwwroot/img/types/file.png");
                    }
                    break;
            }

            return new(blob, new Preview(blob, file));
        }
    }
}
