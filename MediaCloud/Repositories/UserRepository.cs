using DynamicExpression.Extensions;
using MediaCloud.Builders.List;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediaCloud.Repositories
{
    public class UserRepository(AppDbContext context) : IListBuildable<User>
    {
        private readonly AppDbContext _context = context;

        public bool Create(User actor)
        {
            if (actor == null)
            {
                return false;
            }

            try
            {
                _context.Users?.Add(actor);
                _context.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public User? Get(string? actorName) 
            => _context.Users.FirstOrDefault(x => x.Name == actorName);

        public User Get(Guid id) 
            => _context.Users.Find(id) ?? new();

        public void Update(User actor)
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

            _context.Users.Remove(actor);
            _context.SaveChanges();

            return true;
        }

        public async Task<List<User>> GetListAsync(ListBuilder<User> listBuilder)
        {
            return await _context.Users.AsNoTracking().Where(x => x.Name.ToLower().Contains(listBuilder.Filtering.Filter.ToLower()))
                                                    .Order(listBuilder.Sorting.GetOrder())
                                                    .Skip(listBuilder.Pagination.Offset)
                                                    .Take(listBuilder.Pagination.Count)
                                                    .ToListAsync();
        }

        public async Task<int> GetListCountAsync(ListBuilder<User> listBuilder)
            => await _context.Users.AsNoTracking()
                .Where(x => x.Name.ToLower().Contains(listBuilder.Filtering.Filter.ToLower()))
                .CountAsync();
    }
}
