using MediaCloud.Data.Models;
using MediaCloud.Repositories;

namespace MediaCloud.WebApp;

public interface IAutotagService
{
    public List<Tag> AutocompleteTagsForImage(Preview preview, TagRepository tagRepository);
    public List<string> GetSuggestionsByString(string searchString, int limit = 10);
    public double GetAverageExecutionTime();
}