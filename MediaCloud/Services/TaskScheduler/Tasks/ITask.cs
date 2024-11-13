using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.UserProvider;

namespace MediaCloud.TaskScheduler.Tasks
{
    public interface ITask
    {
        public int GetWorkCount();

        public void DoTheTask(IServiceProvider serviceProvider, IUserProvider actorProvider);
        User GetAuthor();
    }
}
