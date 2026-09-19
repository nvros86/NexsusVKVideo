namespace NexsusVKVideo.Core.Models;

public sealed record VideoSearchRequest
{
    public VideoSearchRequest(string query, int page = 1, int pageSize = 24)
    {
        Query = string.IsNullOrWhiteSpace(query)
            ? throw new ArgumentException("Query must not be empty.", nameof(query))
            : query.Trim();

        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page));
        }

        if (pageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        Page = page;
        PageSize = pageSize;
    }

    public string Query { get; }

    public int Page { get; }

    public int PageSize { get; }
}
