using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.MediaUploader.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCloud.MediaUploader
{
    public class Worker
    {
        private Thread _workRoutine;

        private Queue _queue;
        private Scheduler _scheduler;

        public Guid CurrentTask = Guid.Empty;
        public bool IsRunning { get; set; } = false;

        public Worker(Queue queue, Scheduler scheduler) 
        {
            _queue = queue;
            _scheduler = scheduler;
            _workRoutine = new(WorkRoutine);
        }

        public void Run()
        {
            IsRunning = true;
            _workRoutine.Start();
        }

        public void Stop()
        {
            IsRunning = false;
            _workRoutine?.Join();
        }

        private void WorkRoutine()
        {
            while (IsRunning)
            {
                if (_queue.IsEmpty)
                {
                    IsRunning = false;
                    return;
                }

                var task = _queue.GetTask();
                CurrentTask = task.Id;

                var logger = _scheduler.GetLogger();
                logger.LogInformation("Worker ({_scheduler.WorkersActive}/{_scheduler.MaxWorkersCount}) running with task: {CurrentTask}",
                    _scheduler.WorkersActive, _scheduler.MaxWorkersCount, CurrentTask);
                
                try
                {
                    task.DoTheTask(_scheduler.GetDataService());
                }
                catch (Exception ex)
                {
                    logger.LogError("Worker failed Media[{task.GetWorkCount}] creation with task: {CurrentTask} exception: {ex}",
                        task.GetWorkCount(), CurrentTask, ex);
                }

                _queue.RemoveTask(task);
                logger.LogInformation("Worker ({_scheduler.WorkersActive - 1}/{_scheduler.MaxWorkersCount}) done task: {CurrentTask}",
                    _scheduler.WorkersActive - 1, _scheduler.MaxWorkersCount, CurrentTask);
                CurrentTask = Guid.Empty;
            }
        }
    }
}
