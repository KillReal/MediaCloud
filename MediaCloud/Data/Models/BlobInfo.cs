using MediaCloud.Extensions;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaCloud.Data.Models
{
    public class BlobInfo : Entity
    {
        public string Resolution { get; set; } = "0x0";

        public int Rate { get; set; }

        public long Size { get; set; }
        
        public string SizeInfo
        {
            get => Size.FormatSize();
            set => Size = long.Parse(value);
        }

        public BlobInfo(Guid id, Guid creatorId, DateTime createdAt, DateTime updatedAt, string resolution, int rate, long size)
        {
            Id = id;
            CreatorId = creatorId;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            Resolution = resolution;
            Rate = rate;
            Size = size;
        }

        public BlobInfo(Blob blob) : this(blob.Id, blob.CreatorId, blob.CreatedAt, blob.UpdatedAt, blob.Resolution, blob.Rate, blob.Size)
        {
            
        }

        public BlobInfo()
        {
            
        }
    }
}
