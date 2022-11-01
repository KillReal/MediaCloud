using DynamicExpression.Extensions;
using DynamicExpression.Interfaces;
using MediaCloud.Builders.Components;
using MediaCloud.Data.Models;
using MediaCloud.Services;

namespace MediaCloud.Builders.List
{
    public class ListBuilder<T> where T : Entity
    {
        public Sorting Sorting { get; set; }

        public Filtering Filtering { get; set; }

        public Pagination Pagination { get; set; }

        public int ColumnCount { get; set; } = 4;

        public ListBuilder(ListRequest request)
        {
            Sorting = new Sorting(request.Sort);
            Filtering = new Filtering(request.Filter);
            Pagination = new Pagination(request.Count, request.Offset);
        }

        public List<T> Build(IRepository<T> repository)
        {
            var entities = repository.GetList(this);
            Pagination.TotalCount = repository.GetListCount(this);

            return entities;
        }
    }
}
