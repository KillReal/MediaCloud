using System.Linq.Expressions;
using MediaCloud.WebApp.Data.Models.Interfaces;

namespace MediaCloud.WebApp.Repositories.Base
{
    public class BlobNameFilter<T>(string searchPhrase) where T : IBlobNameSearchable
    {
        public Expression<Func<T, bool>> GetExpression()
        {
            return x => x.BlobName.Contains(searchPhrase);
        }
    }
}
