using Microsoft.Data.Sqlite;
using NexsusVKVideo.Core.Models;
using NexsusVKVideo.Infrastructure.Storage;

namespace NexsusVKVideo.Infrastructure.Tests;

public sealed class SqliteLocalDataStoreTests : IAsyncLifetime
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "NexsusVKVideo.Tests", Guid.NewGuid().ToString("N"));
    private string DatabasePath => Path.Combine(_directory, "data", "test.db");

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
    public async Task AddFavoriteAsync_PersistsAcrossStoreInstancesWithoutDuplicates()
    {
        var video = new VideoSummary(new VideoKey("vk", "-42_7"), "Видео");

        using (var firstStore = new SqliteLocalDataStore(DatabasePath))
        {
            Assert.True(await firstStore.AddAsync(video, CancellationToken.None));
            Assert.False(await firstStore.AddAsync(video, CancellationToken.None));
        }

        using var reopenedStore = new SqliteLocalDataStore(DatabasePath);
        var persisted = await reopenedStore.GetAllAsync(CancellationToken.None);

        Assert.Equal(video, Assert.Single(persisted));
    }

    [Fact]
    public async Task AddHistoryAsync_ReturnsRecentEntriesAndClearRemovesThem()
    {
        using var store = new SqliteLocalDataStore(DatabasePath);
        var older = new HistoryEntry(new VideoSummary(new VideoKey("vk", "1"), "Старое"), DateTimeOffset.UnixEpoch);
        var newer = new HistoryEntry(new VideoSummary(new VideoKey("vk", "2"), "Новое"), DateTimeOffset.UnixEpoch.AddMinutes(1));

        await store.AddAsync(older, CancellationToken.None);
        await store.AddAsync(newer, CancellationToken.None);

        var recent = await store.GetRecentAsync(1, CancellationToken.None);
        Assert.Equal(newer, Assert.Single(recent));

        await store.ClearAsync(CancellationToken.None);
        Assert.Empty(await store.GetRecentAsync(10, CancellationToken.None));
    }

    [Fact]
    public async Task SaveSettingsAsync_PersistsAndMigrationSetsTheSchemaVersion()
    {
        using var store = new SqliteLocalDataStore(DatabasePath);
        var expected = new AppSettings(2, "Dark");

        await store.SaveAsync(expected, CancellationToken.None);

        Assert.Equal(expected, await store.LoadAsync(CancellationToken.None));
        await using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = DatabasePath,
            Pooling = false
        }.ToString());
        await connection.OpenAsync(CancellationToken.None);
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA user_version;";

        Assert.Equal(1L, Convert.ToInt64(await command.ExecuteScalarAsync(CancellationToken.None)));
    }

    [Fact]
    public async Task RemoveFavoriteAsync_ReturnsWhetherTheVideoExisted()
    {
        using var store = new SqliteLocalDataStore(DatabasePath);
        var video = new VideoSummary(new VideoKey("vk", "8"), "Видео");

        await store.AddAsync(video, CancellationToken.None);

        Assert.True(await store.RemoveAsync(video.Key, CancellationToken.None));
        Assert.False(await store.RemoveAsync(video.Key, CancellationToken.None));
    }

    [Fact]
    public async Task GetAllAsync_WhenDatabaseIsNewerThanTheApp_RefusesToDowngradeIt()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath)!);
        await using (var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = DatabasePath,
            Pooling = false
        }.ToString()))
        {
            await connection.OpenAsync(CancellationToken.None);
            await using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA user_version = 2;";
            await command.ExecuteNonQueryAsync(CancellationToken.None);
        }

        using var store = new SqliteLocalDataStore(DatabasePath);

        await Assert.ThrowsAsync<InvalidOperationException>(() => store.GetAllAsync(CancellationToken.None));
    }

    [Fact]
    public async Task GetAllAsync_WhenCanceled_StopsBeforeOpeningTheDatabase()
    {
        using var store = new SqliteLocalDataStore(DatabasePath);
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => store.GetAllAsync(cancellationSource.Token));
        Assert.False(File.Exists(DatabasePath));
    }
}
