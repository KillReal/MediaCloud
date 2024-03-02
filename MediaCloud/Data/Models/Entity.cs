using System.ComponentModel.DataAnnotations.Schema;

namespace MediaCloud.Data.Models
{
    public class Entity : Record
    {
        [ForeignKey("Creator")]
        public Guid CreatorId { get; set; }
        public virtual Actor Creator { get; set; } = new();
        public virtual Actor Updator { get; set; } = new();
    }
}
