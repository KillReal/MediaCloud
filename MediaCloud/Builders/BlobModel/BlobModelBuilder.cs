using MediaCloud.Data.Models;
using MediaCloud.Services;
using MediaCloud.WebApp.Services.ConfigProvider;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

namespace MediaCloud.WebApp.Builders.BlobModel
{
    public class BlobModelBuilder(IPictureService pictureService, IConfigProvider configProvider)
    {
        private readonly long _lowQualitySizeLimit = configProvider.EnvironmentSettings.SmallImageSizeLimitKb * 1024;
        private readonly long _defaultQualitySizeLimit = configProvider.EnvironmentSettings.ImageSizeLimitKb * 1024; 

        private readonly WebpEncoder _defaultWebpEncoder = new() 
        {
            Quality = configProvider.EnvironmentSettings.ImageProcessingQuality,
            Method = (WebpEncodingMethod)configProvider.EnvironmentSettings.ImageProcessingLevel,
            
        };
        
        private readonly WebpEncoder _smallImageWebpEncoder = new()
        {
            Quality = configProvider.EnvironmentSettings.SmallImageProcessingQuality,
            Method = (WebpEncodingMethod)configProvider.EnvironmentSettings.SmallImageProcessingLevel,
        };
        
        private readonly WebpEncoder _largeImageWebpEncoder = new()
        {
            Quality = configProvider.EnvironmentSettings.LargeImageProcessingQuality,
            Method = (WebpEncodingMethod)configProvider.EnvironmentSettings.LargeImageProcessingLevel,
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

                    if (extension != "webp" && file.KeepOriginalFormat == false)
                    {
                        var stream = new MemoryStream();
                            
                        if (file.Content.Length < _lowQualitySizeLimit)
                        {
                            image.SaveAsWebp(stream, _smallImageWebpEncoder);
                        }
                        else if (file.Content.Length < _defaultQualitySizeLimit)
                        {
                            image.SaveAsWebp(stream, _defaultWebpEncoder);
                        }
                        else
                        {
                            image.SaveAsWebp(stream, _largeImageWebpEncoder);
                        }
                            
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
