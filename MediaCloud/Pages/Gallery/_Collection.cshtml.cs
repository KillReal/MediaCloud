﻿using MediaCloud.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace MediaCloud.WebApp;

public class _CollectionPageModel(List<Preview> previews, int batchIdOffset = 0)
{
    [BindProperty] public List<Preview> Previews { get; set; } = previews;
    [BindProperty] public int BatchIdOffset { get; set; } = batchIdOffset;
}
