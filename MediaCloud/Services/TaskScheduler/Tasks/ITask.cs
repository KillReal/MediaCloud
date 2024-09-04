using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.UserProvider;

namespace MediaCloud.TaskScheduler.Tasks
{
    public interface ITask
    {
        public int GetWorkCount();
        public User GetAuthor();

        public void DoTheTask(IServiceProvider serviceProvider, IUserProvider actorProvider);
    }
}
