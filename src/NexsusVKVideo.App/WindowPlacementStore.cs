using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace NexsusVKVideo.App;

internal sealed record WindowPlacement(double Left, double Top, double Width, double Height, bool IsMaximized);

internal sealed class WindowPlacementStore
{
    private readonly string _path;

    public WindowPlacementStore(string path)
    {
        _path = path ?? throw new ArgumentNullException(nameof(path));
    }

    public WindowPlacement? Load()
    {
        try
        {
            if (!File.Exists(_path))
            {
                return null;
            }

            return JsonSerializer.Deserialize<WindowPlacement>(File.ReadAllText(_path));
        }
        catch (Exception exception) when (exception is IOException or JsonException or UnauthorizedAccessException)
        {
            Trace.TraceWarning($"Could not load window placement: {exception.GetType().Name}");
            return null;
        }
    }

    public void Save(WindowPlacement placement)
    {
        try
        {
            var directory = Path.GetDirectoryName(_path);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_path, JsonSerializer.Serialize(placement));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            Trace.TraceWarning($"Could not save window placement: {exception.GetType().Name}");
        }
    }
}
