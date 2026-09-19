namespace NexsusVKVideo.Core.Models;

public sealed record HistoryEntry(VideoSummary Video, DateTimeOffset WatchedAt);
