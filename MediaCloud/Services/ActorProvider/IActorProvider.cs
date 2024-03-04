using MediaCloud.Data;
using MediaCloud.Data.Models;

namespace MediaCloud.WebApp.Services.ActorProvider
{
    public interface IActorProvider
    {
        public Actor? GetCurrent();
        public Actor? GetCurrent(AppDbContext context);
    }
}
