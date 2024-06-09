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
using MediaCloud.WebApp.Services.ConfigurationProvider;
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
    private static readonly HttpClient _httpClient = new();
    private Mutex _mutex = new();

    private double _averageExecutionTime = 0;
    private string _joyTagConnectionString;

    public double GetAverageExecutionTime() => _averageExecutionTime;

    public AutotagService(IConfigProvider configProvider)
    {
        _joyTagConnectionString = configProvider.EnvironmentSettings.AiJoyTagConnectionString ?? 
            throw new Exception("AI JoyTag Connection String is not set");
    }

    public List<Tag> AutocompleteTagsForPreview(Preview preview, TagRepository tagRepository, 
        bool isParallel = false)
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

            var result = Post("predictTags", data);

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

            if (_averageExecutionTime <= 0.0) 
            {
                _averageExecutionTime = elapsedTime;
            }
            else 
            {
                _averageExecutionTime = (_averageExecutionTime + elapsedTime) / 2;
            }

            _logger.Info("AI tag autocompletion for Preview: {previewId} successfully executed within: {elapsedTime} sec, suggested tags: {suggestedTagsString}", 
                preview.Id, elapsedTime, suggestedTagsString);
            
            if (isParallel)
            {
                _mutex.WaitOne();
            }

            var actualTags = tagRepository.GetRangeByAliasString(suggestedTagsString);

            if (isParallel)
            {
                _mutex.ReleaseMutex();
            }

            var actualTagsString = string.Join(" ", actualTags.Select(x => x.Name));

            _logger.Info("Existing tags for suggestion: {actualTagsString}", actualTagsString);

            _proceededPreviewIds.Remove(preview.Id);

            return actualTags;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to process autotagging for image");
            return new();
        }
    }

    public List<Tag> AutocompleteTagsForCollection(Collection collection, TagRepository tagRepository, 
        int parallelDegree = 1)
    {
        if (collection == null || collection.Previews.Any() == false)
        {
            return new();
        }

        try {
            var stopwatch = DateTime.Now;

            _logger.Info("Executed AI tag autocompletion for Collection: {collection.Id}", collection.Id);

            List<Tag> tags = new();

            var options = new ParallelOptions { MaxDegreeOfParallelism = parallelDegree };
            Parallel.ForEach(collection.Previews, options, preview => 
            {
                tags = tags.Union(AutocompleteTagsForPreview(preview, tagRepository, parallelDegree > 1))
                            .ToList();
            });
            
            var elapsedTime = (DateTime.Now - stopwatch).TotalSeconds;
            var tagsString = string.Join(" ", tags.Select(x => x.Name));

            _logger.Info("AI tag autocompletion for Preview: {previewId} successfully executed within: {elapsedTime} sec, suggested tags: {suggestedTagsString}", 
                collection.Id, elapsedTime, tagsString);
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

            return result.Split("\n").ToList();
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
            return string.Empty;
        }

        return result.Content.ReadAsStringAsync()
                                .GetAwaiter()
                                .GetResult();
    }
}
