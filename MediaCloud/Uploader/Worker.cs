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

            _workRoutine.Join();
        }

        public void WorkRoutine()
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

                task.DoTheTask();

                Queue.RemoveTask(task);
                CurrentTask = Guid.Empty;
            }
        }
    }
}
