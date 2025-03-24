using MediaCloud.WebApp.Services.ConfigProvider;
using SixLabors.ImageSharp.Formats.Webp;

namespace MediaCloud.WebApp.Builders.BlobModel;

public class WebpEncoderFactory(IConfigProvider configProvider)
{
    private readonly long _lowQualitySizeLimit = configProvider.EnvironmentSettings.SmallImageSizeLimitKb * 1024;
    private readonly long _defaultQualitySizeLimit = configProvider.EnvironmentSettings.ImageSizeLimitKb * 1024; 
    
    public WebpEncoder GetBestEncoder(int fileSize)
    {
        if (fileSize < _lowQualitySizeLimit)
        {
            return new WebpEncoder
            {
                Quality = configProvider.EnvironmentSettings.SmallImageProcessingQuality,
                Method = (WebpEncodingMethod)configProvider.EnvironmentSettings.SmallImageProcessingLevel,
            };
        }

        if (fileSize < _defaultQualitySizeLimit)
        {
            return new WebpEncoder
            {
                Quality = configProvider.EnvironmentSettings.SmallImageProcessingQuality,
                Method = (WebpEncodingMethod)configProvider.EnvironmentSettings.SmallImageProcessingLevel,
            };;
        }

        return new WebpEncoder
        {
            Quality = configProvider.EnvironmentSettings.LargeImageProcessingQuality,
            Method = (WebpEncodingMethod)configProvider.EnvironmentSettings.LargeImageProcessingLevel,
        };;
    }
}