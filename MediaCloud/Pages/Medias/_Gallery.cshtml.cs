using MediaCloud.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace MediaCloud.WebApp;

public class _GalleryPageModel
{
    [BindProperty]
    public List<Preview> Previews { get; set; }

    public _GalleryPageModel(List<Preview> previews)
    {
        Previews = previews;
    }
}
