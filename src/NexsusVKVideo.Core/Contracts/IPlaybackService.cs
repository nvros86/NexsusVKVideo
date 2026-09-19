using NexsusVKVideo.Core.Models;

namespace NexsusVKVideo.Core.Contracts;

public interface IPlaybackService : IAsyncDisposable
{
    Task OpenAsync(PlaybackDescriptor descriptor, CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);
}
