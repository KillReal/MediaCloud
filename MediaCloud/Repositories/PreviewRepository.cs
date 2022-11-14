using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using DynamicExpression.Extensions;
using MediaCloud.Builders.List;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Extensions;
using Microsoft.EntityFrameworkCore;

namespace MediaCloud.Repositories
{
    public class PreviewRepository : Repository<Preview>, IListBuildable<Preview>
    {
        public PreviewRepository(AppDbContext context) : base(context)
        {
        }

        public Preview? GetAndSetTags(Guid id, List<Tag> tags)
        {
            var preview = Get(id);

            if (preview == null)
            {
                return null;
            }

            if (preview.Collection != null)
            {
                preview = preview.Collection.Previews.OrderBy(x => x.Order)
                                                     .First();
            }

            preview.Tags.Clear();
            preview.Tags.AddRange(tags);
            _context.Previews.Update(preview);
            _context.SaveChanges();

            return preview;
        }

        public async Task<int> GetMediaCountAsync(Guid id)
        {
            return await _context.Medias.AsNoTracking()
                                        .Where(x => x.Preview.Id == id)
                                        .AsQueryable()
                                        .CountAsync();
        }


        public async Task<int> GetListCountAsync(ListBuilder<Preview> listBuilder)
        {
            var query = _context.Previews.AsQueryable()
                                         .Where(x => x.Order == 0);

            if (!string.IsNullOrEmpty(listBuilder.Filter))
            {
                var tagIds = _context.Tags.AsNoTracking()
                                          .Where(x => listBuilder.Filter.ToLower()
                                                                        .Contains(x.Name.ToLower()))
                                          .Select(x => x.Id)
                                          .ToList();

                query = query.Where(x => x.Tags.Where(y => tagIds.Contains(y.Id))
                                               .Count() == tagIds.Count);
            }

            return await query.AsNoTracking()
                                .CountAsync();
        }

        public List<Preview> GetList(ListBuilder<Preview> listBuilder)
        {
            var query = _context.Previews.AsNoTracking()
                                         .Where(x => x.Order == 0);

            if (!string.IsNullOrEmpty(listBuilder.Filter))
            {
                var tagFilter = new TagRepository(_context).GetTagFilter(listBuilder.Filter);

                query = tagFilter.GetQuery(query);
            }

            if (listBuilder.Sort.Contains("Random") &&
                int.TryParse(listBuilder.Sort.Split('-').Last(), out int seed))
            {
                var previewIds = _context.Previews.Where(x => x.Order == 0)
                                                        .Select(x => x.Id);

                var previewIdsList = previewIds.Shuffle(seed)
                                               .ToList();

                return query.Where(x => previewIdsList.Any(id => id == x.Id))
                            .Include(x => x.Collection)
                            .ToList()
                            .OrderBy(x => previewIdsList.IndexOf(x.Id))
                            .Skip(listBuilder.Offset)
                            .Take(listBuilder.Count)
                            .ToList();
            }

            return query.Order(listBuilder.Order)
                        .Skip(listBuilder.Offset)
                        .Take(listBuilder.Count)
                        .Include(x => x.Collection)
                        .ToList();
        }

        public override bool TryRemove(Guid id)
        {
            var preview = Get(id);

            if (preview == null)
            {
                return false;
            }

            if (preview.Collection != null)
            {
                preview.Collection.Previews = preview.Collection.Previews.OrderBy(x => x.Order)
                                                                         .ToList();
                if (preview.Order == 0)
                {
                    if (preview.Collection.Previews.Count > 1)
                    {
                        preview.Collection.Previews[1].Order = 0;
                        preview.Collection.Previews[1].Tags = preview.Tags;
                        preview.Collection.Count--;

                        _context.Collections.Update(preview.Collection);
                        _context.Medias.Remove(preview.Media);
                        _context.SaveChanges();
                        return true;
                    }

                    _context.Medias.Remove(preview.Media);
                    _context.Collections.Remove(preview.Collection);
                    _context.SaveChanges();
                    return true;
                }
            }

            _context.Medias.Remove(preview.Media);
            _context.SaveChanges();
            return true;
        }
    }
}
