using MediaCloud.Data.Models;
using MediaCloud.WebApp.Data.Models.Interfaces;

namespace MediaCloud.WebApp.Repositories.Base
{
    public class TagFilter<T> where T : ITaggable
    {
        public List<Guid> PositiveTagIds { get; set; }

        public List<Guid> NegativeTagIds { get; set; }

        public TagFilter(List<Guid> positiveTagIds, List<Guid> negativeTagIds)
        {
            PositiveTagIds = positiveTagIds;
            NegativeTagIds = negativeTagIds;
        }

        public IQueryable<T> GetQuery(IQueryable<T> query)
        {
            if (PositiveTagIds.Any() || NegativeTagIds.Any())
            {
                return query.Where(x => (x.Tags.Where(y => PositiveTagIds.Contains(y.Id))
                                               .Count() == PositiveTagIds.Count)
                                     && (x.Tags.Where(y => NegativeTagIds.Contains(y.Id))
                                               .Any() == false));
            }

            return query;
        }
    }
}
