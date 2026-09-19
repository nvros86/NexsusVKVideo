namespace NexsusVKVideo.Core.Models;

public sealed record VideoSearchResult(IReadOnlyList<VideoSummary> Videos, bool HasMore);
