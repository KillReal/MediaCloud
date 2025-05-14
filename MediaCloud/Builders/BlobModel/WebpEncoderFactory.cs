using MediaCloud.WebApp.Services.ConfigProvider;
using SixLabors.ImageSharp.Formats.Webp;

namespace MediaCloud.WebApp.Builders.BlobModel;

public class WebpEncoderFactory(IConfigProvider configProvider)
{
    private readonly long _lowQualitySizeLimit = configProvider.EnvironmentSettings.SmallImageSizeLimitKb * 1024;
    private readonly long _defaultQualitySizeLimit = configProvider.EnvironmentSettings.ImageSizeLimitKb * 1024; 
    
    
    private readonly Lazy<WebpEncoder> _smallFileWebpEncoder = new Lazy<WebpEncoder>(() => new WebpEncoder
    {
        Quality = configProvider.EnvironmentSettings.SmallImageProcessingQuality,
        Method = (WebpEncodingMethod)configProvider.EnvironmentSettings.SmallImageProcessingLevel,
    });
    
    private readonly Lazy<WebpEncoder> _defaultFileWebpEncoder = new Lazy<WebpEncoder>(() => new WebpEncoder
    {
        Quality = configProvider.EnvironmentSettings.ImageProcessingQuality,
        Method = (WebpEncodingMethod)configProvider.EnvironmentSettings.ImageProcessingLevel
    });
    
    private readonly Lazy<WebpEncoder> _largeFileWebpEncoder = new Lazy<WebpEncoder>(() => new WebpEncoder
    {
        Quality = configProvider.EnvironmentSettings.LargeImageProcessingQuality,
        Method = (WebpEncodingMethod)configProvider.EnvironmentSettings.LargeImageProcessingLevel
    });
    
    public WebpEncoder GetBestEncoder(int fileSize)
    {
        if (fileSize < _lowQualitySizeLimit)
        {
            return _smallFileWebpEncoder.Value;
        }

        if (fileSize < _defaultQualitySizeLimit)
        {
            return _defaultFileWebpEncoder.Value;
        }

        return _largeFileWebpEncoder.Value;
    }
}