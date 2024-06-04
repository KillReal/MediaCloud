namespace MediaCloud.WebApp;

public interface IAutotagService
{
    public List<string> AutocompleteTagsForImage(Guid mediaId);
    public List<string> GetSuggestionsByString(string searchString, int limit = 10);
}