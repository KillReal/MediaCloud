using MediaCloud.Data.Models;
using MediaCloud.Repositories;

namespace MediaCloud.WebApp;

public interface IAutotagService
{
    public List<Tag> AutocompleteTagsForPreview(Preview preview, TagRepository tagRepository);
    public List<string> GetSuggestionsByString(string searchString, int limit = 10);
    public double GetAverageExecutionTime();
    public bool IsPreviewIsProceeded(Guid previewId);
}