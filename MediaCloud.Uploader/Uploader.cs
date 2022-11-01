using MediaCloud.Data;
using MediaCloud.Repositories;
using MediaCloud.Uploader.Tasks;

namespace MediaCloud.Uploader
{
    public static class Uploader
    {
        internal static MediaRepository MediaRepository;
        internal static TagRepository TagRepository;

        public static void InitRepositories(MediaRepository mediaRepository, TagRepository tagRepository)
        {
            MediaRepository = mediaRepository;
            TagRepository = tagRepository;
        }

        public static void AddTask(UploadTask task)
        {
            Queue.AddTask(task);

            if (Scheduler.IsRunning)
            {
                Scheduler.Run();
            }
        }

        public static UploaderStatus GetStatus()
        {
            return new();
        }
    }
}