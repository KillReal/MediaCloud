using MediaCloud.Data;
using MediaCloud.Data.Models;

namespace MediaCloud.Repositories
{
    public class Repository<T> where T : Entity
    {
        protected AppDbContext _context;
        protected ILogger _logger;

        public Repository(AppDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public virtual T? Get(Guid id) => _context.Find<T>(id);

        public virtual bool TryRemove(Guid id)
        {
            var entity = Get(id);

            var entityId = entity.Id;
            var entityName = entity.GetType().Name.ToLower();

            if (entity != null)
            {
                Remove(entity);
                _logger.LogInformation($"Removed {entityName} with id: {entityId} by: {new ActorRepository(_context).GetCurrent().Id}");
                return true;
            }

            return false;
        }

        public virtual void Update(T entity)
        {
            var entityId = entity.Id;
            var entityName = entity.GetType().Name.ToLower();

            _context.Update(entity);
            SaveChanges();

            _logger.LogInformation($"Updated {entityName} with id: {entityId} by: {new ActorRepository(_context).GetCurrent().Id}");
        }

        public virtual void Update(List<T> entities)
        {
            var entityCount = entities.Count;
            var entityName = entities.First().GetType().Name.ToLower();

            _context.UpdateRange(entities);
            SaveChanges();

            _logger.LogInformation($"Updated <{entityCount}> {entityName} by: {new ActorRepository(_context).GetCurrent().Id}");
        }

        public virtual void Remove(T entity)
        {
            var entityId = entity.Id;
            var entityName = entity.GetType().Name.ToLower();

            _context.Remove(entity);
            SaveChanges();
            _logger.LogInformation($"Removed {entityName} with id: {entityId} by: {new ActorRepository(_context).GetCurrent().Id}");
        }

        public virtual void Remove(List<T> entities)
        {
            var entityCount = entities.Count;
            var entityName = entities.First().GetType().Name.ToLower();

            _context.RemoveRange(entities);
            SaveChanges();
            _logger.LogInformation($"Removed <{entityCount}> {entityName} by: {new ActorRepository(_context).GetCurrent().Id}");
        }

        public virtual void SaveChanges() => _context.SaveChanges();
    }
}
