using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MediaCloud.WebApp.Data.Types;

public enum PreviewRatingType
{
   [Display(Name = "Unknown")]
   Unknown,
   [Display(Name = "General")]
   General,
   [Display(Name = "Sensitive")]
   Sensitive,
   [Display(Name = "Questionable")]
   Questionable,
   [Display(Name = "Explicit")]
   Explicit
}
