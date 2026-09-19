namespace NexsusVKVideo.Infrastructure.Storage;

public static class ApplicationDataPaths
{
    public static string GetApplicationDataDirectory() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "NexsusVKVideo");

    public static string GetDatabasePath() => Path.Combine(GetApplicationDataDirectory(), "data", "nexsusvkvideo.db");

    public static string GetWebViewUserDataPath() => Path.Combine(GetApplicationDataDirectory(), "WebView2");

    public static string GetWebViewProfileResetMarkerPath() => Path.Combine(GetApplicationDataDirectory(), "data", "webview2-reset.pending");
}
