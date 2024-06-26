﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCloud.TaskScheduler
{
    public class TaskSchedulerStatus
    {
        public int TaskCount { get; set; }
        public int MediaCount { get; set; }
        public int WorkersActive { get; set; }
        public int MaxWorkersAvailable { get; set; }
        public List<TaskStatus> TaskStatuses { get; set; } = new();

        public TaskSchedulerStatus(Queue currentQueue, TaskScheduler scheduler)
        {
            MediaCount = currentQueue.WorkCount;
            TaskCount = currentQueue.TaskCount;
            WorkersActive = scheduler.BusyWorkersCount;
            MaxWorkersAvailable = scheduler.MaxWorkersCount - WorkersActive;
            TaskStatuses = currentQueue.GetTaskStatuses();
        }
    }
}
