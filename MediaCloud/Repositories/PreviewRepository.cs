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
        public PreviewRepository(AppDbContext context, ILogger logger)
            : base(context, logger)
        {
        }

        public void SetPreviewTags(Preview preview, List<Tag>? tags)
        {
            if (preview.Collection != null)
            {
                preview = preview.Collection.Previews.OrderBy(x => x.Order)
                                                     .First();
            }

            preview.Tags.Clear();
            if (tags != null)
            {
                preview.Tags.AddRange(tags);
            }

            _context.Previews.Update(preview);
            _context.SaveChanges();

            new TagRepository(_context, _logger).RecalculateCountsAsync();

            return;
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
            var query = _context.Previews.AsQueryable().Where(x => x.Order == 0);

            if (string.IsNullOrEmpty(listBuilder.Filter) == false)
            {
                var tagFilter = new TagRepository(_context, _logger).GetTagFilter(listBuilder.Filter);
                query = tagFilter.GetQuery(query);
            }

            return await query.AsNoTracking()
                              .CountAsync();
        }

        public List<Preview> GetList(ListBuilder<Preview> listBuilder)
        {
            var query = _context.Previews.AsNoTracking()
                                         .Where(x => x.Order == 0);

            if (string.IsNullOrEmpty(listBuilder.Filter) == false)
            {
                var tagFilter = new TagRepository(_context, _logger).GetTagFilter(listBuilder.Filter);
                query = tagFilter.GetQuery(query);
            }

            if (listBuilder.Sort.Contains("Random") &&
                int.TryParse(listBuilder.Sort.Split('-').Last(), out int seed))
            {
                var previewIdsList = _context.Previews.Where(x => x.Order == 0)
                                                      .Select(x => x.Id)
                                                      .Shuffle(seed)
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
                          
                        new CollectionRepository(_context, _logger).Update(preview.Collection);
                        new MediaRepository(_context, _logger).Remove(preview.Media);
                        SaveChanges();

                        return true;
                    }

                    new MediaRepository(_context, _logger).Remove(preview.Media);
                    new CollectionRepository(_context, _logger).Remove(preview.Collection);
                    SaveChanges();
                    
                    return true;
                }
            }

            new MediaRepository(_context, _logger).Remove(preview.Media);
            SaveChanges();

            return true;
        }
    }
}
