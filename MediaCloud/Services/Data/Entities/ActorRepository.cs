using DynamicExpression.Extensions;
using MediaCloud.Builders.List;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MediaCloud.Repositories
{
    public class ActorDataService : IListBuildable<Actor>
    {
        private readonly AppDbContext _context;

        public ActorDataService(AppDbContext context)
        {
            _context = context;
        }


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

        public void SetLastLoginAt(Actor actor, DateTime dateTime)
        {
            actor.LastLoginAt = dateTime;
            Update(actor);
        }

        public Actor? Get(string? actorName) 
            => _context.Actors.FirstOrDefault(x => x.Name == actorName);

        public Actor Get(Guid id) 
            => _context.Actors.Find(id) ?? new();

        public Actor? GetByInviteCode(string inviteCode)
            => _context.Actors.FirstOrDefault(x => x.InviteCode == inviteCode && x.IsActivated == false);

        public bool IsNameFree(string actorName)
            => _context.Actors.Any(x => x.Name == actorName) == false;

        public Actor? GetByAuthData(AuthData data)
        {
            var actor = _context.Actors.FirstOrDefault(x => x.Name == data.Name && x.IsActivated);

            if (actor == null || actor.PasswordHash == null || SecureHash.Verify(data.Password, actor.PasswordHash) == false)
            {
                return null;
            }

            actor.UpdatedAt = actor.UpdatedAt.ToUniversalTime();
            actor.CreatedAt = actor.CreatedAt.ToUniversalTime();
            actor.LastLoginAt = actor.LastLoginAt.ToUniversalTime();

            return actor;
        }

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
            return _context.Actors.AsNoTracking().Order(listBuilder.Order)
                                                 .Skip(listBuilder.Offset)
                                                 .Take(listBuilder.Count)
                                                 .ToList();
        }

        public async Task<int> GetListCountAsync(ListBuilder<Actor> listBuilder)
            => await _context.Actors.AsNoTracking().CountAsync();
    }
}
