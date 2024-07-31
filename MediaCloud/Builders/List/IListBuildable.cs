using MediaCloud.Builders.List;
using MediaCloud.Data.Models;

namespace MediaCloud.WebApp.Services.Data.Repositories.Interfaces
{
    public interface IListBuildable<T> where T : Record
    {
        public Task<List<T>> GetListAsync(ListBuilder<T> listBuilder);
        public Task<int> GetListCountAsync(ListBuilder<T> listBuilder);
    }
}
