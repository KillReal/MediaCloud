using DynamicExpression.Extensions;
using MediaCloud.Builders.List;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Data.Types;
using MediaCloud.WebApp.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace MediaCloud.Repositories
{
    public class TagRepository : Repository<Tag>, IListBuildable<Tag>
    {
        public TagRepository(AppDbContext context) : base(context)
        {
        }

        public bool Create(Tag tag)
        {
            tag.Creator = new ActorRepository(_context).GetCurrent();
            tag.Updator = tag.Creator;

            _context.Tags.Add(tag);
            _context.SaveChanges();

            return true;
        }

        public void RecalculateCounts()
        {
            var query = @"UPDATE 'Tags' AS t
                          SET 'PreviewsCount' = (
                                SELECT Count(*) 
                                FROM 'PreviewTag' AS pt 
                                WHERE pt.TagsId = t.Id
                          )";

            _context.Database.ExecuteSqlRawAsync(query);
        }

        public List<Tag> GetRangeByString(string tagsString)
        {
            if (string.IsNullOrEmpty(tagsString))
            {
                return new();
            }

            return _context.Tags.Where(x => tagsString.ToLower()
                                                      .Contains(x.Name.ToLower()))
                                .ToList();
        }

        public ReviewTagFilter GetTagFilter(string tagsString)
        {
            var tags = tagsString.ToLower().Split(' ');
            var positiveTags = new List<string>();
            var negativeTags = new List<string>();

            foreach (var tag in tags)
            {
                if (tag.Contains('!'))
                {
                    negativeTags.Add(tag.Remove(0, 1));
                }
                else
                {
                    positiveTags.Add(tag);
                }
            }

            var positiveTagIds = _context.Tags.Where(x => positiveTags.Contains(x.Name.ToLower()))
                                      .Select(x => x.Id)
                                      .ToList();

            var negativeTagIds = _context.Tags.Where(x => negativeTags.Contains(x.Name.ToLower()))
                                        .Select(x => x.Id)
                                        .ToList();

            return new(positiveTagIds, negativeTagIds);
        }

        public List<Tag> GetList(ListBuilder<Tag> listBuilder)
        {
            return _context.Tags.AsNoTracking()
                                .Order(listBuilder.Order)
                                .Skip(listBuilder.Offset)
                                .Take(listBuilder.Count)
                                .ToList();
        }

        public async Task<int> GetListCountAsync(ListBuilder<Tag> listBuilder)
        {
            return await _context.Tags.AsNoTracking()
                                .CountAsync();
        }
    }
}
