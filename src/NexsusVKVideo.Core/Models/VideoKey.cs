namespace NexsusVKVideo.Core.Models;

public sealed record VideoKey
{
    public VideoKey(string provider, string videoId)
    {
        Provider = RequireValue(provider, nameof(provider));
        VideoId = RequireValue(videoId, nameof(videoId));
    }

    public string Provider { get; }

    public string VideoId { get; }

    private static string RequireValue(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value must not be empty.", parameterName);
        }

        return value.Trim();
    }
}
