using System.ComponentModel.DataAnnotations;

namespace MediaCloud.Data.Types
{
    public enum TagColor
    {
        [Display(Name = "Blue")]
        Blue = 0,
        [Display(Name = "Purple")]
        Purple = 1,
        [Display(Name = "Orange")]
        Orange = 2,
        [Display(Name = "Red")]
        Red = 3,
        [Display(Name = "Green")]
        Green = 4,
        [Display(Name = "Aquamarine")]
        Aquamarine = 5,
        [Display(Name = "Black")]
        Black = 6,
        [Display(Name = "White")]
        White = 7
    }
}
