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
    /// <summary>
    /// List builder of entities with sortings, filtering and pagination.
    /// </summary>
    /// <typeparam name="T"><see cref="Record"/></typeparam>
    public class ListBuilder<T> where T : Record
    {
        private Sorting Sorting { get; set; }

        private Filtering Filtering { get; set; }

        private Components.Pagination Pagination { get; set; }

        /// <summary>
        /// Column count for list. Default value is <see cref="ConfigurationService.Gallery.GetColumnCount"/>
        /// </summary>
        public int ColumnCount { get; set; }

        /// <summary>
        /// Filter parameter.
        /// </summary>
        public string Filter => Filtering.Filter;

        /// <summary>
        /// Sort entity by property name.
        /// </summary>
        public string Sort => Sorting.PropertyName;

        /// <summary>
        /// Ordering of query of sorting.
        /// </summary>
        public Ordering Order => Sorting.GetOrder();

        /// <summary>
        /// Entities count on single page.
        /// </summary>
        public int Count => Pagination.Count;

        /// <summary>
        /// Total entities count.
        /// </summary>
        public int TotalCount => Pagination.TotalCount;

        /// <summary>
        /// Entities offset in query.
        /// </summary>
        public int Offset => Pagination.Offset;

        /// <summary>
        /// Start page number.
        /// </summary>
        public int StartPageNumber => Pagination.StartPageNumber;

        /// <summary>
        /// Current page number.
        /// </summary>
        public int CurrentPageNumber => Pagination.CurrentPageNumber;

        /// <summary>
        /// Last page number.
        /// </summary>
        public int LastPageNumber => Pagination.EndPageNumber;

        /// <summary>
        /// Init list builder via list request.
        /// </summary>
        /// <param name="request">List request.</param>
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

        /// <summary>
        /// Build the entity list by <see cref="Repository{T}"/> querying from db.
        /// </summary>
        /// <param name="repository">Repository instance of entities.</param>
        /// <returns>List of selected entities.</returns>
        public List<T> Build(IListBuildable<T> repository)
        {
            Pagination.SetTotalCount(repository.GetListCountAsync(this).Result);

            return repository.GetList(this);
        }
    }
}
