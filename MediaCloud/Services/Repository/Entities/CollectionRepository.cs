using DynamicExpression.Extensions;
using MediaCloud.Builders.List;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Extensions;
using MediaCloud.WebApp.Services.Repository.Entities.Base;
using Microsoft.EntityFrameworkCore;

namespace MediaCloud.Repositories
{
    public class CollectionRepository : Repository<Collection>
    {
        public CollectionRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }

        public override Collection? Get(Guid id)
        {
            var collection = _context.Collections.Find(id);

            if (collection == null || collection.Creator.Id != _actorId)
            {
                return null;
            }

            return collection;
        }

        public Collection? GetTopBatch(Guid id, int count = 42)
        {
            var collection = _context.Collections.Find(id);

            if (collection == null || collection.Creator.Id != _actorId)
            {
                return null;
            }

            collection.Previews = collection.Previews.OrderBy(x => x.Order)
                                                     .Take(count)
                                                     .ToList();

            return collection;
        }

        public int GetListCount(Guid id)
        {
            var collection = _context.Collections.Find(id);

            if (collection == null || collection.Creator.Id != _actorId)
            {
                return 0;
            }

            return collection.Count;
        }

        public List<Preview> GetList(Guid id, ListRequest listRequest)
        {
            var collection = _context.Collections.Find(id);

            if (collection == null || collection.Creator.Id != _actorId)
            {
                return new();
            }

            return _context.Previews.Where(x => x.Collection == collection)
                                    .OrderBy(x => x.Order)
                                    .Skip(listRequest.Offset)
                                    .Take(listRequest.Count)
                                    .ToList();
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
            SaveChanges();

            _logger.LogInformation($"Updated previews order for collection with id: {collection.Id} by: {_actorId}");
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

            _context.Medias.RemoveRange(medias);
            Remove(collection);
            SaveChanges();

            return true;
        }
    }
}
