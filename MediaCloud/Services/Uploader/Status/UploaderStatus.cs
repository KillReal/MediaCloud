﻿using System;
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

        public UploaderStatus(Queue currentQueue, Scheduler scheduler)
        {
            MediaCount = currentQueue.WorkCount;
            TaskCount = currentQueue.TaskCount;
            WorkersActive = scheduler.WorkersActive;
            MaxWorkersAvailable = scheduler.MaxWorkersCount - WorkersActive;
        }
    }
}
