using DynamicExpression.Extensions;
using MediaCloud.Builders.List;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace MediaCloud.Services
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
        }

        public Actor GetCurrent()
        {
            return Get(CurrentActorId);
        }

        public Actor Get(Guid id)
        {
            var actor = _context.Actors.FirstOrDefault(x => x.Id == id);
            
            if (actor == null)
            {
                return new();
            }

            return actor;
        }
    }
}
