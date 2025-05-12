using System.ComponentModel.DataAnnotations.Schema;

namespace MediaCloud.Data.Models
{
    public class Entity : Record
    {
        [ForeignKey("Creator")]
        public Guid CreatorId { get; set; }
        public virtual User Creator { get; set; } = null!;
        public virtual User Updator { get; set; } = null!;
    }
}
