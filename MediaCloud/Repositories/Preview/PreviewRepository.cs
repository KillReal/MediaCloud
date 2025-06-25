using DynamicExpression.Extensions;
using MediaCloud.Builders.List;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.UserProvider;
using MediaCloud.WebApp.Services.Data.Repositories.Interfaces;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.EntityFrameworkCore;
using NLog;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Builders;
using Microsoft.Extensions.Caching.Memory;

namespace MediaCloud.WebApp.Repositories
{
    public class PreviewRepository(AppDbContext context, StatisticProvider statisticProvider, IUserProvider userProvider, IMemoryCache memoryCache) 
        : BaseRepository<Preview>(context, statisticProvider, LogManager.GetLogger("CollectionRepository"), userProvider), IListBuildable<Preview>
    {
        private IQueryable<Preview> GetFilterQuery(IQueryable<Preview> query, string filter)
        {
            filter = filter.ToLower();

            var ratingFilter = new RatingFiltration<Preview>(filter);
            filter = ratingFilter.GetFilterWithoutRatings();

            if (string.IsNullOrWhiteSpace(filter))
            {
                return query.Where(ratingFilter.GetExpression());
            }
            
            var tagFilter = new TagFiltration<Preview>(filter, _context.Tags);
            var nameFilter = new BlobNameFiltration<Preview>(filter, _context.Tags, GetCustomFilterAliases());

            var filterExpression = tagFilter.GetExpression()
                .And(ratingFilter.GetExpression());

            if (nameFilter.IsValid())
            {
                filterExpression = filterExpression.Or(nameFilter.GetExpression());
            }
            
            return query.Where(filterExpression);
        }

        private IQueryable<Preview> SetFilterToQuery(IQueryable<Preview> query, string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return query;
            }

            return GetFilterQuery(query, filter);
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
                .Where(x => x.CreatorId == _user.Id)
                .CountAsync();
        }

        public async Task<List<Preview>> GetListAsync(ListBuilder<Preview> listBuilder)
        {
            var query = _context.Previews.AsNoTracking().Where(x => x.Order == 0);
            query = SetFilterToQuery(query, listBuilder.Filtering.Filter);

            _statisticProvider.ActivityFactorRaised.Invoke();

            if (listBuilder.Sorting.PropertyName.Contains("Random"))
            {
                return await query.Where(x => x.Order == 0 && x.CreatorId == _user.Id)
                    .Include(x => x.Collection)
                    .OrderBy(x => EF.Functions.Random())
                    .Skip(listBuilder.Pagination.Offset)
                    .Take(listBuilder.Pagination.Count)
                    .ToListAsync();
            }  

            return await query.Order(listBuilder.Sorting.GetOrder())
                        .Where(x => x.CreatorId == _user.Id)
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
                        preview.Collection.Id, _user.Name);
                    _statisticProvider.MediasCountChanged.Invoke(-1, -size);

                    return true;
                }

                var collectionId = preview.Collection.Id;

                _context.Blobs.Remove(preview.Blob);
                _context.Collections.Remove(preview.Collection);
                SaveChanges();
                _logger.Info("Removed Collection with id: {collectionId} by: {_actor.Name}", collectionId, _user.Name);
                _statisticProvider.MediasCountChanged.Invoke(-1, -size);

                return true;
            }

            var mediaId = preview.Blob.Id;

            _context.Blobs.Remove(preview.Blob);
            SaveChanges();
            _logger.Info("Removed Media  with id: {mediaId} by: {_actor.Name}", mediaId, _user.Name);
            _statisticProvider.MediasCountChanged.Invoke(-1, -size);

            return true;
        }

        public List<string> GetCustomFilterAliases()
        {
            var aliases = new List<string>();

            if (memoryCache.TryGetValue("PreviewTagFilterAliases", out List<string>? tagAliases) == false)
            {
                tagAliases = TagFiltration<Preview>.GetAliasSuggestions().Select(x => new string(x)).ToList();
                memoryCache.Set("PreviewTagFilterAliases", tagAliases);
            }
            
            aliases.AddRange(tagAliases ?? []);

            if (userProvider.GetCurrent().IsAutotaggingAllowed == false)
            {
                return aliases;
            }
            
            if (memoryCache.TryGetValue("PreviewRatingFilterAliases", out List<string>? ratingAliases) == false)
            {
                ratingAliases = RatingFiltration<Preview>.GetAliasSuggestions().Select(x => new string(x)).ToList();
                memoryCache.Set("PreviewRatingFilterAliases", ratingAliases);
            }
                
            aliases.AddRange(ratingAliases ?? []);

            return aliases;
        }
    }
}
