namespace MediaCloud.MediaUploader
{
    public static class Scheduler
    {
        private static List<Worker> _workers;

        public static int MaxWorkersCount = 1;

        public static int WorkersActive
        {
            get
            {
                var count = 0;

                foreach (var worker in _workers)
                {
                    if (worker.IsRunning)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public static void InitWorkers()
        {
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
                throw new Exception($"{nameof(_workers)} doesn't initialized. Use InitWorkers() for that.");
            }

            foreach (var worker in _workers)
            {
                if (worker.IsRunning == false)
                {
                    worker.Run();
                }
            }
        }

        public static bool IsTaskInProgress(Guid id)
        {
            if (id == Guid.Empty)
            {
                return false;
            }

            foreach (var worker in _workers)
            {
                if (worker.CurrentTask == id)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
