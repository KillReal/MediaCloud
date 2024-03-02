using MediaCloud.Data;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.DataService;

namespace MediaCloud.MediaUploader
{
    public class Scheduler
    {
        private readonly IDataService _dataService;
        private readonly ILogger _logger;
        private readonly List<Worker> _workers = new();

        public int MaxWorkersCount = 1;

        public int WorkersActive => _workers.Count(x => x.IsRunning);

        public Scheduler(IDataService dataService, ILogger<Uploader> logger, Queue queue)
        {
            _dataService = dataService;
            _logger = logger;
            _workers = new List<Worker>();
            for (int i = 0; i < MaxWorkersCount; i++)
            {
                _workers.Add(new Worker(queue, this));
            }
        }

        public void Run()
        {
            if (_workers.Any() == false)
            {
                throw new Exception($"{nameof(_workers)} doesn't initialized. Use Init() for that.");
            }

            _workers.Where(x => x.IsRunning == false).ToList().ForEach(x => x.Run());
        }

        public bool IsTaskInProgress(Guid id) => _workers.FirstOrDefault(x => x.CurrentTask == id) != null;

        public IDataService GetDataService()
        {
            if (_dataService == null)
            {
                throw new InvalidOperationException($"{nameof(_dataService)} doesn't initialized. Use Init().");
            }

            return _dataService;
        }

        public ILogger GetLogger()
        {
            if (_logger == null)
            {
                throw new InvalidOperationException($"{nameof(_logger)} doesn't initialized. Use Init().");
            }

            return _logger;
        }
    }
}
