using MediaCloud.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace MediaCloud.WebApp;

public class _TagsModel(List<Tag> tags)
{
    [BindProperty]
    public List<Tag> Tags { get; set; } = tags;
}
