using MediaCloud.WebApp.Services.ConfigProvider;
using Task = MediaCloud.TaskScheduler.Tasks.Task;

namespace MediaCloud.TaskScheduler
{
    public class Queue
    {
        private readonly Mutex _takeMutex = new Mutex();
        private readonly int _completedTaskLifetimeMin;
        private readonly List<Task> _tasks = [];

        private void RemoveOldCompletedTasks()
        {
            _tasks.RemoveAll(x => x.IsCompleted 
                && DateTime.Now - x.CompletedAt > new TimeSpan(0, _completedTaskLifetimeMin, 0));
        }

        public readonly Action<Task> OnTaskComplete;

        public Queue(IConfigProvider configProvider) 
        {
            OnTaskComplete += CompleteTask;

            _completedTaskLifetimeMin = configProvider.EnvironmentSettings.TaskSchedulerQueueCleanupTime;
        }

        public bool IsEmpty => _tasks.Count == 0;

        public int TaskCount => _tasks.Count(x => x.IsCompleted == false);

        public int WorkCount => _tasks.Sum(x => x.GetWorkCount());
        
        private void CompleteTask(Task task) 
        {
            task.IsCompleted = true;
            task.CompletedAt = DateTime.Now;

            RemoveOldCompletedTasks();
        }

        public void AddTask(Task task)
        { 
            _tasks.Add(task);

            RemoveOldCompletedTasks();
        }

        public void CleanupCompleted()
        {
            _tasks.RemoveAll(x => x.IsCompleted);
        }

        public void CleanupAll()
        {
            _tasks.Clear();
        }

        public Task? TakeNextTask() 
        {
            _takeMutex.WaitOne();
            var task = _tasks.FirstOrDefault(x => x is { IsCompleted: false, IsExecuted: false });

            if (task == null)
            {
                _takeMutex.ReleaseMutex();
                return null;
            }

            task.IsExecuted = true;
            _takeMutex.ReleaseMutex();

            return task;
        }

        public Task? GetTask(Guid id) => _tasks.FirstOrDefault(x => x.Id == id);

        public int GetTaskPosition(Guid id)
        {
            var task = GetTask(id);

            if (task == null)
            {
                return -1;
            }

            return _tasks.Where(x => x.IsCompleted == false && x.IsExecuted == false).ToList().IndexOf(task) + 1;
        }

        public TaskStatus GetTaskStatus(Guid taskId) 
        {
             var taskStatus = new TaskStatus
            {
                Id = taskId,
                QueuePosition = GetTaskPosition(taskId)
            };
            var task = GetTask(taskId);

            if (task == null)
            {
                return new TaskStatus();
            }


            //TODO: Replace with Task itself.
            taskStatus.Type = task.Type;
            taskStatus.IsInProgress = task.IsExecuted;
            taskStatus.IsCompleted = task.IsCompleted;
            taskStatus.IsSuccess = !task.IsFailed;
            taskStatus.CompletionMessage = task.CompletionMessage;
            taskStatus.WorkCount = task.GetWorkCount();
            taskStatus.ExecutedAt = task.ExecutedAt;
            taskStatus.CompletedAt = task.CompletedAt;

            return taskStatus;
        }

        public List<TaskStatus> GetTaskStatuses()
        {
            return _tasks.Select(task => GetTaskStatus(task.Id)).ToList();
        }
    }
}
