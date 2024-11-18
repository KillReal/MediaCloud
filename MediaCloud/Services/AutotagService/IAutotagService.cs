using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.AutotagService;

namespace MediaCloud.WebApp;

public interface IAutotagService
{
    public List<AutotagResult> AutotagPreviewRange(List<Preview> previews, TagRepository tagRepository);
    public AutotagResult AutotagPreview(Preview preview, TagRepository tagRepository);
    public List<string> GetSuggestionsByString(string searchString, int limit = 10);
    public double GetAverageExecutionTime();
    public double GetAverageExecutionTime(int previewsCount);
}