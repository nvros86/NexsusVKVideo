using Microsoft.Extensions.DependencyInjection;
using NexsusVKVideo.App.Services;
using NexsusVKVideo.App.ViewModels;
using NexsusVKVideo.Core.Contracts;
using NexsusVKVideo.Infrastructure.Storage;
using NexsusVKVideo.Infrastructure.Vk;
using System.Windows;

namespace NexsusVKVideo.App;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _serviceProvider = new ServiceCollection()
            .AddSingleton<SqliteLocalDataStore>(_ => new SqliteLocalDataStore(ApplicationDataPaths.GetDatabasePath()))
            .AddSingleton<IHistoryRepository>(services => services.GetRequiredService<SqliteLocalDataStore>())
            .AddSingleton<IFavoritesRepository>(services => services.GetRequiredService<SqliteLocalDataStore>())
            .AddSingleton<ISettingsStore>(services => services.GetRequiredService<SqliteLocalDataStore>())
            .AddSingleton<IThemeService>(_ => new WpfThemeService(this))
            .AddSingleton<WebViewProfileResetScheduler>(_ => new WebViewProfileResetScheduler(
                ApplicationDataPaths.GetWebViewUserDataPath(),
                ApplicationDataPaths.GetWebViewProfileResetMarkerPath()))
            .AddSingleton<IWebViewProfileResetScheduler>(services => services.GetRequiredService<WebViewProfileResetScheduler>())
            .AddSingleton<VkVideoLinkParser>()
            .AddSingleton<MainWindowViewModel>()
            .AddSingleton<MainWindow>()
            .BuildServiceProvider();

        ApplyScheduledWebViewReset(_serviceProvider.GetRequiredService<WebViewProfileResetScheduler>());
        _serviceProvider.GetRequiredService<MainWindow>().Show();
        await ApplyPersistedThemeAsync(_serviceProvider);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }

    private static async Task ApplyPersistedThemeAsync(ServiceProvider serviceProvider)
    {
        try
        {
            var settings = await serviceProvider.GetRequiredService<ISettingsStore>().LoadAsync(CancellationToken.None);
            if (settings.Theme is "Dark" or "Light")
            {
                serviceProvider.GetRequiredService<IThemeService>().Apply(settings.Theme);
            }
        }
        catch (Exception exception)
        {
            System.Diagnostics.Trace.TraceWarning($"Could not apply persisted theme: {exception.GetType().Name}");
        }
    }

    private static void ApplyScheduledWebViewReset(WebViewProfileResetScheduler resetScheduler)
    {
        try
        {
            resetScheduler.ApplyScheduledReset();
        }
        catch (Exception exception)
        {
            System.Diagnostics.Trace.TraceWarning($"Could not clear the scheduled WebView2 profile: {exception.GetType().Name}");
        }
    }
}
