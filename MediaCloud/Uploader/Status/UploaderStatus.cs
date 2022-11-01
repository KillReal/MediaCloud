using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCloud.MediaUploader
{
    public class UploaderStatus
    {
        public int TaskCount { get; set; }
        public int MediaCount { get; set; }
        public int WorkersActive { get; set; }
        public int MaxWorkersAvailable { get; set; }

        public UploaderStatus()
        {
            MediaCount = Queue.MediaCount;
            TaskCount = Queue.TaskCount;
            WorkersActive = Scheduler.WorkersActive;
            MaxWorkersAvailable = Scheduler.MaxWorkersCount - WorkersActive;
        }
    }
}
