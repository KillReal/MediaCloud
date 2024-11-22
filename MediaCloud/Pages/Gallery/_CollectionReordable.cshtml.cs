using MediaCloud.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace MediaCloud.WebApp;

public class _CollectionReordablePageModel(List<Preview> previews)
{
    [BindProperty]
    public List<Preview> Previews { get; set; } = previews;
}
