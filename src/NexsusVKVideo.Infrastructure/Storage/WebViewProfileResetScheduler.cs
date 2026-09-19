using NexsusVKVideo.Core.Contracts;

namespace NexsusVKVideo.Infrastructure.Storage;

public sealed class WebViewProfileResetScheduler : IWebViewProfileResetScheduler
{
    private readonly string _userDataFolder;
    private readonly string _markerPath;

    public WebViewProfileResetScheduler(string userDataFolder, string markerPath)
    {
        _userDataFolder = Path.GetFullPath(userDataFolder ?? throw new ArgumentNullException(nameof(userDataFolder)));
        _markerPath = Path.GetFullPath(markerPath ?? throw new ArgumentNullException(nameof(markerPath)));
    }

    public async Task ScheduleAsync(CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_markerPath)!);
        await File.WriteAllTextAsync(_markerPath, "reset", cancellationToken);
    }

    public void ApplyScheduledReset()
    {
        if (!File.Exists(_markerPath))
        {
            return;
        }

        if (Directory.Exists(_userDataFolder))
        {
            Directory.Delete(_userDataFolder, recursive: true);
        }

        File.Delete(_markerPath);
    }
}
