using MediaCloud.Builders.List;
using MediaCloud.Data.Models;

namespace MediaCloud.Services
{
    public interface IRepository<T> where T : Entity
    {
        public List<T> GetList(ListBuilder<T> listBuilder);
        public int GetListCount(ListBuilder<T> listBuilder);
    }
}
