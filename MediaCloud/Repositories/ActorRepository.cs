using DynamicExpression.Extensions;
using MediaCloud.Builders.List;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MediaCloud.Repositories
{
    public class ActorRepository
    {
        private AppDbContext _context;

        public ActorRepository(AppDbContext context)
        {
            _context = context;

            // DEV
            if (_context.Actors.Count() == 0)
            {
                _context.Actors.Add(new Actor { Id = Guid.NewGuid(), Name = "superadmin", PasswordHash = "superadmin" });
                _context.SaveChanges();
            }
        }

        public Actor? Get(string? actorName) 
            => _context.Actors.FirstOrDefault(x => x.Name == actorName);

        public Actor Get(Guid id) 
            => _context.Actors.Find(id) ?? new();
    }
}
