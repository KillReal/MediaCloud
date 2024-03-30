using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.DataService.Entities.Base;
using MediaCloud.WebApp.Services.Statistic;
using NLog;
using ILogger = NLog.ILogger;

namespace MediaCloud.Repositories
{
    public class BaseRepository<T> where T : Entity
    {
        protected StatisticProvider _statisticProvider;
        protected AppDbContext _context;
        protected ILogger _logger;
        protected Actor _actor;

        public BaseRepository(RepositoryContext repositoriesContext)
        {
            _statisticProvider = repositoriesContext.StatisticProvider;
            _context = repositoriesContext.DbContext;
            _logger = repositoriesContext.Logger;

            if (repositoriesContext.Actor == null)
            {
                throw new ArgumentException("Cannot initialize repository with unknown actor context");
            }

            _actor = repositoriesContext.Actor;
        }

        public virtual T? Get(Guid id)
        {
            var entity = _context.Find<T>(id);

            if (entity == null || entity.CreatorId != _actor.Id)
            {
                return null;
            }

            return entity;
        }

        public virtual bool TryRemove(Guid id)
        {
            var entity = Get(id);

            if (entity == null || entity.CreatorId != _actor.Id)
            {
                return false;
            }

            if (entity != null)
            {
                var entityId = entity.Id;
                var entityName = entity.GetType().Name.ToLower();

                Remove(entity);
                _logger.Info("Removed {entityName} with id: {entityId} by: {_actor.Name}", entityName, entityId, _actor.Name);
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

            _logger.Info("Updated {entityName} with id: {entityId} by: {_actor.Name}", entityName, entityId, _actor.Name);
        }

        public virtual void Update(List<T> entities)
        {
            var entityCount = entities.Count;
            var entityName = entities.First().GetType().Name.ToLower();

            _context.UpdateRange(entities);
            SaveChanges();

            _logger.Info("Updated <{entityCount}> {entityName} by: {_actor.Name}", entityCount, entityName, _actor.Name);
        }

        public virtual void Remove(T entity)
        {
            var entityId = entity.Id;
            var entityName = entity.GetType().Name.ToLower();

            _context.Remove(entity);
            SaveChanges();

            _logger.Info("Removed {entityName} with id: {entityId} by: {_actor.Name}", entityName, entityId, _actor.Name);
        }

        public virtual void Remove(List<T> entities)
        {
            var entityCount = entities.Count;
            var entityName = entities.First().GetType().Name.ToLower();

            _context.RemoveRange(entities);
            SaveChanges();
            _logger.Info("Removed <{entityCount}> {entityName} by: {_actor.Name}", entityCount, entityName, _actor.Name);
        }

        public virtual void SaveChanges() => _context.SaveChanges();
    }
}
