using MediaCloud.Data;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services;
using MediaCloud.WebApp.Services.DataService;
using NLog;
using ILogger = NLog.ILogger;

namespace MediaCloud.MediaUploader
{
    /// <summary>
    /// Internal task scheduler.
    /// </summary>
    public class Scheduler
    {
        private readonly IDataService _dataService;
        private readonly ILogger _logger;
        private readonly List<Worker> _workers = new();
        private readonly Queue _queue;
        
        /// <summary>
        /// Total available workers.
        /// </summary>
        public int MaxWorkersCount { get; } = ConfigurationService.Uploader.GetWorkersCount();

        /// <summary>
        /// Logging task start event.
        /// </summary>
        public Action<Guid> OnTaskStarted { get; set; }
        /// <summary>
        /// Logging task completion event. Notify <see cref="Queue"/> about task completion and if <see cref="Queue"/> is not empty, tries to run free <see cref="Worker"/>.
        /// </summary>
        public Action<Guid> OnTaskCompleted { get; set; }
        /// <summary>
        /// Task error occured event.
        /// </summary>
        public Action<Guid, Exception> OnTaskErrorOccured { get; set; }

        public int WorkersActive => _workers.Count(x => x.IsReady == false);

        private void WorkerStartTask(Guid id)
        {
            _logger.Info("Worker ({WorkersActive}/{MaxWorkersCount}) processing the task: {id}",
                WorkersActive, MaxWorkersCount, id);
        }

        private void WorkerCompleteTask(Guid id)
        {
            _logger.Info("Worker ({WorkersActive - 1}/{MaxWorkersCount}) completed the task: {id}",
                WorkersActive - 1, MaxWorkersCount, id);

            _queue.OnTaskComplete.Invoke(id);
            if (_queue.IsEmpty == false)
            {
                Run();
            }
        }

        private void WorkerFacedErrorWithTask(Guid id, Exception ex)
        {
            _logger.Error("Worker faced error during processing of task: {id} exception: {ex}", id, ex);
        }

        /// <summary>
        /// Init internal scheduler.
        /// </summary>
        /// <param name="dataService"> Current dataService instance. </param>
        /// <param name="queue"> Current <see cref="Uploader"/> queue. </param>
        public Scheduler(IDataService dataService, Queue queue)
        {
            _dataService = dataService;
            _logger = LogManager.GetLogger("Uploader.Scheduler");
            _workers = new List<Worker>();
            _queue = queue;
            for (int i = 0; i < MaxWorkersCount; i++)
            {
                _workers.Add(new Worker(_queue, this, _dataService));
            }

            OnTaskStarted += WorkerStartTask;
            OnTaskCompleted += WorkerCompleteTask;
            OnTaskErrorOccured += WorkerFacedErrorWithTask;
        }

        /// <summary>
        /// Run scheduler. Scheduler check whether queue is empty, and if not tries to run one of free <see cref="Worker"/>.
        /// </summary>
        public void Run()
        {
            _logger.Info("Scheduler was triggered with queue size <{_queue.TaskCount}>", _queue.TaskCount);

            if (_queue.IsEmpty)
            {
                return;
            }

            var worker = _workers.Where(x => x.IsReady).FirstOrDefault();
            worker?.Run();
        }

        /// <summary>
        /// Check if certain task in <see cref="Worker"/> processing.
        /// </summary>
        /// <param name="id"> Task id. </param>
        /// <returns> True if task now processing by <see cref="Worker"/>. </returns>
        public bool IsTaskInProgress(Guid id) => _workers.Where(x => x.Task?.Id == id).Any();
    }
}
