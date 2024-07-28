using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Builders.List.Components;
using MediaCloud.WebApp.Services.ConfigProvider;
using MediaCloud.WebApp.Services.Data.Repositories.Interfaces;
using Pagination = MediaCloud.WebApp.Builders.List.Components.Pagination;

namespace MediaCloud.Builders.List
{
    /// <summary>
    /// List builder of entities with sortings, filtering and pagination.
    /// </summary>
    /// <typeparam name="T"><see cref="Record"/></typeparam>
    /// <remarks>
    /// Init list builder via list request.
    /// </remarks>
    /// <param name="request"> List request. </param>
    /// <param name="configProvider"> Config provider for current user <see cref="IDataService"/>. </param>
    public class ListBuilder<T>(ListRequest request, UserSettings actorSettings) where T : Record
    {
        public Sorting Sorting { get; init; } = new Sorting(request.Sort ?? "UpdatedAtDesc");

        public Filtering Filtering { get; init; } = new Filtering((request.Filter ?? "").ToLower());

        public Pagination Pagination { get; init; } = new Pagination(request.Count == 0
                ? actorSettings.ListMaxEntitiesCount
                : request.Count,
                request.Offset,
                actorSettings.ListMaxPageCount);

        /// <summary>
        /// Build the entity list by <see cref="BaseRepository{T}"/> querying from db.
        /// </summary>
        /// <param name="repository">DataService instance of entities.</param>
        /// <returns>List of selected entities.</returns>
        public async Task<List<T>> BuildAsync(IListBuildable<T> repository)
        {
            Pagination.SetTotalCount(await repository.GetListCountAsync(this));

            return repository.GetList(this);
        }
    }
}
