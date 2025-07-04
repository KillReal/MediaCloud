﻿using System.Net;
using System.Net.Http.Headers;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Data.Types;
using MediaCloud.WebApp.Services.ConfigProvider;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using NLog;

namespace MediaCloud.WebApp.Services.AutotagService;

public class AutotagService : IAutotagService
{
    private readonly Logger _logger = LogManager.GetLogger("AutotagService");
    private readonly HttpClient _httpClient;
    private readonly Mutex _mutex = new();
    private readonly Semaphore _semaphore;
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions _memoryCacheOptions;

    private const int _maxParallelDegree = 1;
    private double? _averageExecutionTime = null;
    private readonly int _autotaggingRequestTimeout;
    private readonly string _autotaggingServiceConnectionString;
    private readonly string _autotaggingAiModel;
    private readonly double _autotaggingAiModelConfidence;

    public double GetAverageExecutionTime() => (_averageExecutionTime ?? _autotaggingRequestTimeout) / 1000;

    public double GetAverageExecutionTime(int previewsCount) 
    {
        var batchCount = (int)Math.Ceiling(((double)previewsCount) / _maxParallelDegree);
        var approximateTime = _averageExecutionTime * batchCount;
        
        if (previewsCount > 4)
        {
            approximateTime *= 1.2;
        }

        return (approximateTime ?? _autotaggingRequestTimeout * batchCount) / 1000;
    }

    public AutotagService(IConfigProvider configProvider, IMemoryCache memoryCache)
    {
        _autotaggingServiceConnectionString = configProvider.EnvironmentSettings.AutotaggingServiceConnectionString ?? 
                                              throw new Exception("Autotagging service Connection String is not set");
        _autotaggingAiModel = configProvider.EnvironmentSettings.AutotaggingAiModel ?? 
            throw new Exception("Autotagging AI Model is not set");
        _autotaggingAiModelConfidence = configProvider.EnvironmentSettings.AutotaggingAiModelConfidence;
        _autotaggingRequestTimeout = configProvider.EnvironmentSettings.AutotaggingRequestTimeout;

        var maxParallelDegree = configProvider.EnvironmentSettings.UseParallelProcessingForAutotagging 
            ? configProvider.EnvironmentSettings.AutotaggingMaxParallelDegree
            : 1;
        _semaphore = new Semaphore(maxParallelDegree, maxParallelDegree);
        
        _memoryCache = memoryCache;
        _memoryCacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));

        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMilliseconds(_autotaggingRequestTimeout),
            
        };
    }

    public List<AutotagResult> AutotagPreviewRange(List<Preview> previews, TagRepository tagRepository)
    {
        var results = new List<AutotagResult>();
        
        if (_maxParallelDegree > 0 && previews.Count > 1)
        {
            var chunks = previews.Chunk(previews.Count / _maxParallelDegree + 1);

            var tasks = chunks.Select(chunk => Task.Run(() =>
            {
                results.AddRange(chunk.Select(preview => AutotagPreview(preview, tagRepository)));
            }))
            .ToList();

            tasks.ForEach(x => x.Wait());

            return results;
        }

        results.AddRange(previews.Select(preview => AutotagPreview(preview, tagRepository)));

        return results;
    }

    public AutotagResult AutotagPreview(Preview preview, TagRepository tagRepository)
    {
        try {
            var stopwatch = DateTime.Now;

            _logger.Info("Executed AI tag autocompletion for Preview: {previewId} with model {_autotaggingAiModel}", preview.Id, _autotaggingAiModel);

            object data = new
            {
                image = preview.Content,
                model = _autotaggingAiModel,
                confidence = _autotaggingAiModelConfidence
            };
            
            var result = JsonConvert.DeserializeObject<AutotagResponse>(Post("predictTags", data));

            if (result is null)
            {
                return new AutotagResult
                {
                    PreviewId = preview.Id,
                    Tags = [],
                    IsSuccess = false,
                    ErrorMessage = "Autotagging service return empty result",
                    Rating = PreviewRatingType.Unknown
                };
            }

            var suggestedTags = result.Aliases.Split("\n")
                .Take(100)
                .Select(x => x.Split(":")[0])
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
                
            var suggestedTagsString = string.Join(" ", suggestedTags);

            var suggestedRating = result.Rating.ToLower() switch
            {
                "sensitive" => PreviewRatingType.Sensitive,
                "questionable" => PreviewRatingType.Questionable,
                "explicit" => PreviewRatingType.Explicit,
                _ => PreviewRatingType.General
            };

            var elapsedTime = (DateTime.Now - stopwatch).TotalMilliseconds;

            if (_averageExecutionTime == null) 
            {
                _averageExecutionTime = elapsedTime;
            }
            else 
            {
                _averageExecutionTime = (_averageExecutionTime + elapsedTime) / 2;
            }

            _logger.Debug("AI tag autocompletion for Preview: {previewId} successfully executed within: {elapsedTime} sec, suggested tags: {suggestedTagsString} rating: {suggestedRating}", 
                preview.Id, elapsedTime.ToString("N0"), suggestedTagsString, suggestedRating);
            
            _mutex.WaitOne();
            var actualTags = tagRepository.GetRangeByAliasString(suggestedTagsString);
            _mutex.ReleaseMutex();

            var actualTagsString = string.Join(" ", actualTags.Select(x => x.Name));

            _logger.Debug("Existing tags for suggestion: {actualTagsString}", actualTagsString);

            return new AutotagResult
            {
                PreviewId = preview.Id,
                Tags = actualTags,
                SuggestedAliases = suggestedTagsString,
                Rating = suggestedRating,
                IsSuccess = true
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to process autotagging for image");
            
            return new AutotagResult
            {
                PreviewId = preview.Id,
                Tags = [],
                IsSuccess = false,
                ErrorMessage = ex.Message,
                Rating = PreviewRatingType.Unknown
            };
        }
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
                limit = -1,
                model = _autotaggingAiModel
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

    public List<string> GetAvailableModels()
    {
        if (_memoryCache.TryGetValue("availableModels", out List<string>? availableModels))
        {
            return availableModels ?? [];
        }

        try
        {
            var result = Post("availableModels", new { });
            
            if (string.IsNullOrWhiteSpace(result))
            {
                return [];
            }
            
            availableModels = JsonConvert.DeserializeObject<List<string>>(result);
            _memoryCache.Set("availableModels", availableModels, _memoryCacheOptions);
            
            return availableModels ?? [];
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get available AI models");
            return [];
        }
    }

    private string Post(string method, object data)
    {
        var content = JsonConvert.SerializeObject(data);
        var buffer = System.Text.Encoding.UTF8.GetBytes(content);
        var contentBytes = new ByteArrayContent(buffer);

        contentBytes.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        _semaphore.WaitOne();
        var response = _httpClient.PostAsync(_autotaggingServiceConnectionString + "/" + method, contentBytes);
        var result = response.GetAwaiter().GetResult();
        _semaphore.Release();


        if (response.IsFaulted || result.StatusCode != HttpStatusCode.OK)
        {
            throw new HttpRequestException($"Request to JoyTag AI failed with code {result.StatusCode}");
        }

        return result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
    }
}
