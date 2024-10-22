using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.AutotagService;

namespace MediaCloud.WebApp;

public interface IAutotagService
{
    public AutotagResult AutocompleteTagsForPreview(Preview preview, TagRepository tagRepository);
    public AutotagResult AutocompleteTagsForCollection(List<Preview> previews, TagRepository tagRepository);
    public List<string> GetSuggestionsByString(string searchString, int limit = 10);
    public double GetAverageExecutionTime();
    public double GetAverageExecutionTime(int previewsCount);
    public bool IsPreviewIsProceeded(Guid previewId);
}