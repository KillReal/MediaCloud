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
        [Display(Name = "Trait")]
        Trait = 4,
        [Display(Name = "Location")]
        Location = 5,
        [Display(Name = "Mark")]
        Mark = 6,
    }
}
