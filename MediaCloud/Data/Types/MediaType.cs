using System.ComponentModel.DataAnnotations;

namespace MediaCloud.Data.Types
{
    public enum FileType
    {
        [Display(Name = "unknown")]
        Unknown = 0,
        [Display(Name = "jpg")]
        JPG = 1,
        [Display(Name = "mp4")]
        MP4 = 2
    }
}
