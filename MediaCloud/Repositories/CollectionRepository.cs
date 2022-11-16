using MediaCloud.Data;
using MediaCloud.Data.Models;

namespace MediaCloud.Repositories
{
    public class CollectionRepository : Repository<Collection>
    {
        public CollectionRepository(AppDbContext context, ILogger logger)
            : base(context, logger)
        {
        }

        public override Collection? Get(Guid id)
        {
            var collection = _context.Collections.Find(id);

            if (collection == null)
            {
                return null;
            }

            collection.Previews = collection.Previews.OrderBy(x => x.Order)
                                                     .ToList();

            return collection;
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

            new PreviewRepository(_context, _logger).Update(previews);
            new CollectionRepository(_context, _logger).Update(collection);
            SaveChanges();

            _logger.LogInformation($"Updated previews order for collection with id: {collection.Id} by: {collection.Updator.Id}");
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

            new MediaRepository(_context, _logger).Remove(medias);
            new CollectionRepository(_context, _logger).Remove(collection);
            SaveChanges();

            return true;
        }
    }
}
