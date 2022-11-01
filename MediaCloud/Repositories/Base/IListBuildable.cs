using MediaCloud.Builders.List;
using MediaCloud.Data.Models;

namespace MediaCloud.Repositories
{
    public interface IListBuildable<T> where T : Entity
    {
        public List<T> GetList(ListBuilder<T> listBuilder);
        public Task<int> GetListCountAsync(ListBuilder<T> listBuilder);
    }
}
