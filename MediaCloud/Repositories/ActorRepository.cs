using DynamicExpression.Extensions;
using MediaCloud.Builders.List;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace MediaCloud.Repositories
{
    public class ActorRepository 
    {
        private AppDbContext _context;
        private Guid CurrentActorId;

        public ActorRepository(AppDbContext context)
        {
            _context = context;
            // DEV
            CurrentActorId = Guid.Empty;
            if (_context.Actors.First(x => x.Id == CurrentActorId) == null)
            {
                _context.Actors.Add(new Actor { Id = Guid.Empty, Name = "Initial Admin" });
            }
        }

        public Actor GetCurrent()
        {
            return Get(CurrentActorId);
        }

        public Actor Get(Guid id)
        {
            return _context.Actors.Find(id) ?? new();
        }
    }
}
