namespace MediaCloud.Data.Models
{
    public class Collection : Entity
    {
        public virtual List<Preview> Previews { get; set; } = new();

        public int Count { get; set; }

        public Collection(List<Preview> previews)
        {
            Previews = previews;
        }

        public Collection()
        {
           
        }
    }
}
