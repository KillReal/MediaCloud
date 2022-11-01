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

        public string Filter
        {
            get => Filtering.Filter;
        }

        public string Sort
        {
            get => Sorting.PropertyName;
        }

        public Ordering Order
        {
            get => Sorting.GetOrder();
        }

        public int Count
        {
            get => Pagination.Count;
        }

        public int TotalCount
        {
            get => Pagination.TotalCount;
        }

        public int Offset
        {
            get => Pagination.Offset;
        }

        public List<T> Build(IListBuildable<T> repository)
        {
            Pagination.TotalCount = repository.GetListCountAsync(this).Result;

            return repository.GetList(this);
        }
    }
}
