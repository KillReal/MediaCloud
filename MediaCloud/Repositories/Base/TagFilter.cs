using System.Linq.Expressions;
using DynamicExpression.Entities;
using MediaCloud.Data.Models.Interfaces;
using MediaCloud.WebApp.Data.Models.Interfaces;

namespace MediaCloud.WebApp.Repositories.Base
{
    public class TagFilter<T>(List<Guid> positiveTagIds, List<Guid> negativeTagIds) where T : ITaggable, IBlobNameSearchable
    {
        public List<Guid> PositiveTagIds { get; set; } = positiveTagIds;

        public List<Guid> NegativeTagIds { get; set; } = negativeTagIds;

        public Expression<Func<T, bool>> GetExpression()
        {
            return x => (x.Tags.Concat(x.Collection.Previews.Where(x => x.Order != 0)
                                    .SelectMany(z => z.Tags))
                                .Distinct()
                                .Where(y => PositiveTagIds.Contains(y.Id))
                                .Count() == PositiveTagIds.Count && PositiveTagIds.Count > 0
                        )
                        && (x.Tags.Concat(x.Collection.Previews.Where(x => x.Order != 0)
                                .SelectMany(z => z.Tags))
                            .Distinct()
                            .Where(y => NegativeTagIds.Contains(y.Id))
                            .Any() == false);
        }
    }
}
