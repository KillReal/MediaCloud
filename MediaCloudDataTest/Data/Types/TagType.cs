using System.ComponentModel.DataAnnotations;

namespace MediaCloud.Data.Types
{
    public enum TagType
    {
        [Display(Name = "Unknown")]
        Unknown = 0,
        [Display(Name = "Character")]
        Character = 1,
        [Display(Name = "Series")]
        Series = 2,
        [Display(Name = "Fetish")]
        Fetish = 3,
    }
}
