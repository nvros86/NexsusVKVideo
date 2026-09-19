using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using NexsusVKVideo.App.Models;
using NexsusVKVideo.App.Services;
using NexsusVKVideo.Core.Contracts;
using NexsusVKVideo.Core.Models;

namespace NexsusVKVideo.App.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly HomePageViewModel _homePage;
    private readonly LibraryPageViewModel _libraryPage;
    private readonly FavoritesPageViewModel _favoritesPage;
    private readonly SettingsPageViewModel _settingsPage;
    private readonly IHistoryRepository _historyRepository;
    private readonly IFavoritesRepository _favoritesRepository;
    private readonly AsyncRelayCommand<DemoVideo> _openDemoCommand;
    private readonly AsyncRelayCommand<VideoSummary> _toggleFavoriteCommand;
    private NavigationItemViewModel? _selectedNavigation;
    private PageViewModel _currentPage;

    public MainWindowViewModel(
        IHistoryRepository historyRepository,
        IFavoritesRepository favoritesRepository,
        ISettingsStore settingsStore,
        IThemeService themeService,
        IWebViewProfileResetScheduler webViewProfileResetScheduler)
    {
        _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
        _favoritesRepository = favoritesRepository ?? throw new ArgumentNullException(nameof(favoritesRepository));
        Player = new PlayerViewModel();
        _openDemoCommand = new AsyncRelayCommand<DemoVideo>(OpenDemoAsync, _ => Player.ReportDataFailure());
        _toggleFavoriteCommand = new AsyncRelayCommand<VideoSummary>(ToggleFavoriteAsync, _ => Player.ReportDataFailure());
        OpenDemoCommand = _openDemoCommand;
        ToggleFavoriteCommand = _toggleFavoriteCommand;
        _homePage = new HomePageViewModel(_openDemoCommand);
        _libraryPage = new LibraryPageViewModel(_historyRepository);
        _favoritesPage = new FavoritesPageViewModel(_favoritesRepository);
        _settingsPage = new SettingsPageViewModel(settingsStore, _historyRepository, themeService, webViewProfileResetScheduler);
        Player.PropertyChanged += OnPlayerPropertyChanged;
        NavigationItems = new ObservableCollection<NavigationItemViewModel>
        {
            new("home", "Главная", "⌂"),
            new("search", "Поиск", "⌕"),
            new("library", "Библиотека", "▣"),
            new("favorites", "Избранное", "♡"),
            new("ai", "AI Center", "✦"),
            new("settings", "Настройки", "⚙")
        };

        _currentPage = _homePage;
        _selectedNavigation = NavigationItems[0];
    }

    public string Title => "NexsusVKVideo";

    public ObservableCollection<NavigationItemViewModel> NavigationItems { get; }

    public NavigationItemViewModel? SelectedNavigation
    {
        get => _selectedNavigation;
        set
        {
            if (SetProperty(ref _selectedNavigation, value) && value is not null)
            {
                CurrentPage = CreatePage(value.Route);
            }
        }
    }

    public PageViewModel CurrentPage
    {
        get => _currentPage;
        private set => SetProperty(ref _currentPage, value);
    }

    public PlayerViewModel Player { get; }

    public ICommand OpenDemoCommand { get; }

    public ICommand ToggleFavoriteCommand { get; }

    private async Task OpenDemoAsync(DemoVideo video, CancellationToken cancellationToken)
    {
        Player.OpenDemo(video);
        if (video.LocalVideo is null)
        {
            return;
        }

        await _historyRepository.AddAsync(new HistoryEntry(video.LocalVideo, DateTimeOffset.UtcNow), cancellationToken);
        var favorites = await _favoritesRepository.GetAllAsync(cancellationToken);
        Player.SetFavorite(favorites.Any(candidate => candidate.Key == video.LocalVideo.Key));
    }

    private async Task ToggleFavoriteAsync(VideoSummary video, CancellationToken cancellationToken)
    {
        if (Player.IsFavorite)
        {
            await _favoritesRepository.RemoveAsync(video.Key, cancellationToken);
            Player.SetFavorite(false);
            return;
        }

        await _favoritesRepository.AddAsync(video, cancellationToken);
        Player.SetFavorite(true);
    }

    private void OnPlayerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PlayerViewModel.CurrentVideo))
        {
            _toggleFavoriteCommand.RaiseCanExecuteChanged();
        }
    }

    private PageViewModel CreatePage(string route) => route switch
    {
        "home" => _homePage,
        "search" => new PlaceholderPageViewModel("Поиск", "Поиск будет добавлен после подтверждения документированного способа доступа к данным поставщика.", "API-запросы и live search не выполняются в демонстрационном каркасе."),
        "library" => LoadPage(_libraryPage),
        "favorites" => LoadPage(_favoritesPage),
        "ai" => new PlaceholderPageViewModel("AI Center", "Скоро", "AI-функции и фоновые запросы не выполняются."),
        "settings" => LoadPage(_settingsPage),
        _ => _homePage
    };

    private static PageViewModel LoadPage(LibraryPageViewModel page)
    {
        page.RefreshCommand.Execute(null);
        return page;
    }

    private static PageViewModel LoadPage(FavoritesPageViewModel page)
    {
        page.RefreshCommand.Execute(null);
        return page;
    }

    private static PageViewModel LoadPage(SettingsPageViewModel page)
    {
        page.LoadCommand.Execute(null);
        return page;
    }
}
