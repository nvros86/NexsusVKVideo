using NexsusVKVideo.Core.Models;

namespace NexsusVKVideo.Core.Contracts;

public interface IHistoryRepository
{
    Task<IReadOnlyList<HistoryEntry>> GetRecentAsync(int take, CancellationToken cancellationToken);

    Task AddAsync(HistoryEntry entry, CancellationToken cancellationToken);

    Task ClearAsync(CancellationToken cancellationToken);
}
