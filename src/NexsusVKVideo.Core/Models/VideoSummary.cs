namespace NexsusVKVideo.Core.Models;

public sealed record VideoSummary
{
    public VideoSummary(VideoKey key, string title)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Title = string.IsNullOrWhiteSpace(title)
            ? throw new ArgumentException("Title must not be empty.", nameof(title))
            : title.Trim();
    }

    public VideoKey Key { get; }

    public string Title { get; }
}
