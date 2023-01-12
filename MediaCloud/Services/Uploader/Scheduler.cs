using MediaCloud.Data;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.Repository;

namespace MediaCloud.MediaUploader
{
    public static class Scheduler
    {
        private static IRepository Repository;
        private static ILogger Logger;
        private static List<Worker> Workers = new();

        public static int MaxWorkersCount = 1;

        public static int WorkersActive => Workers.Count(x => x.IsRunning);

        public static void Init(IRepository repository, ILogger<Uploader> logger)
        {
            Repository = repository;
            Logger = logger;
            Workers = new List<Worker>();
            for (int i = 0; i < MaxWorkersCount; i++)
            {
                Workers.Add(new Worker());
            }
        }

        public static void Run()
        {
            if (Workers.Any() == false)
            {
                throw new Exception($"{nameof(Workers)} doesn't initialized. Use Init() for that.");
            }

            Workers.Where(x => x.IsRunning == false).ToList().ForEach(x => x.Run());
        }

        public static bool IsTaskInProgress(Guid id) => Workers.FirstOrDefault(x => x.CurrentTask == id) != null;

        public static IRepository GetRepository()
        {
            if (Repository == null)
            {
                throw new InvalidOperationException($"{nameof(Repository)} doesn't initialized. Use Init().");
            }

            return Repository;
        }

        public static ILogger GetLogger()
        {
            if (Logger == null)
            {
                throw new InvalidOperationException($"{nameof(Logger)} doesn't initialized. Use Init().");
            }

            return Logger;
        }
    }
}
