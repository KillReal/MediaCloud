using MediaCloud.TaskScheduler;

namespace MediaCloud.WebApp;

public class AutotagWorker(Queue queue, TaskScheduler.TaskScheduler scheduler, IServiceScopeFactory serviceScopeFactory) : Worker(queue, scheduler, serviceScopeFactory,
    [typeof(AutotagPreviewTask), typeof(AutotagCollectionTask)])
{
}

