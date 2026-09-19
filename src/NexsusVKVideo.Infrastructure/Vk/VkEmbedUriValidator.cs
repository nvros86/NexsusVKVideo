namespace NexsusVKVideo.Infrastructure.Vk;

public static class VkEmbedUriValidator
{
    private const string VkHost = "vk.ru";

    public static bool IsAllowedEmbed(Uri? uri)
    {
        if (!IsSecureVkUri(uri) || !string.Equals(uri!.AbsolutePath, "/video_ext.php", StringComparison.Ordinal))
        {
            return false;
        }

        var parameters = ParseQuery(uri.Query);
        return parameters.TryGetValue("oid", out var oid) && !string.IsNullOrWhiteSpace(oid)
            && parameters.TryGetValue("id", out var id) && !string.IsNullOrWhiteSpace(id);
    }

    public static bool IsAllowedVideoPage(Uri? uri) =>
        IsSecureVkUri(uri) && uri!.AbsolutePath.StartsWith("/video-", StringComparison.Ordinal);

    public static bool IsAllowedVkSite(Uri? uri) =>
        uri is { IsAbsoluteUri: true }
        && string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
        && uri.Port == 443
        && (IsVkOwnedHost(uri.Host, "vk.ru")
            || IsVkOwnedHost(uri.Host, "vkvideo.ru")
            || IsVkOwnedHost(uri.Host, "vk.com"));

    private static bool IsSecureVkUri(Uri? uri) =>
        uri is { IsAbsoluteUri: true }
        && string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
        && string.Equals(uri.Host, VkHost, StringComparison.OrdinalIgnoreCase)
        && uri.Port == 443;

    private static bool IsVkOwnedHost(string host, string rootHost) =>
        string.Equals(host, rootHost, StringComparison.OrdinalIgnoreCase)
        || host.EndsWith($".{rootHost}", StringComparison.OrdinalIgnoreCase);

    private static Dictionary<string, string> ParseQuery(string query)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var item in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = item.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            var key = Uri.UnescapeDataString(item[..separator]);
            var value = Uri.UnescapeDataString(item[(separator + 1)..]);
            result.TryAdd(key, value);
        }

        return result;
    }
}
