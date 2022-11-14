using MediaCloud.Data.Models;

namespace MediaCloud.WebApp.Repositories.Base
{
    public class ReviewTagFilter
    {
        public List<Guid> PositiveTagIds { get; set; }

        public List<Guid> NegativeTagIds { get; set; }

        public ReviewTagFilter(List<Guid> positiveTagIds, List<Guid> negativeTagIds)
        {
            PositiveTagIds = positiveTagIds;
            NegativeTagIds = negativeTagIds;
        }

        public IQueryable<Preview> GetQuery(IQueryable<Preview> query)
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
