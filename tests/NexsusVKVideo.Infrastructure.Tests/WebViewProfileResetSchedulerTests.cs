using NexsusVKVideo.Infrastructure.Storage;

namespace NexsusVKVideo.Infrastructure.Tests;

public sealed class WebViewProfileResetSchedulerTests : IAsyncLifetime
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "NexsusVKVideo.Tests", Guid.NewGuid().ToString("N"));

    public Task InitializeAsync()
    {
        Directory.CreateDirectory(_directory);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }

        return Task.CompletedTask;
    }

    [Fact]
    public async Task ApplyScheduledReset_DeletesOnlyTheScheduledWebViewProfile()
    {
        var profileDirectory = Path.Combine(_directory, "WebView2");
        var markerPath = Path.Combine(_directory, "data", "webview2-reset.pending");
        var unrelatedFile = Path.Combine(_directory, "data", "nexsusvkvideo.db");
        Directory.CreateDirectory(profileDirectory);
        Directory.CreateDirectory(Path.GetDirectoryName(unrelatedFile)!);
        await File.WriteAllTextAsync(Path.Combine(profileDirectory, "cache.bin"), "cache");
        await File.WriteAllTextAsync(unrelatedFile, "database");
        var scheduler = new WebViewProfileResetScheduler(profileDirectory, markerPath);

        await scheduler.ScheduleAsync(CancellationToken.None);
        scheduler.ApplyScheduledReset();

        Assert.False(Directory.Exists(profileDirectory));
        Assert.False(File.Exists(markerPath));
        Assert.True(File.Exists(unrelatedFile));
    }
}
