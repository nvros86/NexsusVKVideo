using NexsusVKVideo.Core.Models;

namespace NexsusVKVideo.Core.Services;

public sealed class FavoritesCollection
{
    private readonly HashSet<VideoKey> _keys = [];
    private readonly List<VideoSummary> _videos = [];

    public IReadOnlyList<VideoSummary> Videos => _videos;

    public bool TryAdd(VideoSummary video)
    {
        ArgumentNullException.ThrowIfNull(video);

        if (!_keys.Add(video.Key))
        {
            return false;
        }

        _videos.Add(video);
        return true;
    }

    public bool Remove(VideoKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (!_keys.Remove(key))
        {
            return false;
        }

        _videos.RemoveAll(video => video.Key == key);
        return true;
    }
}
