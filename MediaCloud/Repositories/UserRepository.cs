using DynamicExpression.Extensions;
using MediaCloud.Builders.List;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.Data.Repositories.Interfaces;
using MediaCloud.WebApp.Services.UserProvider;
using Microsoft.EntityFrameworkCore;

namespace MediaCloud.Repositories
{
    public class UserRepository(AppDbContext context) : IListBuildable<User>
    {
        private readonly AppDbContext _context = context;

        public bool Create(User user)
        {
            try
            {
                _context.Users.Add(user);
                _context.SaveChangesAsync();

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
            => _context.Users.Find(id) ?? new User();

        public void Update(User user)
        {
            _context.Update(user);
            _context.SaveChangesAsync();
        }

        public bool TryRemove(Guid id)
        {
            var user = Get(id);

            _context.Users.Remove(user);
            _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<User>> GetListAsync(ListBuilder<User> listBuilder)
        {
            return await _context.Users.AsNoTracking().Where(x => x.Name != null && x.Name.ToLower().Contains(listBuilder.Filtering.Filter.ToLower()))
                                                    .Order(listBuilder.Sorting.GetOrder())
                                                    .Skip(listBuilder.Pagination.Offset)
                                                    .Take(listBuilder.Pagination.Count)
                                                    .ToListAsync();
        }

        public async Task<int> GetListCountAsync(ListBuilder<User> listBuilder)
            => await _context.Users.AsNoTracking()
                .Where(x => x.Name != null && x.Name.ToLower().Contains(listBuilder.Filtering.Filter.ToLower()))
                .CountAsync();
    }
}
