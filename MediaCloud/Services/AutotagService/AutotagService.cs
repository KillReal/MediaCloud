using System.Net;
using System.Net.Http.Headers;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.AutotagService;
using MediaCloud.WebApp.Services.ConfigProvider;
using Microsoft.Extensions.Caching.Memory;
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
    private IMemoryCache _memoryCache;
    private MemoryCacheEntryOptions _memoryCacheOptions;

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

    public AutotagService(IConfigProvider configProvider, IMemoryCache memoryCache)
    {
        _joyTagConnectionString = configProvider.EnvironmentSettings.AiJoyTagConnectionString ?? 
            throw new Exception("AI JoyTag Connection String is not set");

        var parralelDegree = configProvider.EnvironmentSettings.AutotaggingMaxParallelDegree;
        _semaphore = new(parralelDegree, parralelDegree);
        _maxParralelDegree = parralelDegree;
        _memoryCache = memoryCache;
        _memoryCacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));

        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(_defaultExecutionTime * 4)
        };
    }

    public AutotagResult AutocompleteTagsForPreview(Preview preview, TagRepository tagRepository)
    {
        if (preview == null)
        {
            return new() 
            {
                PreviewId = Guid.Empty,
                Tags = [],
                IsSuccess = true
            };
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
                return new() 
                {
                    PreviewId = preview.Id,
                    Tags = [],
                    IsSuccess = false,
                    ErrorMessage = "Autotagging service return empty result"
                };
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

            return new() 
            {
                PreviewId = preview.Id,
                Tags = actualTags,
                IsSuccess = true
            };
        }
        catch (Exception ex)
        {
            _proceededPreviewIds.Remove(preview.Id);
            _logger.Error(ex, "Failed to process autotagging for image");
            
            return new() 
            {
                PreviewId = preview.Id,
                Tags = [],
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public List<AutotagResult> AutocompleteTagsForCollection(List<Preview> previews, TagRepository tagRepository)
    {
        var collectionId = previews.First().Collection?.Id;

        if (previews.Count == 0 || collectionId == null)
        {
            return [];
        }

        var stopwatch = DateTime.Now;
            
        _logger.Info("Executed AI tag autocompletion for Collection: {collection.Id}", collectionId);

        var options = new ParallelOptions { MaxDegreeOfParallelism = _maxParralelDegree};

        var autotagResult = new List<AutotagResult>(); 

        Parallel.ForEach(previews, options, preview => 
        {
            autotagResult.Add(AutocompleteTagsForPreview(preview, tagRepository));
        });
        
        if (autotagResult.Any(x => x.IsSuccess))
        {
            var elapsedTime = (DateTime.Now - stopwatch).TotalSeconds;

            _logger.Debug("AI tag autocompletion for Collection: {collectionId} successfully executed within: {elapsedTime} sec", collectionId, elapsedTime.ToString("N0"));
            
            return autotagResult;
        }

        _logger.Error("AI tag autocompletion for Collection: {collectionId} failed to execute due to non of previews processed", collectionId);
        
        return autotagResult;
    }

    public List<string> GetSuggestionsByString(string searchString, int limit = 10)
    {
        if (_memoryCache.TryGetValue("tagSuggestions", out List<string>? aliases))
        {
            return aliases?.Where(x => x.StartsWith(searchString)).Take(limit).ToList() ?? [];
        }

        try 
        {
            object data = new
            {
                searchString = "",
                limit = -1
            };

            var result = Post("suggestedTags", data);

            if (string.IsNullOrWhiteSpace(result))
            {
                return [];
            }

            aliases = JsonConvert.DeserializeObject<List<string>>(result);
            _memoryCache.Set("tagSuggestions", aliases, _memoryCacheOptions);

            aliases = aliases?.Where(x => x.StartsWith(searchString)).Take(limit).ToList();

            return aliases ?? [];
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
