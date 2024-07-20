using DynamicExpression.Extensions;
using MediaCloud.Builders.List;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediaCloud.Repositories
{
    public class ActorRepository(AppDbContext context) : IListBuildable<Actor>
    {
        private readonly AppDbContext _context = context;

        public bool Create(Actor actor)
        {
            if (actor == null)
            {
                return false;
            }

            try
            {
                _context.Actors?.Add(actor);
                _context.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public Actor? Get(string? actorName) 
            => _context.Actors.FirstOrDefault(x => x.Name == actorName);

        public Actor Get(Guid id) 
            => _context.Actors.Find(id) ?? new();

        public void Update(Actor actor)
        {
            _context.Update(actor);
            _context.SaveChanges();
        }

        public bool TryRemove(Guid id)
        {
            var actor = Get(id);

            if (actor == null)
            {
                return false;
            }

            _context.Actors.Remove(actor);
            _context.SaveChanges();

            return true;
        }

        public List<Actor> GetList(ListBuilder<Actor> listBuilder)
        {
            return _context.Actors.AsNoTracking().Order(listBuilder.Sorting.GetOrder())
                                                 .Skip(listBuilder.Pagination.Offset)
                                                 .Take(listBuilder.Pagination.Count)
                                                 .ToList();
        }

        public async Task<int> GetListCountAsync(ListBuilder<Actor> listBuilder)
            => await _context.Actors.AsNoTracking().CountAsync();
    }
}
