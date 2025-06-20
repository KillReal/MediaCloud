using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.AutotagService;
using MediaCloud.WebApp.Services.ConfigProvider;

namespace MediaCloud.WebApp;

public interface IAutotagService
{
    public List<AutotagResult> AutotagPreviewRange(List<Preview> previews, AutotagRequest request);
    public AutotagResult AutotagPreview(Preview preview, AutotagRequest request);
    public List<string> GetSuggestionsByString(string model, string searchString, int limit = 10);
    public List<string> GetAvailableModels();
    public double GetAverageExecutionTime();
    public double GetAverageExecutionTime(int previewsCount);
}