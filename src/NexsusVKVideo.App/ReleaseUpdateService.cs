using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace NexsusVKVideo.App;

internal sealed class ReleaseUpdateService
{
    private const string ReleasesEndpoint = "https://api.github.com/repos/nvros86/NexsusVKVideo/releases?per_page=20";
    private static readonly HttpClient HttpClient = CreateHttpClient();
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<AvailableRelease?> GetNewerReleaseAsync(string currentVersion, CancellationToken cancellationToken)
    {
        if (!ReleaseVersion.TryParse(currentVersion, out var current))
        {
            return null;
        }

        using var response = await HttpClient.GetAsync(ReleasesEndpoint, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        var releases = await JsonSerializer.DeserializeAsync<List<GitHubRelease>>(content, JsonOptions, cancellationToken) ?? [];
        return releases
            .Where(release => !release.Draft && ReleaseVersion.TryParse(release.TagName, out _))
            .Select(release => new { Release = release, Version = ReleaseVersion.Parse(release.TagName) })
            .Where(item => item.Version.CompareTo(current) > 0)
            .OrderByDescending(item => item.Version)
            .Select(item => new AvailableRelease(item.Release.TagName, GetReleasePageUri(item.Release.TagName)))
            .FirstOrDefault();
    }

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("NexsusVKVideo", "1.0"));
        client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        return client;
    }

    private static Uri GetReleasePageUri(string tagName) => new($"https://github.com/nvros86/NexsusVKVideo/releases/tag/{Uri.EscapeDataString(tagName)}", UriKind.Absolute);

    private sealed record GitHubRelease(string TagName, bool Draft);

    internal sealed record AvailableRelease(string TagName, Uri ReleasePage);

    private sealed record ReleaseVersion(int Major, int Minor, int Patch, int BetaNumber) : IComparable<ReleaseVersion>
    {
        private static readonly Regex Pattern = new(@"^v?(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(?:-beta\.(?<beta>\d+))?$", RegexOptions.CultureInvariant);

        public static bool TryParse(string value, out ReleaseVersion version)
        {
            var match = Pattern.Match(value);
            if (!match.Success
                || !int.TryParse(match.Groups["major"].Value, out var major)
                || !int.TryParse(match.Groups["minor"].Value, out var minor)
                || !int.TryParse(match.Groups["patch"].Value, out var patch))
            {
                version = null!;
                return false;
            }

            var betaNumber = match.Groups["beta"].Success && int.TryParse(match.Groups["beta"].Value, out var parsedBeta)
                ? parsedBeta
                : int.MaxValue;
            version = new ReleaseVersion(major, minor, patch, betaNumber);
            return true;
        }

        public static ReleaseVersion Parse(string value) => TryParse(value, out var version)
            ? version
            : throw new FormatException("Invalid release version.");

        public int CompareTo(ReleaseVersion? other)
        {
            if (other is null)
            {
                return 1;
            }

            return Comparer<(int Major, int Minor, int Patch, int BetaNumber)>.Default.Compare(
                (Major, Minor, Patch, BetaNumber),
                (other.Major, other.Minor, other.Patch, other.BetaNumber));
        }
    }
}
