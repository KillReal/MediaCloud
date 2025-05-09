using MediaCloud.WebApp;
using MediaCloud.WebApp.Data.Models.Interfaces;
using MediaCloud.WebApp.Data.Types;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaCloud.Data.Models
{
    public class Preview : Entity, ITaggable, IBlobNameSearchable
    {
        [ForeignKey("BlobId")]
        public virtual Blob Blob { get; set; }

        public string BlobType { get; set; }
        
        public string BlobName { get; set; }
        
        public byte[] Content { get; set; }
        
        public virtual List<Tag> Tags { get; set; }
        
        [ForeignKey("CollectionId")]
        public virtual Collection? Collection { get; set; }
        
        public int Order { get; set; }

        public PreviewRatingType Rating { get; set; }

      public Preview(Blob file, UploadedFile uploadedFile)
        {
            Blob = file;
            BlobName = uploadedFile.Name;
            BlobType = uploadedFile.Type;
            Content = uploadedFile.Content;
            Tags = [];
        }

        public Preview()
        {
            Blob = new();
            BlobName = "unknown";
            BlobType = "unknown";
            Content = [];
            Tags = [];
        }
    }
}
