using System.ComponentModel.DataAnnotations;

namespace MediaCloud.Data.Types
{
    public enum TagType
    {
        [Display(Name = "Unknown")]
        Unknown = 0,
        [Display(Name = "Series")]
        Series = 1,
        [Display(Name = "Character")]
        Character = 2,
        [Display(Name = "Clothes")]
        Clothes = 3,
        [Display(Name = "Fetish")]
        Fetish = 4,
    }
}
