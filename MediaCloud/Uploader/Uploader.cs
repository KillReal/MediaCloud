using MediaCloud.Data;
using MediaCloud.Repositories;
using Task = MediaCloud.MediaUploader.Tasks.Task;

namespace MediaCloud.MediaUploader
{
    public static class Uploader
    {
        public static void Init(AppDbContext context) => Scheduler.Init(context);

        public static Guid AddTask(Task task)
        {
            Queue.AddTask(task);
            Scheduler.Run();

            return task.Id;
        }

        public static UploaderStatus GetStatus() => new();

        public static UploaderTaskStatus GetStatus(Guid taskId) => new(taskId);
    }
}