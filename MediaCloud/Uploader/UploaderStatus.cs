using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCloud.Uploader
{
    public class UploaderStatus
    {
        public bool IsSchedulerRunning { get; set; }
        public int TaskCount { get; set; }
        public int WorkersActive { get; set; }

        public UploaderStatus()
        {
            IsSchedulerRunning = Scheduler.IsRunning;
            TaskCount = Queue.TaskCount;
            WorkersActive = Scheduler.WorkersActive;
        }
    }
}
