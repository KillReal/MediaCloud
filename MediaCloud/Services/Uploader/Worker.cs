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

        public Guid CurrentTask = Guid.Empty;
        public bool IsRunning { get; set; } = false;

        public void Run()
        {
            IsRunning = true;

            _workRoutine = new(WorkRoutine);
            _workRoutine.Start();
        }

        public void Stop()
        {
            IsRunning = false;

            if (_workRoutine != null)
            {
                _workRoutine.Join();
            }
        }

        private void WorkRoutine()
        {
            while (IsRunning)
            {
                if (Queue.IsEmpty)
                {
                    IsRunning = false;
                    return;
                }

                var task = Queue.GetTask();
                CurrentTask = task.Id;

                Scheduler.GetLogger().LogInformation($"Worker ({Scheduler.WorkersActive}/{Scheduler.MaxWorkersCount}) running with task: {CurrentTask}");
                
                try
                {
                    task.DoTheTask();
                }
                catch (Exception ex)
                {
                    Scheduler.GetLogger().LogError($"Worker failed Media[{task.GetWorkCount}] creation with task: {CurrentTask} exception: {ex}");
                }

                Queue.RemoveTask(task);
                Scheduler.GetLogger().LogInformation($"Worker ({Scheduler.WorkersActive - 1}/{Scheduler.MaxWorkersCount}) done task: {CurrentTask}");
                CurrentTask = Guid.Empty;
            }
        }
    }
}
