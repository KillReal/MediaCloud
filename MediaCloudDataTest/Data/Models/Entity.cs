namespace MediaCloud.Data.Models
{
    public class Entity
    {
        public Guid Id { get; set; }
        public virtual Actor Creator { get; set; }
        public DateTime CreatedAt { get; set; }
        public virtual Actor Updator { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
