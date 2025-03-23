using MediaCloud.WebApp.Services.ConfigProvider;
using SixLabors.ImageSharp.Formats.Webp;

namespace MediaCloud.WebApp.Builders.BlobModel;

public class WebpEncoderFactory(IConfigProvider configProvider)
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
    
    public WebpEncoder GetBestEncoder(int fileSize)
    {
        if (fileSize < _lowQualitySizeLimit)
        {
            return _smallImageWebpEncoder;
        }

        if (fileSize < _defaultQualitySizeLimit)
        {
            return _defaultWebpEncoder;
        }

        return _largeImageWebpEncoder;
    }
}