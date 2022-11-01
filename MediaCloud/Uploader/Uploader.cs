using MediaCloud.Data;
using MediaCloud.Repositories;
using MediaCloud.MediaUploader.Tasks;

namespace MediaCloud.MediaUploader
{
    public static class Uploader
    {
        internal static MediaRepository MediaRepository;
        internal static TagRepository TagRepository;

        public static void InitRepositories(AppDbContext context)
        {
            MediaRepository = new(context);
            TagRepository = new(context);

            Scheduler.InitWorkers();
        }

        public static Guid AddTask(UploadTask task)
        {
            if (MediaRepository == null || TagRepository == null)
            {
                throw new InvalidOperationException($"{nameof(Uploader)} doesn't initialized. Use InitRepositories for that.");
            }

            Queue.AddTask(task);
            Scheduler.Run();

            return task.Id;
        }

        public static UploaderStatus GetStatus()
        {
            return new();
        }

        public static UploaderTaskStatus GetStatus(Guid taskId)
        {
            return new(taskId);
        }
    }
}