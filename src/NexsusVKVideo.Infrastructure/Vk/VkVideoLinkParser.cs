using System.Text.RegularExpressions;
using NexsusVKVideo.Core.Models;

namespace NexsusVKVideo.Infrastructure.Vk;

public sealed partial class VkVideoLinkParser
{
    public bool TryParse(Uri? uri, out VkVideoLink? videoLink)
    {
        videoLink = null;
        if (!VkEmbedUriValidator.IsAllowedVideoPage(uri))
        {
            return false;
        }

        var match = VideoPagePath().Match(uri!.AbsolutePath);
        if (!match.Success)
        {
            return false;
        }

        var ownerId = match.Groups["ownerId"].Value;
        var videoId = match.Groups["videoId"].Value;
        var key = new VideoKey("vk", $"{ownerId}_{videoId}");
        var videoPageUri = new Uri($"https://vk.ru/video{ownerId}_{videoId}", UriKind.Absolute);
        var embedUri = new Uri($"https://vk.ru/video_ext.php?oid={ownerId}&id={videoId}", UriKind.Absolute);
        videoLink = new VkVideoLink(key, videoPageUri, embedUri);
        return true;
    }

    [GeneratedRegex("^/video(?<ownerId>-?\\d+)_(?<videoId>\\d+)$", RegexOptions.CultureInvariant)]
    private static partial Regex VideoPagePath();
}

public sealed record VkVideoLink(VideoKey Key, Uri VideoPageUri, Uri EmbedUri);
