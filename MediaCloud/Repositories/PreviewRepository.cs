using DynamicExpression.Extensions;
using MediaCloud.Builders.List;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Extensions;
using MediaCloud.WebApp.Repositories.Base;
using MediaCloud.WebApp.Services.UserProvider;
using MediaCloud.WebApp.Services.Data.Repositories.Interfaces;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace MediaCloud.Repositories
{
    public class PreviewRepository(AppDbContext context, StatisticProvider statisticProvider, IUserProvider actorProvider) : BaseRepository<Preview>(context, statisticProvider, LogManager.GetLogger("CollectionRepository"), actorProvider), IListBuildable<Preview>
    {
        private static string DeduplicateTagString(string tagString)
        {
            var tags = tagString.Split(' ');

            if (tags.Length < 2)
            {
                return tagString;
            }

            return string.Join(' ', tags.Distinct());
        }

        private TagFilter<Preview> GetFilterQueryByTags(string tagsString)
        {
            tagsString = DeduplicateTagString(tagsString);
            var tags = tagsString.ToLower().Split(' ');

            var positiveTags = new List<string>();
            var negativeTags = new List<string>();
            foreach (var tag in tags)
            {
                if (tag.Contains('!'))
                {
                    negativeTags.Add(tag.Remove(0, 1));
                    continue;
                }

                positiveTags.Add(tag);
            }

            var positiveTagIds = _context.Tags.Where(x => positiveTags.Any(y => y.ToLower() == x.Name.ToLower()))
                                              .Select(x => x.Id);
            var negativeTagIds = _context.Tags.Where(x => negativeTags.Any(y => y.ToLower() == x.Name.ToLower()))
                                              .Select(x => x.Id);

            return new([.. positiveTagIds], [.. negativeTagIds]);
        }

        private IQueryable<Preview> SetFilterToQuery(IQueryable<Preview> query, string filter)
        {
            if (string.IsNullOrEmpty(filter) == false)
            {
                // TODO: rework tag type to complete db model with TagTypeDataService.
                // Rework TagType filtering
                // Or exclude TagTypes at all...

                if (filter.Contains("notag"))
                {
                    return query.Where(x => x.Tags.Count == 0);
                }

                if (filter.Contains("!character") || filter.Contains("!char"))
                {
                    query = query.Where(x => !x.Tags.Any(x => x.Type == Data.Types.TagType.Character));
                }
                else if (filter.Contains("character") || filter.Contains("char"))
                {
                    query = query.Where(x => x.Tags.Any(x => x.Type == Data.Types.TagType.Character));
                }

                if (filter.Contains("!series"))
                {
                    query = query.Where(x => !x.Tags.Any(x => x.Type == Data.Types.TagType.Series));
                }
                else if (filter.Contains("series"))
                {
                    query = query.Where(x => x.Tags.Any(x => x.Type == Data.Types.TagType.Series));
                }

                return GetFilterQueryByTags(filter).GetQuery(query);
            }

            return query;
        }

        public override Preview? Get(Guid id)
        {
            var preview = base.Get(id);

            if (preview?.Order == 0)
            {
                _statisticProvider.ActivityFactorRaised.Invoke();
            }

            return preview;
        }

        public async Task<int> GetListCountAsync(ListBuilder<Preview> listBuilder)
        {
            var query = _context.Previews.AsNoTracking().Where(x => x.Order == 0);

            return await SetFilterToQuery(query, listBuilder.Filtering.Filter).Where(x => x.CreatorId == _actor.Id)
                                                                    .CountAsync();
        }

        public async Task<List<Preview>> GetListAsync(ListBuilder<Preview> listBuilder)
        {
            var query = _context.Previews.AsNoTracking().Where(x => x.Order == 0);
            query = SetFilterToQuery(query, listBuilder.Filtering.Filter);

            _statisticProvider.ActivityFactorRaised.Invoke();

            if (listBuilder.Sorting.PropertyName.Contains("Random") &&
                int.TryParse(listBuilder.Sorting.PropertyName.Split('_').Last(), out int seed))
            {
                var previewIdsList = _context.Previews.Where(x => x.Order == 0)
                                                      .Select(x => x.Id)
                                                      .Shuffle(seed)
                                                      .ToList();

                var list = await query.Where(x => previewIdsList.Any(id => id == x.Id) 
                                            && x.CreatorId == _actor.Id)
                                        .Include(x => x.Collection)
                                        .ToListAsync();

                return list.OrderBy(x => previewIdsList.IndexOf(x.Id))
                            .Skip(listBuilder.Pagination.Offset)
                            .Take(listBuilder.Pagination.Count)
                            .ToList();
            }  

            return await query.Order(listBuilder.Sorting.GetOrder())
                        .Where(x => x.CreatorId == _actor.Id)
                        .Skip(listBuilder.Pagination.Offset)
                        .Take(listBuilder.Pagination.Count)
                        .Include(x => x.Collection)
                        .ToListAsync();
        }

        public override bool TryRemove(Guid id)
        {
            var preview = Get(id);

            if (preview == null)
            {
                return false;
            }

            var size = preview.Blob.Size;
            
            if (preview.Collection != null)
            {
                preview.Collection.Previews = [.. preview.Collection.Previews.OrderBy(x => x.Order)];
                if (preview.Order == 0)
                {
                    if (preview.Collection.Previews.Count > 1)
                    {
                        preview.Collection.Previews[1].Order = 0;
                        preview.Collection.Previews[1].Tags = preview.Tags;
                        preview.Collection.Count--;

                        _context.Collections.Update(preview.Collection);
                        _context.Blobs.Remove(preview.Blob);
                        SaveChanges();
                        _logger.Info("Removed Media in Collection with id: {preview.Collection.Id} by: {_actor.Name}", 
                            preview.Collection.Id, _actor.Name);
                        _statisticProvider.MediasCountChanged.Invoke(-1, -size);

                        return true;
                    }

                    var collectionId = preview.Collection.Id;

                    _context.Blobs.Remove(preview.Blob);
                    _context.Collections.Remove(preview.Collection);
                    SaveChanges();
                    _logger.Info("Removed Collection with id: {collectionId} by: {_actor.Name}", collectionId, _actor.Name);
                    _statisticProvider.MediasCountChanged.Invoke(-1, -size);

                    return true;
                }
            }

            var mediaId = preview.Blob.Id;

            _context.Blobs.Remove(preview.Blob);
            SaveChanges();
            _logger.Info("Removed Media  with id: {mediaId} by: {_actor.Name}", mediaId, _actor.Name);
            _statisticProvider.MediasCountChanged.Invoke(-1, -size);

            return true;
        }
    }
}
