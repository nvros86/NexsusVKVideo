using NexsusVKVideo.Core.Models;

namespace NexsusVKVideo.Core.Contracts;

public interface ISettingsStore
{
    Task<AppSettings> LoadAsync(CancellationToken cancellationToken);

    Task SaveAsync(AppSettings settings, CancellationToken cancellationToken);
}
