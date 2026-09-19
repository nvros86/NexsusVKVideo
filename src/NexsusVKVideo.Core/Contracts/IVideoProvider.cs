using NexsusVKVideo.Core.Models;

namespace NexsusVKVideo.Core.Contracts;

public interface IVideoProvider
{
    Task<VideoSearchResult> SearchAsync(VideoSearchRequest request, CancellationToken cancellationToken);

    Task<VideoSummary?> GetVideoAsync(VideoKey key, CancellationToken cancellationToken);

    Task<PlaybackDescriptor?> GetPlaybackAsync(VideoKey key, CancellationToken cancellationToken);
}
