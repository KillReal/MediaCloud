using MediaCloud.Data.Models;

namespace MediaCloud.WebApp.Services
{
    public interface IActorProvider
    {
        public Actor? GetCurrent();
    }
}
