﻿using System.Diagnostics;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.Services;
using MediaCloud.WebApp.Controllers;
using MediaCloud.WebApp.Services.ActorProvider;
using MediaCloud.WebApp.Services.ConfigurationProvider;
using Microsoft.AspNetCore.Routing.Constraints;
using NLog;
using ILogger = NLog.ILogger;

namespace MediaCloud.WebApp;

public class AutotagService : IAutotagService
{
    private readonly ILogger _logger = LogManager.GetLogger("AutotagService");
    private readonly IPictureService _pictureService;

    private readonly List<Guid> _proceededPreviewIds = new();

    private double _averageExecutionTime = 0;
    private string _tempFilePath;
    private string _joyTagExecutionPath;
    private string _joyTagTagsPath;
    private string _pythonPath;

    public double GetAverageExecutionTime() => _averageExecutionTime;

    public AutotagService(IPictureService pictureService, IConfigProvider configProvider)
    {
        _pictureService = pictureService;

        _tempFilePath = configProvider.EnvironmentSettings.PreviewAiAutotagProcessingPath ?? "./temp.jpg";
        _joyTagExecutionPath = configProvider.EnvironmentSettings.AiJoyTagExecutionPath ?? "./JoyTag/joytag.py";
        _joyTagTagsPath = configProvider.EnvironmentSettings.AiJoyTagTagsPath ?? "./JoyTag/models/top_tags.txt";
        _pythonPath = configProvider.EnvironmentSettings.PythonPath ?? "python3";
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
            var image = _pictureService.SaveImageToPath(preview.Content, _tempFilePath + "/temp.jpg");

            Run(_joyTagExecutionPath, "");

            var suggestedTags = File.ReadLines(_tempFilePath + "/suggested_tags.txt")
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
            
            var actualTags = tagRepository.GetRangeByAliasString(suggestedTagsString);
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

    public List<Tag> AutocompleteTagsForCollection(Collection collection, TagRepository tagRepository)
    {
        if (collection == null || collection.Previews.Any() == false)
        {
            return new();
        }

        try {
            var stopwatch = DateTime.Now;
            _proceededPreviewIds.AddRange(collection.Previews.Select(x => x.Id));

            _logger.Info("Executed AI tag autocompletion for Collection: {collection.Id}", collection.Id);

            List<Tag> tags = new();

            foreach(var preview in collection.Previews)
            {
                tags = tags.Union(AutocompleteTagsForPreview(preview, tagRepository)).ToList();
            }
            
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
        try {
            return File.ReadLines(_joyTagTagsPath)
                .Where(x => x.ToLower().StartsWith(searchString.ToLower()))
                .Take(limit)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to read tag aliases");
            return new();
        }
    }

    public bool IsPreviewIsProceeded(Guid previewId)
    {
        return _proceededPreviewIds.Exists(x => x == previewId);
    }

    private void Run(string cmd, string args)
    {
        ProcessStartInfo info = new()
        {
            FileName = _pythonPath,
            Arguments = string.Format("\"{0}\" \"{1}\"", cmd, args),
            UseShellExecute = false,// Do not use OS shell
            CreateNoWindow = false, // We don't need new window
            RedirectStandardOutput = true,// Any output, generated by application will be redirected back
            RedirectStandardError = true // Any error in standard output will be redirected back (for example exceptions)
        };

        Process.Start(info)?.WaitForExit();
    }
}
