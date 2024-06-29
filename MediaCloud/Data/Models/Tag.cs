using MediaCloud.Data.Types;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaCloud.Data.Models
{
    public class Tag : Entity
    {
        public string Name { get; set; } = "";

        public string? Description { get; set; } = "";
        public string Alias { get; set; } = "";

        public TagType Type { get; set; }

        public int PreviewsCount { get; set; }

        public virtual List<Preview> Previews { get; set; } = new();

        public Tag(string name, string description, TagType type)
        {
            Name = name;
            Description = description;
            Type = type;
        }

        public Tag()
        {

        }
    }
}
