using System.Windows;
using NexsusVKVideo.Infrastructure.Storage;

namespace NexsusVKVideo.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ApplyScheduledWebViewReset();
        new MainWindow().Show();
    }

    private static void ApplyScheduledWebViewReset()
    {
        try
        {
            var resetScheduler = new WebViewProfileResetScheduler(
                ApplicationDataPaths.GetWebViewUserDataPath(),
                ApplicationDataPaths.GetWebViewProfileResetMarkerPath());
            resetScheduler.ApplyScheduledReset();
        }
        catch (Exception exception)
        {
            System.Diagnostics.Trace.TraceWarning($"Could not clear the scheduled WebView2 profile: {exception.GetType().Name}");
        }
    }
}
