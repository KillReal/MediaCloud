using MediaCloud.Data;
using MediaCloud.Data.Models;

namespace MediaCloud.Repositories
{
    public class Repository<T> where T : Entity
    {
        protected AppDbContext _context;

        public Repository(AppDbContext context) => _context = context;

        public virtual T? Get(Guid id) => _context.Find<T>(id);

        public virtual bool TryRemove(Guid id)
        {
            var entity = Get(id);

            if (entity != null)
            {
                Remove(entity);
                return true;
            }

            return false;
        }

        public virtual void Update(T media) => _context.Update(media);

        public virtual void Update(List<T> medias) => _context.UpdateRange(medias);

        public virtual void Remove(T media) => _context.Remove(media);

        public virtual void Remove(List<T> medias) => _context.RemoveRange(medias);

        public virtual void SaveChanges() => _context.SaveChanges();
    }
}
