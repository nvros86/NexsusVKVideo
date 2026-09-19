using Microsoft.Data.Sqlite;
using NexsusVKVideo.Core.Contracts;
using NexsusVKVideo.Core.Models;

namespace NexsusVKVideo.Infrastructure.Storage;

public sealed class SqliteLocalDataStore : IHistoryRepository, IFavoritesRepository, ISettingsStore, IDisposable
{
    private const int CurrentDatabaseVersion = 1;
    private readonly string _databasePath;
    private readonly string _connectionString;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private bool _initialized;

    public SqliteLocalDataStore(string databasePath)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            throw new ArgumentException("Database path must not be empty.", nameof(databasePath));
        }

        _databasePath = Path.GetFullPath(databasePath);
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = _databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared,
            Pooling = false
        }.ToString();
    }

    public async Task<IReadOnlyList<HistoryEntry>> GetRecentAsync(int take, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(take);
        if (take == 0)
        {
            return [];
        }

        await EnsureMigratedAsync(cancellationToken);
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT provider, video_id, title, watched_at_unix_ms
            FROM History
            ORDER BY watched_at_unix_ms DESC, id DESC
            LIMIT $take;
            """;
        command.Parameters.AddWithValue("$take", take);

        var entries = new List<HistoryEntry>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            entries.Add(new HistoryEntry(
                new VideoSummary(
                    new VideoKey(reader.GetString(0), reader.GetString(1)),
                    reader.GetString(2)),
                DateTimeOffset.FromUnixTimeMilliseconds(reader.GetInt64(3))));
        }

        return entries;
    }

    public async Task AddAsync(HistoryEntry entry, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);
        await EnsureMigratedAsync(cancellationToken);
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO History (provider, video_id, title, watched_at_unix_ms)
            VALUES ($provider, $videoId, $title, $watchedAt);
            """;
        AddVideoParameters(command, entry.Video);
        command.Parameters.AddWithValue("$watchedAt", entry.WatchedAt.ToUnixTimeMilliseconds());
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task ClearAsync(CancellationToken cancellationToken)
    {
        await EnsureMigratedAsync(cancellationToken);
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM History;";
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<VideoSummary>> GetAllAsync(CancellationToken cancellationToken)
    {
        await EnsureMigratedAsync(cancellationToken);
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT provider, video_id, title
            FROM Favorites
            ORDER BY title COLLATE NOCASE, provider, video_id;
            """;

        var favorites = new List<VideoSummary>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            favorites.Add(new VideoSummary(
                new VideoKey(reader.GetString(0), reader.GetString(1)),
                reader.GetString(2)));
        }

        return favorites;
    }

    public async Task<bool> AddAsync(VideoSummary video, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(video);
        await EnsureMigratedAsync(cancellationToken);
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Favorites (provider, video_id, title)
            VALUES ($provider, $videoId, $title)
            ON CONFLICT(provider, video_id) DO NOTHING;
            """;
        AddVideoParameters(command, video);
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<bool> RemoveAsync(VideoKey key, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(key);
        await EnsureMigratedAsync(cancellationToken);
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Favorites WHERE provider = $provider AND video_id = $videoId;";
        command.Parameters.AddWithValue("$provider", key.Provider);
        command.Parameters.AddWithValue("$videoId", key.VideoId);
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken)
    {
        await EnsureMigratedAsync(cancellationToken);
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT settings_schema_version, theme FROM Settings WHERE id = 1;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken)
            ? new AppSettings(reader.GetInt32(0), reader.GetString(1))
            : new AppSettings(1, "System");
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(settings);
        if (settings.SchemaVersion < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(settings), "Settings schema version must be positive.");
        }

        if (string.IsNullOrWhiteSpace(settings.Theme))
        {
            throw new ArgumentException("Theme must not be empty.", nameof(settings));
        }

        await EnsureMigratedAsync(cancellationToken);
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Settings (id, settings_schema_version, theme)
            VALUES (1, $schemaVersion, $theme)
            ON CONFLICT(id) DO UPDATE SET
                settings_schema_version = excluded.settings_schema_version,
                theme = excluded.theme;
            """;
        command.Parameters.AddWithValue("$schemaVersion", settings.SchemaVersion);
        command.Parameters.AddWithValue("$theme", settings.Theme.Trim());
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public void Dispose()
    {
        _initializationLock.Dispose();
    }

    private async Task EnsureMigratedAsync(CancellationToken cancellationToken)
    {
        if (_initialized)
        {
            return;
        }

        await _initializationLock.WaitAsync(cancellationToken);
        try
        {
            if (_initialized)
            {
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(_databasePath)!);
            await using var connection = await OpenConnectionAsync(cancellationToken);
            var databaseVersion = await GetDatabaseVersionAsync(connection, cancellationToken);
            if (databaseVersion > CurrentDatabaseVersion)
            {
                throw new InvalidOperationException(
                    $"Database version {databaseVersion} is newer than supported version {CurrentDatabaseVersion}.");
            }

            if (databaseVersion == CurrentDatabaseVersion)
            {
                _initialized = true;
                return;
            }

            await using var command = connection.CreateCommand();
            command.CommandText = """
                BEGIN IMMEDIATE;
                CREATE TABLE IF NOT EXISTS Favorites (
                    provider TEXT NOT NULL,
                    video_id TEXT NOT NULL,
                    title TEXT NOT NULL,
                    PRIMARY KEY (provider, video_id)
                );
                CREATE TABLE IF NOT EXISTS History (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    provider TEXT NOT NULL,
                    video_id TEXT NOT NULL,
                    title TEXT NOT NULL,
                    watched_at_unix_ms INTEGER NOT NULL
                );
                CREATE INDEX IF NOT EXISTS IX_History_Recent
                    ON History (watched_at_unix_ms DESC, id DESC);
                CREATE TABLE IF NOT EXISTS Settings (
                    id INTEGER PRIMARY KEY CHECK (id = 1),
                    settings_schema_version INTEGER NOT NULL,
                    theme TEXT NOT NULL
                );
                PRAGMA user_version = 1;
                COMMIT;
                """;
            await command.ExecuteNonQueryAsync(cancellationToken);
            _initialized = true;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    private async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private static async Task<int> GetDatabaseVersionAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA user_version;";
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    private static void AddVideoParameters(SqliteCommand command, VideoSummary video)
    {
        command.Parameters.AddWithValue("$provider", video.Key.Provider);
        command.Parameters.AddWithValue("$videoId", video.Key.VideoId);
        command.Parameters.AddWithValue("$title", video.Title);
    }
}
