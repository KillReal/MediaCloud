using MediaCloud.Data.Models;
using MediaCloud.Services;
using MediaCloud.WebApp.Services.ConfigProvider;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

namespace MediaCloud.WebApp.Builders.BlobModel
{
    public class BlobModelBuilder(IPictureService pictureService, IConfigProvider configProvider)
    {
        private readonly WebpEncoderFactory _webpEncoderFactory = new WebpEncoderFactory(configProvider);
        
        public FileModel Build(UploadedFile file)
        {
            Blob blob;
            var extension = file.Name.Split('.').Last();
            var fileSize = file.Content.Length;

            switch (extension)
            {
                case "jpeg":
                case "jpg":
                case "png":
                case "gif":
                case "tiff":
                case "webp":
                    var image = Image.Load(file.Content);
                    
                    if (extension != "webp" && file.KeepOriginalFormat == false)
                    {
                        var stream = new MemoryStream();
                        image.SaveAsWebp(stream, _webpEncoderFactory.GetBestEncoder(fileSize));
                            
                        image = Image.Load(stream.ToArray());
                        file.Type = "image/webp";
                        file.Name = file.Name.Split('.').First() + ".webp";
                        file.Content = stream.ToArray();
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
