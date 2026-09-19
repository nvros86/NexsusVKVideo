namespace NexsusVKVideo.App.Models;

public sealed record DemoVideo(
    string Title,
    string Creator,
    string Duration,
    string Description,
    string Badge = "ДЕМО",
    Uri? EmbedUri = null,
    Uri? BrowserUri = null);
