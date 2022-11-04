using MediaCloud.Data;

namespace MediaCloud.MediaUploader
{
    public static class Scheduler
    {
        private static AppDbContext _context;
        private static List<Worker> _workers;

        public static int MaxWorkersCount = 1;

        public static int WorkersActive => _workers.Count(x => x.IsRunning);

        public static void Init(AppDbContext context)
        {
            _context = context;

            _workers = new List<Worker>();
            for (int i = 0; i < MaxWorkersCount; i++)
            {
                _workers.Add(new Worker());
            }
        }

        public static void Run()
        {
            if (_workers == null)
            {
                throw new Exception($"{nameof(_workers)} doesn't initialized. Use Init() for that.");
            }

            foreach (var worker in _workers)
            {
                if (worker.IsRunning == false)
                {
                    worker.Run();
                }
            }
        }

        public static bool IsTaskInProgress(Guid id) => _workers.FirstOrDefault(x => x.CurrentTask == id) != null;

        public static AppDbContext GetContext()
        {
            if (_context == null)
            {
                throw new InvalidOperationException($"{nameof(_context)} doesn't initialized. Use Init().");
            }

            return _context;
        }
    }
}
