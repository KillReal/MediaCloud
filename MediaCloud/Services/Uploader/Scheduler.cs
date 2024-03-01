using MediaCloud.Data;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.Repository;

namespace MediaCloud.MediaUploader
{
    public class Scheduler
    {
        private IRepository Repository;
        private ILogger Logger;
        private List<Worker> Workers = new();

        public int MaxWorkersCount = 1;

        public int WorkersActive => Workers.Count(x => x.IsRunning);

        public Scheduler(IRepository repository, ILogger<Uploader> logger, Queue queue)
        {
            Repository = repository;
            Logger = logger;
            Workers = new List<Worker>();
            for (int i = 0; i < MaxWorkersCount; i++)
            {
                Workers.Add(new Worker(queue, this));
            }
        }

        public void Run()
        {
            if (Workers.Any() == false)
            {
                throw new Exception($"{nameof(Workers)} doesn't initialized. Use Init() for that.");
            }

            Workers.Where(x => x.IsRunning == false).ToList().ForEach(x => x.Run());
        }

        public bool IsTaskInProgress(Guid id) => Workers.FirstOrDefault(x => x.CurrentTask == id) != null;

        public IRepository GetRepository()
        {
            if (Repository == null)
            {
                throw new InvalidOperationException($"{nameof(Repository)} doesn't initialized. Use Init().");
            }

            return Repository;
        }

        public ILogger GetLogger()
        {
            if (Logger == null)
            {
                throw new InvalidOperationException($"{nameof(Logger)} doesn't initialized. Use Init().");
            }

            return Logger;
        }
    }
}
