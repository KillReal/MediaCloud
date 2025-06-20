using MediaCloud.Data.Models;
using MediaCloud.WebApp.Data.Types;
using Microsoft.AspNetCore.Mvc;

namespace MediaCloud.WebApp;

public class _FloatingTagsModel(List<Tag> tags)
{
    public List<Tag> Tags { get; set; } = tags;
}