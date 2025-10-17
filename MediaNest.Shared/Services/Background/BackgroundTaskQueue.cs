using System.Threading.Channels;

namespace MediaNest.Shared.Services.Background;

using WorkItem = Func<CancellationToken, ValueTask>;
public class BackgroundTaskQueue : IBackgroundTaskQueue {

    private readonly Channel<WorkItem> _queue;

    public BackgroundTaskQueue(int capacity = 100) {
        var options = new BoundedChannelOptions(capacity) {
            FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<WorkItem>(options);
    }
    public async ValueTask<WorkItem> DequeueTask(CancellationToken cancellationToken) {
        var workItem = await _queue.Reader.ReadAsync(cancellationToken);
        return workItem;
    }

    public async ValueTask EnqueueTask(WorkItem workitem) {
        if (workitem == null)
            throw new ArgumentNullException(nameof(workitem));
        await _queue.Writer.WriteAsync(workitem);
    }
}
