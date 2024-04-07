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
        public Worker(Queue queue, TaskScheduler scheduler, IServiceScopeFactory serviceScopeFactory) 
        {
            _queue = queue;
            _scheduler = scheduler;
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// Worker take first <see cref="ITask"/> from <see cref="Queue"/> and begin it's processing in new <see cref="Thread"/>.
        /// </summary>
        public void Run()
        {
            Task = _queue.GetNextTask();
            ThreadPool.QueueUserWorkItem(WorkRoutine);
        }

        /// <summary>
        /// Waiting to worker thread completion or stop. Usually used for wait to thread completion.
        /// </summary>
        public void Stop()
        {
            
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

            _scheduler.OnTaskCompleted.Invoke(Task);
            Task = null;
            IsReady = true;

            return;
        }
    }
}
