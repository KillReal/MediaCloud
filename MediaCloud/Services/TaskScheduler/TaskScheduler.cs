using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.ConfigProvider;
using NLog;
using Task = MediaCloud.TaskScheduler.Tasks.Task;

namespace MediaCloud.TaskScheduler
{
    /// <summary>
    /// Internal task scheduler.
    /// </summary>
    public partial class TaskScheduler : ITaskScheduler
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly Logger _logger;
        private readonly List<Worker> _workers = [];
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
            _logger = LogManager.GetLogger("Scheduler");
            _workers = [];
            _queue = new Queue(configProvider);

            var workersCount = configProvider.EnvironmentSettings.TaskSchedulerWorkerCount;
            for (int i = 0; i < workersCount; i++)
            {
                _workers.Add(new Worker(_queue, this, _serviceScopeFactory));
            }

            MaxWorkersCount = _workers.Count;

            OnTaskStarted += WorkerStartTask;
            OnTaskCompleted += WorkerCompleteTask;
            OnTaskErrorOccured += WorkerFacedErrorWithTask;

            _logger.Debug("Initialized TaskScheduler");
        }

        
        /// <summary>
        /// Adds a task to the queue and starts processing the queue.
        /// </summary>
        /// <param name="task">The task to be added to the queue.</param>
        /// <returns>The unique identifier of the added task.</returns>
        public Guid AddTask(Task task)
        {
            _queue.AddTask(task);
            Run();

            return task.Id;
        }

        /// <summary>
        /// Executes the scheduler run process.
        /// Checks if the queue is empty, and if not, assigns tasks to available workers.
        /// </summary>
        public void Run()
        {
            _logger.Info("Scheduler was triggered with queue size <{_queue.TaskCount}>", _queue.TaskCount);

            if (_queue.IsEmpty)
            {
                return;
            }

            _workers.Where(worker => worker.IsReady).FirstOrDefault()?.Run();
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
        public TaskStatus GetStatus(Guid taskId) => _queue.GetTaskStatus(taskId);

        public void CleanupQueue(bool cleanupOnlyCompleted = true)
        {
            if (cleanupOnlyCompleted) 
            {
                _queue.CleanupCompleted();
            }
            else
            {
                _queue.CleanupAll();
            }
        }
    }
}
