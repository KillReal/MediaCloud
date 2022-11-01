using DynamicExpression.Extensions;
using MediaCloud.Builders.List;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Data.Types;
using Microsoft.EntityFrameworkCore;

namespace MediaCloud.Services
{
    public class TagRepository : Repository<Tag>, IRepository<Tag>
    {
        public TagRepository(AppDbContext context) : base(context)
        {
        }

        public Tag? Get(Guid id)
        {
            return _context.Tags.Where(x => x.Id == id)
                                        .FirstOrDefault();
        }

        public bool Create(Tag tag)
        {
            tag.Creator = new ActorRepository(_context).GetCurrent();
            tag.Updator = tag.Creator;

            _context.Tags.Add(tag);
            _context.SaveChanges();

            return true;
        }

        public List<Tag> GetRangeByString(string tagsString)
        {
            if (string.IsNullOrEmpty(tagsString))
            {
                return new();
            }

            var tags = _context.Tags.Where(x => tagsString.ToLower().Contains(x.Name.ToLower()))
                                    .ToList();

            if (!tags.Any())
            {
                return new();
            }

            return tags;
        }

        private Tag SetPreviewsCount(Tag tag)
        {
            tag.PreviewsCount = _context.Entry(tag)
                                           .Collection(x => x.Previews)
                                           .Query()
                                           .Count();

            return tag;
        }

        private List<Tag> SetPreviewsCount(List<Tag> tags)
        {
            for(var i = 0; i < tags.Count(); i++)
            {
                tags[i] = SetPreviewsCount(tags[i]);
            }

            return tags;
        }

        public List<Tag> GetList(ListBuilder<Tag> listBuilder)
        {
            var tags = new List<Tag>();
            var order = listBuilder.Sorting.GetOrder();
            if (order.By == "PreviewsCount")
            {
                tags = _context.Tags.ToList();

                tags = SetPreviewsCount(tags).AsQueryable()
                                            .Order(listBuilder.Sorting.GetOrder())
                                            .Skip(listBuilder.Pagination.Offset)
                                            .Take(listBuilder.Pagination.Count)
                                            .ToList();
            }
            else
            {
                tags = _context.Tags.AsQueryable()
                                        .Order(listBuilder.Sorting.GetOrder())
                                        .Skip(listBuilder.Pagination.Offset)
                                        .Take(listBuilder.Pagination.Count)
                                        .ToList();

                tags = SetPreviewsCount(tags);
            }

            if (!tags.Any())
            {
                return new();
            }

            return tags;
        }

        public int GetListCount(ListBuilder<Tag> listBuilder)
        {
            return _context.Tags.Count();
        }
    }
}
