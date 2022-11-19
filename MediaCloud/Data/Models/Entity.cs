namespace MediaCloud.Data.Models
{
    public class Entity : Record
    {
        public virtual Actor Creator { get; set; }
        public virtual Actor Updator { get; set; }
    }
}
