using MediaCloud.Data.Models.Interfaces;
using MediaCloud.WebApp.Data.Models.Interfaces;

namespace MediaCloud.WebApp.Repositories.Base
{
    public class BlobNameFilter<T>(string searchPhrase) where T : IBlobNameSearchable
    {
        public IQueryable<T> ApplyToQuery(IQueryable<T> query)
        {
            return query.Where(x => x.BlobName.Contains(searchPhrase));
        }
    }
}
