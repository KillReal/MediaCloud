using MediaCloud.Data.Models.Interfaces;
using MediaCloud.WebApp.Data.Models.Interfaces;

namespace MediaCloud.WebApp.Repositories.Base
{
    public class TagFilter<T>(List<Guid> positiveTagIds, List<Guid> negativeTagIds) where T : ITaggable
    {
        public List<Guid> PositiveTagIds { get; set; } = positiveTagIds;

        public List<Guid> NegativeTagIds { get; set; } = negativeTagIds;

        public IQueryable<T> ApplyToQuery(IQueryable<T> query)
        {
            if (PositiveTagIds.Any() || NegativeTagIds.Any())
            {
                return query.Where(x => (x.Tags.Concat(x.Collection.Previews.Where(x => x.Order != 0)
                    .SelectMany(z => z.Tags))
                        .Distinct()
                        .Where(y => PositiveTagIds.Contains(y.Id))
                        .Count() == PositiveTagIds.Count
                )
                && (x.Tags.Concat(x.Collection.Previews.Where(x => x.Order != 0)
                    .SelectMany(z => z.Tags))
                        .Distinct()
                        .Where(y => NegativeTagIds.Contains(y.Id))
                        .Any() == false)
                );
            }

            return query;
        }
    }
}
