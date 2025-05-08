using System;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Data.Types;

namespace MediaCloud.WebApp.Services.AutotagService;

public class AutotagResult
{
   public required Guid PreviewId { get; set; }
   public bool IsSuccess { get; set; }
   public required List<Tag> Tags { get; set; }
   public PreviewRatingType Rating { get; set; }
   public string? SuggestedAliases { get; set; }
   public string? ErrorMessage { get; set; }
}
