using MediaCloud.Extensions;
using MediaCloud.Services;
using System.ComponentModel.DataAnnotations.Schema;
using SixLabors.ImageSharp;

namespace MediaCloud.Data.Models
{
    public class Media : Entity
    {
        public virtual Preview Preview { get; set; } = null!;

        public byte[] Content { get; set; } = Array.Empty<byte>();

        public string Resolution { get; set; } = "0x0";

        public int Rate { get; set; }

        public long Size { get; set; }

        [NotMapped]
        public string SizeInfo
        {
            get => Size.FormatSize();
            set => Size = long.Parse(value);
        }

        public Media(byte[] file, int width, int height)
        {
            Content = file;
            Resolution = $"{width}x{height}";
            Size = Content.Length;
            Rate = 0;
        }

        public Media()
        {

        }
    }
}
