using MediaCloud.Data.Models;
using MediaCloud.WebApp.Data.Types;
using Microsoft.AspNetCore.Mvc;

namespace MediaCloud.WebApp;

public class _CollectionPageModel(List<Preview> previews, PreviewRatingType rating, int batchIdOffset = 0)
{
    [BindProperty] public List<Preview> Previews { get; set; } = previews;
    [BindProperty] public int BatchIdOffset { get; set; } = batchIdOffset;
    [BindProperty] public PreviewRatingType AllowedNSFWContentRating { get; set; } = rating;
}
