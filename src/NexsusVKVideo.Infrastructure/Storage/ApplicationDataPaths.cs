namespace NexsusVKVideo.Infrastructure.Storage;

public static class ApplicationDataPaths
{
    public static string GetDatabasePath() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "NexsusVKVideo",
        "data",
        "nexsusvkvideo.db");
}
