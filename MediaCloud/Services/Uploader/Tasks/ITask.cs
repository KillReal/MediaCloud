using MediaCloud.Data;
using MediaCloud.WebApp.Services.Repository;

namespace MediaCloud.MediaUploader.Tasks
{
    public interface ITask
    {
        public int GetWorkCount();

        public void DoTheTask(IRepository repository);
    }
}
