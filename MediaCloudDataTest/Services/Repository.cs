using MediaCloud.Data;
using MediaCloud.Data.Models;

namespace MediaCloud.Services
{
    public class Repository<T> where T : Entity
    {
        protected AppDbContext _context;

        public Repository(AppDbContext context)
        {
            _context = context;
        }

        public void Update(T media)
        {
            _context.Update(media);
            _context.SaveChanges();
        }

        public void Update(List<T> medias)
        {
            _context.UpdateRange(medias);
            _context.SaveChanges();
        }

        public void Remove(T media)
        {
            _context.Remove(media);
            _context.SaveChanges();
        }

        public void Remove(List<T> medias)
        {
            _context.RemoveRange(medias);
            _context.SaveChanges();
        }
    }
}
