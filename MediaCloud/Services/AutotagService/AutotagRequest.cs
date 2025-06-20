using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.ConfigProvider;

namespace MediaCloud.WebApp.Services.AutotagService;

public class AutotagRequest
{
    public readonly string Model;
    public readonly double Confidence;
    public readonly bool MCutEnabled;

    public AutotagRequest(string model, double confidence, bool mCutEnabled)
    {
        Model = model;
        Confidence = confidence;
        MCutEnabled = mCutEnabled;
    }

    public AutotagRequest(IConfigProvider configProvider)
    {
        Model = (string.IsNullOrWhiteSpace(configProvider.UserSettings.AutotaggingAiModel)
            ? configProvider.EnvironmentSettings.AutotaggingAiModel
            : configProvider.UserSettings.AutotaggingAiModel
            ) ?? throw new ArgumentException("Need to specify AI model in user or environment settings before run the task.");
        
        Confidence = configProvider.UserSettings.AutotaggingAiModelConfidence == 0.0
            ? configProvider.EnvironmentSettings.AutotaggingAiModelConfidence
            : configProvider.UserSettings.AutotaggingAiModelConfidence;
        
        MCutEnabled = configProvider.UserSettings.AutotaggingMCutThresholdEnabled;
    }
}