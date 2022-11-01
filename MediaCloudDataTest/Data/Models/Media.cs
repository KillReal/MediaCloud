using MediaCloud.Extensions;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace MediaCloud.Data.Models
{
    public class Media : Entity
    {
        public virtual Preview? Preview { get; set; }

        public byte[] Content { get; set; }

        public string Resolution { get; set; }

        public int Rank { get; set; }

        public int Size { get; set; }

        [NotMapped]
        public string SizeInfo
        {
            get => FormatBytesSize(Size);
            set => Size = int.Parse(value);
        }

        private string FormatBytesSize(long bytes, bool useUnit = true)
        {
            string[] Suffix = { " B", " kB", " MB", " GB", " TB" };
            double dblSByte = bytes;
            int i;
            for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }
            return $"{dblSByte:0}{(useUnit ? Suffix[i] : null)}";
        }

        public Media(IFormFile file)
        {
            Content = file.GetBytes();

            var stream = new MemoryStream(Content);
            var picture = new Bitmap(stream);

            Resolution = $"{picture.Width}x{picture.Height}";
            Size = Content.Count();

            Rank = 1;
        }

        public Media()
        {

        }
    }
}
