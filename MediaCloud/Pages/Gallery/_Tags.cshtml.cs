using MediaCloud.Data.Models;
using MediaCloud.WebApp.Data.Types;
using Microsoft.AspNetCore.Mvc;

namespace MediaCloud.WebApp;

public class _GalleryPageModel(List<Preview> previews, PreviewRatingType rating)
{
    [BindProperty]
    public List<Preview> Previews { get; set; } = previews;
    [BindProperty]
    public PreviewRatingType AllowedNSFWContentRating { get; set; } = rating;
}
