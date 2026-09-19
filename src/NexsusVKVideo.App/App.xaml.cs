using Microsoft.Extensions.DependencyInjection;
using NexsusVKVideo.App.ViewModels;
using NexsusVKVideo.Core.Contracts;
using NexsusVKVideo.Infrastructure.Storage;
using System.Windows;

namespace NexsusVKVideo.App;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _serviceProvider = new ServiceCollection()
            .AddSingleton<SqliteLocalDataStore>(_ => new SqliteLocalDataStore(ApplicationDataPaths.GetDatabasePath()))
            .AddSingleton<IHistoryRepository>(services => services.GetRequiredService<SqliteLocalDataStore>())
            .AddSingleton<IFavoritesRepository>(services => services.GetRequiredService<SqliteLocalDataStore>())
            .AddSingleton<ISettingsStore>(services => services.GetRequiredService<SqliteLocalDataStore>())
            .AddSingleton<MainWindowViewModel>()
            .AddSingleton<MainWindow>()
            .BuildServiceProvider();

        _serviceProvider.GetRequiredService<MainWindow>().Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
