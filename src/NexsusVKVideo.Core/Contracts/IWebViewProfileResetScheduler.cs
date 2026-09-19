namespace NexsusVKVideo.Core.Contracts;

public interface IWebViewProfileResetScheduler
{
    Task ScheduleAsync(CancellationToken cancellationToken);
}
