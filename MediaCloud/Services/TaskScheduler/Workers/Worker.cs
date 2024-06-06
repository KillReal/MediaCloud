using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.TaskScheduler.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Task = MediaCloud.TaskScheduler.Tasks.Task;

namespace MediaCloud.TaskScheduler
{
    /// <summary>
    /// Worker which do task processing.
    /// </summary>
    public class Worker
    {
        private readonly string _taskType;
        private readonly Queue _queue;
        private readonly TaskScheduler _scheduler;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        /// <summary>
        /// Current task. Return's null if no task currently taken.
        /// </summary>
        public Task? Task;

        /// <summary>
        /// State of Worker that it ready to take next <see cref="ITask"/>.
        /// </summary>
        public bool IsReady { get; private set; } = true;

        /// <summary>
        /// Initilize worker instance.
        /// </summary>
        /// <param name="queue"> Current <see cref="Queue"/>. </param>
        /// <param name="scheduler"> Current <see cref="TaskScheduler"/>. </param>
        /// <param name="dataService"> Current data service <seealso cref="IDataService"/>. </param>
        public Worker(Queue queue, TaskScheduler scheduler, IServiceScopeFactory serviceScopeFactory, string taskType) 
        {
            _queue = queue;
            _scheduler = scheduler;
            _serviceScopeFactory = serviceScopeFactory;
            _taskType = taskType;
        }

        /// <summary>
        /// Worker take first <see cref="ITask"/> from <see cref="Queue"/> and begin it's processing in new <see cref="Thread"/>.
        /// </summary>
        public void Run()
        {
            Task = _queue.GetNextTask(_taskType);

            if (Task == null)
            {
                return;
            }
            
            ThreadPool.QueueUserWorkItem(WorkRoutine);
        }

        /// <summary>
        /// Checks if the worker is able to execute the given task type.
        /// </summary>
        /// <param name="taskType"> The type of task to check. </param>
        /// <returns> True if the worker can execute the task type, otherwise false. </returns>
        public bool IsAbleToExecute(string taskType)
        {
            return _taskType.Split(" ").Any(x => x == taskType);
        }

        private void WorkRoutine(object? state)
        {
            IsReady = false;

            if (Task == null)
            {
                return;
            }

            _scheduler.OnTaskStarted.Invoke(Task);

            try
            {
                var taskContext = new TaskExecutionContext(Task);

                taskContext.DoTheTask(_serviceScopeFactory.CreateScope().ServiceProvider);
            }
            catch (Exception ex)
            {
                _scheduler.OnTaskErrorOccured.Invoke(Task, ex);
            }

            IsReady = true;

            _scheduler.OnTaskCompleted.Invoke(Task);
        }
    }
}
