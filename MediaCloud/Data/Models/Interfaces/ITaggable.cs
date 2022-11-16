using MediaCloud.Data.Models;

namespace MediaCloud.WebApp.Data.Models.Interfaces
{
    public interface ITaggable
    {
        public List<Tag> Tags { get; set; }
    }
}
