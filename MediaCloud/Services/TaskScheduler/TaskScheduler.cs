using MediaCloud.Data;
using MediaCloud.MediaUploader.Tasks;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services;
using MediaCloud.WebApp.Services.ConfigurationProvider;
using NLog;
using ILogger = NLog.ILogger;
using Task = MediaCloud.MediaUploader.Tasks.Task;

namespace MediaCloud.MediaUploader
{
    /// <summary>
    /// Internal task scheduler.
    /// </summary>
    public partial class TaskScheduler : ITaskScheduler
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger _logger;
        private readonly List<Worker> _workers = new();
        private readonly Queue _queue;
        
        public readonly int MaxWorkersCount;
        public int BusyWorkersCount => _workers.Count(x => x.IsReady == false);

        /// <summary>
        /// Init internal scheduler.
        /// </summary>
        /// <param name="dataService"> Current dataService instance. </param>
        /// <param name="queue"> Current <see cref="Uploader"/> queue. </param>
        public TaskScheduler(IServiceScopeFactory serviceScopeFactory, IConfigProvider configProvider)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = LogManager.GetLogger("Uploader.Scheduler");
            _workers = new List<Worker>();
            _queue = new Queue();
            MaxWorkersCount = configProvider.EnvironmentSettings.TaskSchedulerWorkerCount;
            for (int i = 0; i < MaxWorkersCount; i++)
            {
                _workers.Add(new Worker(_queue, this, _serviceScopeFactory));
            }

            OnTaskStarted += WorkerStartTask;
            OnTaskCompleted += WorkerCompleteTask;
            OnTaskErrorOccured += WorkerFacedErrorWithTask;
        }

        public Guid AddTask(Task task)
        {
            _queue.AddTask(task);
            Run();

            return task.Id;
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
        /// Collect info about uploader status.
        /// </summary>
        /// <returns> <see cref="TaskSchedulerStatus"/> with state of internal queue and scheduler. </returns>
        public TaskSchedulerStatus GetStatus() => new(_queue, this);

        /// <summary>
        /// Collect info about certain task.
        /// </summary>
        /// <param name="taskId"> Task id. </param>
        /// <returns> <see cref="TaskStatus"/> with task state. </returns>
        public TaskStatus GetStatus(Guid taskId) 
        {
            var tastStatus = new TaskStatus
            {
                Id = taskId,
                IsInProgress = _workers.Where(x => x.Task?.Id == taskId).Any(),
                QueuePosition = _queue.GetTaskPosition(taskId)
            };
            var task = _queue.GetTask(taskId);
            tastStatus.IsExist = task != null;
            tastStatus.WorkCount = task == null 
                ? 0 
                : task.GetWorkCount();

            return tastStatus;
        }
    }
}
