using System.Buffers.Text;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.Services;
using MediaCloud.WebApp.Controllers;
using MediaCloud.WebApp.Services.ActorProvider;
using MediaCloud.WebApp.Services.ConfigProvider;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NLog;
using ILogger = NLog.ILogger;

namespace MediaCloud.WebApp;

public class AutotagService : IAutotagService
{
    private readonly ILogger _logger = LogManager.GetLogger("AutotagService");
    private readonly List<Guid> _proceededPreviewIds = new();
    private readonly HttpClient _httpClient;
    private readonly Mutex _mutex = new();
    private readonly Semaphore _semaphore;

    private int _maxParralelDegree = 1;
    private double _defaultExecutionTime = 45.0;
    private double? _averageExecutionTime = null;
    private string _joyTagConnectionString;

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
            return new();
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
                return new();
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

            _logger.Info("AI tag autocompletion for Preview: {previewId} successfully executed within: {elapsedTime} sec, suggested tags: {suggestedTagsString}", 
                preview.Id, elapsedTime, suggestedTagsString);
            
            _mutex.WaitOne();
            var actualTags = tagRepository.GetRangeByAliasString(suggestedTagsString);
            _mutex.ReleaseMutex();

            var actualTagsString = string.Join(" ", actualTags.Select(x => x.Name));

            _logger.Info("Existing tags for suggestion: {actualTagsString}", actualTagsString);

            _proceededPreviewIds.Remove(preview.Id);

            return actualTags;
        }
        catch (Exception ex)
        {
            _proceededPreviewIds.Remove(preview.Id);
            _logger.Error(ex, "Failed to process autotagging for image");
            return new();
        }
    }

    public List<Tag> AutocompleteTagsForCollection(List<Preview> previews, TagRepository tagRepository)
    {
        var collectionId = previews.First().Collection?.Id;

        if (previews.Any() == false || collectionId == null)
        {
            return new();
        }

        try {
            var stopwatch = DateTime.Now;
            
            _logger.Info("Executed AI tag autocompletion for Collection: {collection.Id}", collectionId);

            List<Tag> tags = new();

            var options = new ParallelOptions { MaxDegreeOfParallelism = _maxParralelDegree};
            Parallel.ForEach(previews, options, preview => 
            {
                var stags = AutocompleteTagsForPreview(preview, tagRepository);
                var strtags = string.Join(" ", stags.Select(x => x.Name));

                tags = tags.Union(stags).ToList();
            });
            
            var elapsedTime = (DateTime.Now - stopwatch).TotalSeconds;
            var tagsString = string.Join(" ", tags.Select(x => x.Name));

            _logger.Info("AI tag autocompletion for Preview: {previewId} successfully executed within: {elapsedTime} sec, suggested tags: {suggestedTagsString}", collectionId, elapsedTime, tagsString);
            
            return tags;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to process autotagging for image");
            return new();
        }
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
                return new();
            }

            return JsonConvert.DeserializeObject<List<string>>(result) ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get tag aliases");
            return new();
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

        if (result.StatusCode != HttpStatusCode.OK)
        {
            throw new HttpRequestException("Request to JoyTag AI failed");
        }

        return result.Content.ReadAsStringAsync()
                                .GetAwaiter()
                                .GetResult();
    }
}
