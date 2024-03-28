using DynamicExpression.Extensions;
using MediaCloud.Builders.List;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Extensions;
using MediaCloud.WebApp.Services.DataService.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace MediaCloud.Repositories
{
    public class CollectionRepository : BaseRepository<Collection>
    {
        public CollectionRepository(RepositoryContext context) : base(context)
        {
        }

        public override Collection? Get(Guid id)
        {
            var collection = _context.Collections.Find(id);

            if (collection == null || collection.CreatorId != _actorId)
            {
                return null;
            }

            return collection;
        }

        public async Task<int> GetListCount(Guid id)
        {
            var collection = await _context.Collections.FindAsync(id);

            if (collection == null || collection.CreatorId != _actorId)
            {
                return 0;
            }

            return collection.Count;
        }

        public List<Preview> GetList(Guid id, ListRequest listRequest)
        {
            var collection = _context.Collections.Find(id);

            if (collection == null || collection.CreatorId != _actorId)
            {
                return new();
            }

            _statisticService.ActivityFactorRaised.Invoke();

            return _context.Previews.Where(x => x.Collection == collection)
                                    .OrderBy(x => x.Order)
                                    .Skip(listRequest.Offset)
                                    .Take(listRequest.Count)
                                    .ToList();
        }

        public long GetSize(Guid id)
        {
            var collection = _context.Collections.Find(id);

            if (collection == null || collection.CreatorId != _actorId)
            {
                return 0;
            }

            return _context.Medias.Where(x => collection.Previews.Contains(x.Preview)).Sum(x => x.Size);
        }

        public bool TryUpdateOrder(Guid id, List<int> orders)
        {
            var collection = Get(id);

            if (collection == null || collection.Previews.Count != orders.Count)
            {
                return false;
            }

            var previews = collection.Previews.OrderBy(x => x.Order).ToList();

            var tags = new List<Tag>();

            for (var i = 0; i < previews.Count; i++)
            {
                previews[i].Order = orders[i];
                if (previews[i].Tags.Count > 0)
                {
                    tags = previews[i].Tags.ToList();
                    previews[i].Tags.Clear();
                }
            }

            if (previews.Where(x => x.Order == 0).Any() == false)
            {
                previews.First().Order = 0;
            }

            previews.First(x => x.Order == 0).Tags.AddRange(tags);

            _context.Previews.UpdateRange(previews);
            Update(collection);

            _logger.Info("Updated previews order for collection with id: {collection.Id} by: {_actorId}",
                collection.Id, _actorId);
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
            var medias = collection.Previews.Select(x => x.Media).ToList();
            var count = medias.Count;
            var size = medias.Sum(x => x.Size);

            _context.Medias.RemoveRange(medias);
            _statisticService.MediasCountChanged(-count, -size);

            Remove(collection);

            return true;
        }
    }
}
