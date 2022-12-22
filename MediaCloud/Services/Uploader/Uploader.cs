using MediaCloud.Data;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.Repository;
using Task = MediaCloud.MediaUploader.Tasks.Task;

namespace MediaCloud.MediaUploader
{
    public class Uploader : IUploader
    {
        public Guid AddTask(Task task)
        {
            Queue.AddTask(task);
            Scheduler.Run();

            return task.Id;
        }

        public UploaderStatus GetStatus() => new();

        public UploaderTaskStatus GetStatus(Guid taskId) => new(taskId);
    }
}