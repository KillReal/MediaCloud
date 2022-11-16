using DynamicExpression.Entities;
using DynamicExpression.Extensions;
using DynamicExpression.Interfaces;
using MediaCloud.Builders.Components;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.Services;

namespace MediaCloud.Builders.List
{
    public class ListBuilder<T> where T : Entity
    {
        private Sorting Sorting { get; set; }

        private Filtering Filtering { get; set; }

        private Components.Pagination Pagination { get; set; }

        public int ColumnCount { get; set; } = 4;

        public ListBuilder(ListRequest request)
        {
            Sorting = new Sorting(request.Sort ?? "UpdatedAtDesc");

            Filtering = new Filtering((request.Filter ?? "").ToLower());

            Pagination = new Components.Pagination(request.Count == 0 
                ? 40
                : request.Count, 
                request.Offset);
        }

        public string Filter => Filtering.Filter;

        public string Sort => Sorting.PropertyName;

        public Ordering Order => Sorting.GetOrder();

        public int Count => Pagination.Count;

        public int TotalCount => Pagination.TotalCount;

        public int Offset => Pagination.Offset;

        public List<T> Build(IListBuildable<T> repository)
        {
            Pagination.TotalCount = repository.GetListCountAsync(this).Result;

            return repository.GetList(this);
        }
    }
}
