using MediaCloud.Data;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.Repository;

namespace MediaCloud.MediaUploader
{
    public static class Scheduler
    {
        private static IRepository _repository;
        private static ILogger _logger;
        private static List<Worker> _workers = new();

        public static int MaxWorkersCount = 1;

        public static int WorkersActive => _workers.Count(x => x.IsRunning);

        public static void LazyInit(IRepository repository, ILogger<Uploader> logger)
        {
            _repository = repository;
            _logger = logger;
            _workers = new List<Worker>();
            for (int i = 0; i < MaxWorkersCount; i++)
            {
                _workers.Add(new Worker());
            }
        }

        public static void Run()
        {
            if (_workers.Any() == false)
            {
                throw new Exception($"{nameof(_workers)} doesn't initialized. Use Init() for that.");
            }

            _workers.Where(x => x.IsRunning == false).ToList().ForEach(x => x.Run());
        }

        public static bool IsTaskInProgress(Guid id) => _workers.FirstOrDefault(x => x.CurrentTask == id) != null;

        public static IRepository GetRepository()
        {
            if (_repository == null)
            {
                throw new InvalidOperationException($"{nameof(_repository)} doesn't initialized. Use Init().");
            }

            return _repository;
        }

        public static ILogger GetLogger()
        {
            if (_logger == null)
            {
                throw new InvalidOperationException($"{nameof(_logger)} doesn't initialized. Use Init().");
            }

            return _logger;
        }
    }
}
