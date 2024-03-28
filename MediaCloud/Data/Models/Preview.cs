using MediaCloud.Data.Types;
using MediaCloud.Services;
using MediaCloud.WebApp.Data.Models.Interfaces;
using SixLabors.ImageSharp;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaCloud.Data.Models
{
    public class Preview : Entity, ITaggable
    {
        [ForeignKey("MediaId")]
        public virtual Media Media { get; set; }

        public MediaType MediaType { get; set; }

        public byte[] Content { get; set; }

        public virtual List<Tag> Tags { get; set; } = new();

        [ForeignKey("CollectionId")]
        public virtual Collection? Collection { get; set; }

        public int Order { get; set; }

        public Preview(Media media, Image convertedImage)
        {
            Media = media;
            MediaType = MediaType.JPG;
            Content = PictureService.LowerResolution(convertedImage, media.Content);
            Order = 0;
        }

        public Preview(Media media)
        {
            Media = media;
            MediaType = MediaType.JPG;
            Content = PictureService.LowerResolution(media.Content);
            Order = 0;
        }

        public Preview()
        {
            Media = new();
            Content = Array.Empty<byte>();
        }
    }
}
