using System;
using MediaCloud.WebApp.Data.Types;
using MediaCloud.WebApp.Repositories;

namespace MediaCloud.WebApp.Data.Models.Interfaces;

public interface IPreviewRatable
{
   public PreviewRatingType Rating { get; set; }
}
