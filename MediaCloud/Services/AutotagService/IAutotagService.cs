using MediaCloud.Data.Models;
using MediaCloud.Repositories;

namespace MediaCloud.WebApp;

public interface IAutotagService
{
    public List<Tag> AutocompleteTagsForPreview(Preview preview, TagRepository tagRepository, 
        bool isParallel = false);
    public List<Tag> AutocompleteTagsForCollection(Collection collection, TagRepository tagRepository, 
        int parallelDegree = 1);
    public List<string> GetSuggestionsByString(string searchString, int limit = 10);
    public double GetAverageExecutionTime();
    public bool IsPreviewIsProceeded(Guid previewId);
}