using NexsusVKVideo.Core.Models;

namespace NexsusVKVideo.Core.Contracts;

public interface IFavoritesRepository
{
    Task<IReadOnlyList<VideoSummary>> GetAllAsync(CancellationToken cancellationToken);

    Task<bool> AddAsync(VideoSummary video, CancellationToken cancellationToken);

    Task<bool> RemoveAsync(VideoKey key, CancellationToken cancellationToken);
}
