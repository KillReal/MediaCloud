using DynamicExpression.Entities;
using DynamicExpression.Extensions;
using DynamicExpression.Interfaces;
using MediaCloud.Builders.Components;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.Services;
using MediaCloud.WebApp.Services;

namespace MediaCloud.Builders.List
{
    public class ListBuilder<T> where T : Record
    {
        private Sorting Sorting { get; set; }

        private Filtering Filtering { get; set; }

        private Components.Pagination Pagination { get; set; }

        public int ColumnCount { get; set; }

        public ListBuilder(ListRequest request)
        {
            Sorting = new Sorting(request.Sort ?? "UpdatedAtDesc");

            Filtering = new Filtering((request.Filter ?? "").ToLower());

            Pagination = new Components.Pagination(request.Count == 0 
                ? ConfigurationService.List.GetEntityMaxCount()
                : request.Count, 
                request.Offset,
                ConfigurationService.List.GetShowedPagesMaxCount());

            ColumnCount = ConfigurationService.Gallery.GetColumnCount();
        }

        public string Filter => Filtering.Filter;

        public string Sort => Sorting.PropertyName;

        public Ordering Order => Sorting.GetOrder();

        public int Count => Pagination.Count;

        public int TotalCount => Pagination.TotalCount;

        public int Offset => Pagination.Offset;

        public int StartPageNumber => Pagination.StartPageNumber;
        public int CurrentPageNumber => Pagination.CurrentPageNumber;
        public int LastPageNumber => Pagination.EndPageNumber;

        public List<T> Build(IListBuildable<T> repository)
        {
            Pagination.SetTotalCount(repository.GetListCountAsync(this).Result);

            return repository.GetList(this);
        }
    }
}
