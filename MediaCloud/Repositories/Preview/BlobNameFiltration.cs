using System.Linq.Expressions;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Data.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using NLog.Config;

namespace MediaCloud.WebApp.Repositories
{
    public class BlobNameFiltration<T> where T : IBlobNameSearchable
    {
        private readonly string _searchName;
        
        public BlobNameFiltration(string filter, DbSet<Tag> tagsDbSet, List<string> customAliases)
        {
            _searchName = filter.Split(' ', StringSplitOptions.RemoveEmptyEntries).First();

            if (tagsDbSet.Any(x => x.Name.ToLower() == _searchName.ToLower()) || customAliases.Contains(_searchName))
            {
                _searchName = string.Empty;
            }
        }
        
        public bool IsValid() => _searchName != string.Empty;
        
        public Expression<Func<T, bool>> GetExpression()
        {
            return x => x.BlobName.ToLower().Contains(_searchName);
        }
    }
}
