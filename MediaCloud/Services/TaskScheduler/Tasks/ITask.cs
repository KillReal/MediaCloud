using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.ActorProvider;

namespace MediaCloud.TaskScheduler.Tasks
{
    public interface ITask
    {
        public int GetWorkCount();
        public Actor GetAuthor();

        public void DoTheTask(IServiceProvider serviceProvider, IActorProvider actorProvider);
    }
}
