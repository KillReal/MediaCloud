using MediaCloud.TaskScheduler;

namespace MediaCloud.WebApp;

public class AutotagWorker : Worker
{
    public AutotagWorker(Queue queue, TaskScheduler.TaskScheduler scheduler, IServiceScopeFactory serviceScopeFactory)
        : base(queue, scheduler, serviceScopeFactory,
        new List<Type> { typeof(AutotagPreviewTask), typeof(AutotagCollectionTask) })
    {
    }
}

