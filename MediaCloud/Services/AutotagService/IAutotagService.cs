using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.AutotagService;
using MediaCloud.WebApp.Services.ConfigProvider;

namespace MediaCloud.WebApp;

public interface IAutotagService
{
    public List<AutotagResult> AutotagPreviewRange(List<Preview> previews, TagRepository tagRepository, IConfigProvider configProvider);
    public AutotagResult AutotagPreview(Preview preview, TagRepository tagRepository, IConfigProvider configProvider);
    public List<string> GetSuggestionsByString(IConfigProvider configProvider, string searchString, int limit = 10);
    public List<string> GetAvailableModels();
    public double GetAverageExecutionTime();
    public double GetAverageExecutionTime(int previewsCount);
}