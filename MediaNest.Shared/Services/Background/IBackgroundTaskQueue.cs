namespace MediaNest.Shared.Services.Background;

public interface IBackgroundTaskQueue {
    ValueTask EnqueueTask(Func<CancellationToken, ValueTask> workitem);
    ValueTask<Func<CancellationToken, ValueTask>> DequeueTask(CancellationToken cancellationToken);

}
