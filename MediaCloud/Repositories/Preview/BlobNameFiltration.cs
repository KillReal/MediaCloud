using System.Linq.Expressions;
using MediaCloud.WebApp.Data.Models.Interfaces;

namespace MediaCloud.WebApp.Repositories
{
    public class BlobNameFiltration<T> where T : IBlobNameSearchable
    {
        private readonly string _searchName;
        
        public BlobNameFiltration(string filter)
        {
            _searchName = filter.Split(' ', StringSplitOptions.RemoveEmptyEntries).First();
        }
        
        public Expression<Func<T, bool>> GetExpression()
        {
            return x => x.BlobName.ToLower().Contains(_searchName);
        }
    }
}
