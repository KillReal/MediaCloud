using System.Net;
using System.Net.Http.Headers;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.ConfigProvider;
using Newtonsoft.Json;
using NLog;
using Logger = NLog.ILogger;

namespace MediaCloud.WebApp;

public class AutotagService : IAutotagService
{
    private readonly Logger _logger = LogManager.GetLogger("AutotagService");
    private readonly List<Guid> _proceededPreviewIds = [];
    private readonly HttpClient _httpClient;
    private readonly Mutex _mutex = new();
    private readonly Semaphore _semaphore;

    private readonly int _maxParralelDegree = 1;
    private const double _defaultExecutionTime = 45.0;
    private double? _averageExecutionTime = null;
    private readonly string _joyTagConnectionString;

    public double GetAverageExecutionTime() => _averageExecutionTime ?? _defaultExecutionTime;

    public double GetAverageExecutionTime(int previewsCount) 
    {
        var batchCount = (int)Math.Ceiling(((double)previewsCount) / _maxParralelDegree);
        var approximateTime = _averageExecutionTime * batchCount;
        
        if (previewsCount > 4)
        {
            approximateTime *= 1.2;
        }

        return approximateTime ?? _defaultExecutionTime * batchCount;
    }

    public AutotagService(IConfigProvider configProvider)
    {
        _joyTagConnectionString = configProvider.EnvironmentSettings.AiJoyTagConnectionString ?? 
            throw new Exception("AI JoyTag Connection String is not set");

        var parralelDegree = configProvider.EnvironmentSettings.TaskSchedulerAutotaggingWorkerCount;
        _semaphore = new(parralelDegree, parralelDegree);
        _maxParralelDegree = parralelDegree;

        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(_defaultExecutionTime * 2)
        };
    }

    public List<Tag> AutocompleteTagsForPreview(Preview preview, TagRepository tagRepository)
    {
        if (preview == null)
        {
            return [];
        }
        
        try {
            var stopwatch = DateTime.Now;
            _proceededPreviewIds.Add(preview.Id);

            _logger.Info("Executed AI tag autocompletion for Preview: {previewId}", preview.Id);

            object data = new
            {
                image = preview.Content
            };

            _semaphore.WaitOne();
            var result = Post("predictTags", data);
            _semaphore.Release();

            if (string.IsNullOrWhiteSpace(result))
            {
                return [];
            }

            var suggestedTags = result.Split("\n")
                .Take(100)
                .Select(x => x.Split(":")[0])
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
                
            var suggestedTagsString = string.Join(" ", suggestedTags);

            var elapsedTime = (DateTime.Now - stopwatch).TotalSeconds;

            if (_averageExecutionTime == null) 
            {
                _averageExecutionTime = elapsedTime;
            }
            else 
            {
                _averageExecutionTime = (_averageExecutionTime + elapsedTime) / 2;
            }

            _logger.Debug("AI tag autocompletion for Preview: {previewId} successfully executed within: {elapsedTime} sec, suggested tags: {suggestedTagsString}", 
                preview.Id, elapsedTime.ToString("N0"), suggestedTagsString);
            
            _mutex.WaitOne();
            var actualTags = tagRepository.GetRangeByAliasString(suggestedTagsString);
            _mutex.ReleaseMutex();

            var actualTagsString = string.Join(" ", actualTags.Select(x => x.Name));

            _logger.Debug("Existing tags for suggestion: {actualTagsString}", actualTagsString);

            _proceededPreviewIds.Remove(preview.Id);

            return actualTags;
        }
        catch (Exception ex)
        {
            _proceededPreviewIds.Remove(preview.Id);
            _logger.Error(ex, "Failed to process autotagging for image");
            return [];
        }
    }

    public List<Tag> AutocompleteTagsForCollection(List<Preview> previews, TagRepository tagRepository)
    {
        var collectionId = previews.First().Collection?.Id;

        if (previews.Count == 0 || collectionId == null)
        {
            return [];
        }

        var stopwatch = DateTime.Now;
        List<Tag> tags = [];
            
        _logger.Info("Executed AI tag autocompletion for Collection: {collection.Id}", collectionId);

        var options = new ParallelOptions { MaxDegreeOfParallelism = _maxParralelDegree};
        Parallel.ForEach(previews, options, preview => 
        {
            var tags = AutocompleteTagsForPreview(preview, tagRepository);
            tags = tags.Union(tags).ToList();
        });
        
        var elapsedTime = (DateTime.Now - stopwatch).TotalSeconds;
        var tagsString = string.Join(" ", tags.Select(x => x.Name));

        _logger.Debug("AI tag autocompletion for Preview: {previewId} successfully executed within: {elapsedTime} sec, suggested tags: {suggestedTagsString}", collectionId, elapsedTime.ToString("N0"), tagsString);
        
        return tags;
    }

    public List<string> GetSuggestionsByString(string searchString, int limit = 10)
    {
        try 
        {
            object data = new
            {
                searchString,
                limit
            };

            var result = Post("suggestedTags", data);

            if (string.IsNullOrWhiteSpace(result))
            {
                return [];
            }

            return JsonConvert.DeserializeObject<List<string>>(result) ?? [];
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get tag aliases");
            return [];
        }
    }

    public bool IsPreviewIsProceeded(Guid previewId)
    {
        return _proceededPreviewIds.Exists(x => x == previewId);
    }

    private string Post(string method, object data)
    {
        var content = JsonConvert.SerializeObject(data);
        var buffer = System.Text.Encoding.UTF8.GetBytes(content);
        var contentBytes = new ByteArrayContent(buffer);

        contentBytes.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var response = _httpClient.PostAsync(_joyTagConnectionString + "/" + method, contentBytes);
        var result = response.GetAwaiter().GetResult();

        if (response.IsFaulted || result.StatusCode != HttpStatusCode.OK)
        {
            throw new HttpRequestException($"Request to JoyTag AI failed with code {result.StatusCode}");
        }

        return result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
    }
}
