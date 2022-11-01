using MediaCloud.Data.Types;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Drawing.Imaging;

namespace MediaCloud.Data.Models
{
    public class Preview : Entity
    {
        [ForeignKey("PreviewId")]
        public virtual List<Media> Medias { get; set; }

        public MediaType MediaType { get; set; }

        public byte[] Content { get; set; }

        public virtual List<Tag> Tags { get; set; }

        private byte[] LowerResolution(byte[] imageBytes, int maxSize = -1)
        {
            if (maxSize == -1)
            {
                //maxSize = Convert.ToInt32(_configuration["PreviewMaxSize"]);
                maxSize = 700;
            }

            Stream stream = new MemoryStream(imageBytes);
            Image image = new Bitmap(stream);
            Size size = image.Size;
            int width = maxSize;
            int height = maxSize;
            float[] div = new float[] { size.Width / (float)width, size.Height / (float)height };
            float maxDiv = Math.Max(div[0], div[1]);

            if (maxDiv > 1.0)
            {
                Bitmap bitmap = new Bitmap(image, new Size(Convert.ToInt32(size.Width / maxDiv), Convert.ToInt32(size.Height / maxDiv)));
                MemoryStream ms = new MemoryStream();
                bitmap.Save(ms, ImageFormat.Jpeg);
                imageBytes = ms.ToArray();
            }

            return imageBytes;
        }

        public Preview(Media media)
        {
            Medias = new() { media };
            MediaType = MediaType.JPG;
            Content = LowerResolution(media.Content);
        }

        public Preview()
        {

        }
    }
}
