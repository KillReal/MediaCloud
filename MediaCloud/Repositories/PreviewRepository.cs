using DynamicExpression.Extensions;
using MediaCloud.Builders.List;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Repositories.Base;
using MediaCloud.WebApp.Services.UserProvider;
using MediaCloud.WebApp.Services.Data.Repositories.Interfaces;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.EntityFrameworkCore;
using NLog;
using MediaCloud.Repositories;
using MediaCloud.Data.Types;

namespace MediaCloud.WebApp.Repositories
{
    public class PreviewRepository(AppDbContext context, StatisticProvider statisticProvider, IUserProvider actorProvider) : BaseRepository<Preview>(context, statisticProvider, LogManager.GetLogger("CollectionRepository"), actorProvider), IListBuildable<Preview>
    {
        private static string[] GetDeduplicatedTags(string tagString)
        {
            var tags = tagString.Split(' ');

            return tags.Length < 2 
                ? tags 
                : [.. tags.Distinct()];
        }

        private TagFilter<Preview> GetFilterQueryByTags(string tagsString)
        {
            var tags = GetDeduplicatedTags(tagsString);

            var positiveTags = new List<string>();
            var negativeTags = new List<string>();
            foreach (var tag in tags)
            {
                if (tag.Contains('!'))
                {
                    negativeTags.Add(tag[1..]);
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
                    query = query.Where(x => !x.Tags.Any(x => x.Type == TagType.Character));
                }
                else if (filter.Contains("character") || filter.Contains("char"))
                {
                    query = query.Where(x => x.Tags.Any(x => x.Type == TagType.Character));
                }

                if (filter.Contains("!series"))
                {
                    query = query.Where(x => !x.Tags.Any(x => x.Type == TagType.Series));
                }
                else if (filter.Contains("series"))
                {
                    query = query.Where(x => x.Tags.Any(x => x.Type == TagType.Series));
                }

                return GetFilterQueryByTags(filter).ApplyToQuery(query);
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

            return await SetFilterToQuery(query, listBuilder.Filtering.Filter)
                .Where(x => x.CreatorId == _actor.Id)
                .CountAsync();
        }

        public async Task<List<Preview>> GetListAsync(ListBuilder<Preview> listBuilder)
        {
            var query = _context.Previews.AsNoTracking().Where(x => x.Order == 0);
            query = SetFilterToQuery(query, listBuilder.Filtering.Filter);

            _statisticProvider.ActivityFactorRaised.Invoke();

            if (listBuilder.Sorting.PropertyName.Contains("Random"))
            {
                return await query.Where(x => x.Order == 0 && x.CreatorId == _actor.Id)
                    .Include(x => x.Collection)
                    .OrderBy(x => EF.Functions.Random())
                    .Skip(listBuilder.Pagination.Offset)
                    .Take(listBuilder.Pagination.Count)
                    .ToListAsync();
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
                if (preview.Collection.Count > 1)
                {
                    if (preview.Order == 0)
                    {
                        var newTitlePreview = _context.Previews.Where(x => x.Collection == preview.Collection)
                            .OrderBy(x => x.Order)
                            .First(x => x.Order > preview.Order);
                        newTitlePreview.Order = 0;
                    }
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

            var mediaId = preview.Blob.Id;

            _context.Blobs.Remove(preview.Blob);
            SaveChanges();
            _logger.Info("Removed Media  with id: {mediaId} by: {_actor.Name}", mediaId, _actor.Name);
            _statisticProvider.MediasCountChanged.Invoke(-1, -size);

            return true;
        }
    }
}
