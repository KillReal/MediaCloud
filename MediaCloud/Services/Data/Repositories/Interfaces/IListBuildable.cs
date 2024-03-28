using MediaCloud.Builders.List;
using MediaCloud.Data.Models;

namespace MediaCloud.WebApp.Services.Data.Repositories.Interfaces
{
    public interface IListBuildable<T> where T : Record
    {
        public List<T> GetList(ListBuilder<T> listBuilder);
        public Task<int> GetListCountAsync(ListBuilder<T> listBuilder);
    }
}
