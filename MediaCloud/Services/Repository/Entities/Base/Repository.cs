﻿using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.DataService.Entities.Base;
using MediaCloud.WebApp.Services.Statistic;

namespace MediaCloud.Repositories
{
    public class DataService<T> where T : Entity
    {
        protected IStatisticService _statisticService;
        protected AppDbContext _context;
        protected ILogger _logger;
        protected Guid _actorId;

        public DataService(DataServiceContext DataServiceContext)
        {
            _statisticService = DataServiceContext.StatisticService;
            _context = DataServiceContext.DbContext;
            _logger = DataServiceContext.Logger;
            _actorId = DataServiceContext.Actor == null
                ? Guid.Empty
                : DataServiceContext.Actor.Id;
        }

        public virtual T? Get(Guid id)
        {
            var entity = _context.Find<T>(id);

            if (entity == null || entity.CreatorId != _actorId)
            {
                return null;
            }

            return entity;
        }

        public virtual bool TryRemove(Guid id)
        {
            var entity = Get(id);

            if (entity == null || entity.CreatorId != _actorId)
            {
                return false;
            }

            if (entity != null)
            {
                var entityId = entity.Id;
                var entityName = entity.GetType().Name.ToLower();

                Remove(entity);
                _logger.LogInformation($"Removed {entityName} with id: {entityId} by: {_actorId}");
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

            _logger.LogInformation($"Updated {entityName} with id: {entityId} by: {_actorId}");
        }

        public virtual void Update(List<T> entities)
        {
            var entityCount = entities.Count;
            var entityName = entities.First().GetType().Name.ToLower();

            _context.UpdateRange(entities);
            SaveChanges();

            _logger.LogInformation($"Updated <{entityCount}> {entityName} by: {_actorId}");
        }

        public virtual void Remove(T entity)
        {
            var entityId = entity.Id;
            var entityName = entity.GetType().Name.ToLower();

            _context.Remove(entity);
            SaveChanges();
            _logger.LogInformation($"Removed {entityName} with id: {entityId} by: {_actorId}");
        }

        public virtual void Remove(List<T> entities)
        {
            var entityCount = entities.Count;
            var entityName = entities.First().GetType().Name.ToLower();

            _context.RemoveRange(entities);
            SaveChanges();
            _logger.LogInformation($"Removed <{entityCount}> {entityName} by: {_actorId}");
        }

        public virtual void SaveChanges() => _context.SaveChanges();
    }
}
