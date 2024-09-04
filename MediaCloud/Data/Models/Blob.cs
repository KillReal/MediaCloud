using MediaCloud.Extensions;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaCloud.Data.Models
{
    public class Blob : Entity
    {
        public virtual Preview Preview { get; set; } = null!;

        public byte[] Content { get; set; } = [];

        public string Resolution { get; set; } = "0x0";

        public int Rate { get; set; }

        public long Size { get; set; }

        [NotMapped]
        public string SizeInfo
        {
            get => Size.FormatSize();
            set => Size = long.Parse(value);
        }

        public Blob(byte[] file)
        {
            Content = file;
            Resolution = $"-";
            Size = Content.Length;
            Rate = 0;
        }

        public Blob(byte[] file, int width, int height)
        {
            Content = file;
            Resolution = $"{width}x{height}";
            Size = Content.Length;
            Rate = 0;
        }

        public Blob()
        {

        }
    }
}
