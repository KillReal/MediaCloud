using MediaCloud.Data;
using MediaCloud.Data.Models;

namespace MediaCloud.Repositories
{
    public class CollectionRepository : Repository<Collection>
    {
        public CollectionRepository(AppDbContext context) : base(context)
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

        public bool UpdateOrder(Guid id, List<int> orders)
        {
            var collection = Get(id);

            if (collection == null && collection.Previews.Count != orders.Count)
            {
                return false;
            }

            var previews = collection.Previews.OrderBy(x => x.Order)
                                              .ToList();

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

            if (!previews.Where(x => x.Order == 0).Any())
            {
                previews.First().Order = 0;
            }

            previews.First(x => x.Order == 0)
                                    .Tags.AddRange(tags);

            _context.Previews.UpdateRange(previews);
            _context.Collections.Update(collection);
            _context.SaveChanges();

            return true;
        }

        public override bool TryRemove(Guid id)
        {
            var collection = _context.Collections.Find(id);

            if (collection == null)
            {
                return false;
            }

            var medias = new List<Media>();
            foreach (var collectionPreview in collection.Previews)
            {
                medias.Add(collectionPreview.Media);
            }

            _context.Medias.RemoveRange(medias);
            _context.Collections.Remove(collection);
            _context.SaveChanges();
            
            return true;
        }
    }
}
