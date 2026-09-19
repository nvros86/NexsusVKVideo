using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using NexsusVKVideo.App.Models;
using NexsusVKVideo.App.Services;
using NexsusVKVideo.Core.Contracts;
using NexsusVKVideo.Core.Models;
using NexsusVKVideo.Infrastructure.Vk;

namespace NexsusVKVideo.App.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly HomePageViewModel _homePage;
    private readonly LibraryPageViewModel _libraryPage;
    private readonly FavoritesPageViewModel _favoritesPage;
    private readonly SettingsPageViewModel _settingsPage;
    private readonly VkVideoBrowserPageViewModel _vkVideoBrowserPage;
    private readonly ImportPageViewModel _importPage;
    private readonly IHistoryRepository _historyRepository;
    private readonly IFavoritesRepository _favoritesRepository;
    private readonly VkVideoLinkParser _vkVideoLinkParser;
    private readonly AsyncRelayCommand<DemoVideo> _openDemoCommand;
    private readonly AsyncRelayCommand<VideoSummary> _toggleFavoriteCommand;
    private NavigationItemViewModel? _selectedNavigation;
    private PageViewModel _currentPage;
    private bool _isBrowserMode;

    public MainWindowViewModel(
        IHistoryRepository historyRepository,
        IFavoritesRepository favoritesRepository,
        ISettingsStore settingsStore,
        IThemeService themeService,
        IWebViewProfileResetScheduler webViewProfileResetScheduler,
        VkVideoLinkParser vkVideoLinkParser)
    {
        _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
        _favoritesRepository = favoritesRepository ?? throw new ArgumentNullException(nameof(favoritesRepository));
        _vkVideoLinkParser = vkVideoLinkParser ?? throw new ArgumentNullException(nameof(vkVideoLinkParser));
        Player = new PlayerViewModel();
        _openDemoCommand = new AsyncRelayCommand<DemoVideo>(OpenDemoAsync, _ => Player.ReportDataFailure());
        _toggleFavoriteCommand = new AsyncRelayCommand<VideoSummary>(ToggleFavoriteAsync, _ => Player.ReportDataFailure());
        OpenDemoCommand = _openDemoCommand;
        ToggleFavoriteCommand = _toggleFavoriteCommand;
        _homePage = new HomePageViewModel(_openDemoCommand);
        _libraryPage = new LibraryPageViewModel(_historyRepository);
        _favoritesPage = new FavoritesPageViewModel(_favoritesRepository);
        _settingsPage = new SettingsPageViewModel(settingsStore, _historyRepository, themeService, webViewProfileResetScheduler);
        _vkVideoBrowserPage = new VkVideoBrowserPageViewModel();
        _importPage = new ImportPageViewModel(ImportVideoLinkAsync);
        Player.PropertyChanged += OnPlayerPropertyChanged;
        NavigationItems = new ObservableCollection<NavigationItemViewModel>
        {
            new("home", "Главная", "⌂"),
            new("browser", "VK Video", "▶"),
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
        private set
        {
            if (SetProperty(ref _currentPage, value))
            {
                IsBrowserMode = value is VkVideoBrowserPageViewModel;
            }
        }
    }

    public bool IsBrowserMode
    {
        get => _isBrowserMode;
        private set
        {
            if (SetProperty(ref _isBrowserMode, value) && value)
            {
                OnPropertyChanged(nameof(IsMainShellVisible));
                BrowserRequested?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                OnPropertyChanged(nameof(IsMainShellVisible));
            }
        }
    }

    public bool IsMainShellVisible => !IsBrowserMode;

    public PlayerViewModel Player { get; }

    public ICommand OpenDemoCommand { get; }

    public ICommand ToggleFavoriteCommand { get; }

    public event EventHandler? BrowserRequested;

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

    private async Task ImportVideoLinkAsync(string value, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || !_vkVideoLinkParser.TryParse(uri, out var videoLink)
            || videoLink is null)
        {
            _importPage.ReportInvalidLink();
            return;
        }

        var video = new DemoVideo(
            $"VK Video {videoLink.Key.VideoId}",
            "Публичная ссылка VK Video",
            "",
            "Открыто по публичной ссылке. Метаданные не загружаются без документированного доступа к API.",
            "VK VIDEO",
            videoLink.EmbedUri,
            videoLink.VideoPageUri,
            new VideoSummary(videoLink.Key, $"VK Video {videoLink.Key.VideoId}"));

        await OpenDemoAsync(video, cancellationToken);
        _importPage.ReportOpened();
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
        "browser" => _vkVideoBrowserPage,
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
