using MediaCloud.Data.Types;
using MediaCloud.Services;
using MediaCloud.WebApp.Data.Models.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Drawing.Imaging;

namespace MediaCloud.Data.Models
{
    public class Preview : Entity, ITaggable
    {
        [ForeignKey("MediaId")]
        public virtual Media Media { get; set; }

        public MediaType MediaType { get; set; }

        public byte[] Content { get; set; }

        public virtual List<Tag> Tags { get; set; }

        [ForeignKey("CollectionId")]
        public virtual Collection? Collection { get; set; }

        public int Order { get; set; }

        public Preview(Media media)
        {
            MediaType = MediaType.JPG;
            Content = PictureService.LowerResolutionToPreview(media.Content);
            Order = 0;
        }

        public Preview()
        {

        }
    }
}
