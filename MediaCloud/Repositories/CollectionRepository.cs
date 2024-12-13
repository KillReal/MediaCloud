using DynamicExpression.Extensions;
using MediaCloud.Builders.List;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.UserProvider;
using MediaCloud.WebApp.Services.Statistic;
using NLog;

namespace MediaCloud.Repositories
{
    public class CollectionRepository(AppDbContext context, StatisticProvider statisticProvider, IUserProvider actorProvider) : BaseRepository<Collection>(context, statisticProvider, LogManager.GetLogger("CollectionRepository"), actorProvider)
    {
        public override Collection? Get(Guid id)
        {
            var collection = _context.Collections.Find(id);

            if (collection == null || collection.CreatorId != _actor.Id)
            {
                return null;
            }

            return collection;
        }

        public async Task<int> GetListCount(Guid id)
        {
            var collection = await _context.Collections.FindAsync(id);

            if (collection == null || collection.CreatorId != _actor.Id)
            {
                return 0;
            }

            return collection.Count;
        }

        public List<Preview> GetList(Guid id, ListRequest listRequest)
        {
            var collection = _context.Collections.Find(id);

            if (collection == null || collection.CreatorId != _actor.Id)
            {
                return [];
            }

            _statisticProvider.ActivityFactorRaised.Invoke();

            return [.. _context.Previews.Where(x => x.Collection == collection)
                                    .OrderBy(x => x.Order)
                                    .Skip(listRequest.Offset)
                                    .Take(listRequest.Count)];
        }

        public long GetSize(Guid id)
        {
            var collection = _context.Collections.Find(id);

            if (collection == null || collection.CreatorId != _actor.Id)
            {
                return 0;
            }

            return _context.Blobs.Where(x => collection.Previews.Contains(x.Preview)).Sum(x => x.Size);
        }

        public bool TryUpdateOrder(Guid id, List<int> orders)
        {
            var collection = Get(id);

            if (collection == null)
            {
                return false;
            }

            var previews = collection.Previews.OrderBy(x => x.Order).ToList();

            var tags = new List<Tag>();

            for (var i = 0; i < orders.Count; i++)
            {
                previews[i].Order = orders[i];
            }

            _context.Previews.UpdateRange(previews);
            Update(collection);

            _logger.Info("Updated previews order for collection with id: {collection.Id} by: {_actor.Name}",
                collection.Id, _actor.Name);
            return true;
        }

        public override bool TryRemove(Guid id)
        {
            var collection = _context.Collections.Find(id);

            if (collection == null)
            {
                return false;
            }

            var collectionId = collection.Id;
            var blobs = collection.Previews.Select(x => x.Blob).ToList();
            var count = blobs.Count;
            var size = blobs.Sum(x => x.Size);

            _context.Blobs.RemoveRange(blobs);
            _statisticProvider.MediasCountChanged(-count, -size);

            Remove(collection);

            return true;
        }
    }
}
