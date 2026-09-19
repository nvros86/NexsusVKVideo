using System.Collections.ObjectModel;
using System.Windows.Input;
using NexsusVKVideo.App.Models;
using NexsusVKVideo.App.Services;
using NexsusVKVideo.Core.Contracts;
using NexsusVKVideo.Core.Models;

namespace NexsusVKVideo.App.ViewModels;

public abstract class PageViewModel(string title, string description) : ObservableObject
{
    public string Title { get; } = title;

    public string Description { get; } = description;
}

public sealed class HomePageViewModel(ICommand openVideoCommand) : PageViewModel(
    "Главная",
    "Демонстрационный каталог — это не данные VK и не доступный для просмотра контент.")
{
    public ICommand OpenVideoCommand { get; } = openVideoCommand;

    public ReadOnlyCollection<DemoVideo> Videos { get; } = new List<DemoVideo>
    {
        new(
            "Виджет VK Video: пример документации",
            "Официальный пример VK для разработчиков",
            "VK widget",
            "Тестовая карточка использует URL из официальной документации VK. Это не каталог приложения.",
            "VK WIDGET",
            new Uri("https://vk.ru/video_ext.php?oid=-22822305&id=456241864&hd=2"),
            new Uri("https://vk.ru/video-22822305_456241864"),
            new VideoSummary(new VideoKey("vk", "-22822305_456241864"), "Виджет VK Video: пример документации")),
        new("Северный горизонт", "Демонстрационный автор", "12:34", "Пример карточки для проверки компоновки."),
        new("Город после дождя", "Демонстрационный автор", "08:21", "Пример карточки для проверки навигации."),
        new("Тихое место", "Демонстрационный автор", "15:03", "Пример карточки без поставщика и внешней ссылки."),
        new("Путь к свету", "Демонстрационный автор", "14:20", "Пример подписанного демонстрационного контента."),
        new("Сезон перемен", "Демонстрационный автор", "09:46", "Пример состояния недоступного воспроизведения."),
        new("Скрытые берега", "Демонстрационный автор", "11:28", "Пример карточки для проверки tab-навигации.")
    }.AsReadOnly();
}

public sealed class PlaceholderPageViewModel(string title, string description, string actionHint) : PageViewModel(title, description)
{
    public string ActionHint { get; } = actionHint;
}

public abstract class LocalDataPageViewModel(string title, string description) : PageViewModel(title, description)
{
    private bool _isLoading;
    private string? _message;

    public bool IsLoading
    {
        get => _isLoading;
        protected set => SetProperty(ref _isLoading, value);
    }

    public string? Message
    {
        get => _message;
        protected set => SetProperty(ref _message, value);
    }

    protected void ReportFailure(Exception exception)
    {
        IsLoading = false;
        Message = "Не удалось открыть локальные данные. Попробуйте обновить список.";
    }
}

public sealed class LibraryPageViewModel : LocalDataPageViewModel
{
    private readonly IHistoryRepository _historyRepository;

    public LibraryPageViewModel(IHistoryRepository historyRepository)
        : base("Библиотека", "Локальная история открытых поддерживаемых видео хранится только на этом устройстве.")
    {
        _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
        RefreshCommand = new AsyncRelayCommand(RefreshAsync, ReportFailure);
        ClearCommand = new AsyncRelayCommand(ClearAsync, ReportFailure);
    }

    public ObservableCollection<HistoryEntry> Entries { get; } = [];

    public bool HasEntries => Entries.Count > 0;

    public ICommand RefreshCommand { get; }

    public ICommand ClearCommand { get; }

    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        Message = null;
        var entries = await _historyRepository.GetRecentAsync(100, cancellationToken);
        ReplaceEntries(entries);
        IsLoading = false;
        Message = HasEntries ? null : "История пока пуста. Откройте поддерживаемое видео, чтобы оно появилось здесь.";
    }

    private async Task ClearAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        await _historyRepository.ClearAsync(cancellationToken);
        ReplaceEntries([]);
        IsLoading = false;
        Message = "История очищена на этом устройстве.";
    }

    private void ReplaceEntries(IReadOnlyList<HistoryEntry> entries)
    {
        Entries.Clear();
        foreach (var entry in entries)
        {
            Entries.Add(entry);
        }

        OnPropertyChanged(nameof(HasEntries));
    }
}

public sealed class FavoritesPageViewModel : LocalDataPageViewModel
{
    private readonly IFavoritesRepository _favoritesRepository;

    public FavoritesPageViewModel(IFavoritesRepository favoritesRepository)
        : base("Избранное", "Сохранённые видео доступны только в локальном профиле этого устройства.")
    {
        _favoritesRepository = favoritesRepository ?? throw new ArgumentNullException(nameof(favoritesRepository));
        RefreshCommand = new AsyncRelayCommand(RefreshAsync, ReportFailure);
        RemoveCommand = new AsyncRelayCommand<VideoSummary>(RemoveAsync, ReportFailure);
    }

    public ObservableCollection<VideoSummary> Videos { get; } = [];

    public bool HasVideos => Videos.Count > 0;

    public ICommand RefreshCommand { get; }

    public ICommand RemoveCommand { get; }

    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        Message = null;
        var favorites = await _favoritesRepository.GetAllAsync(cancellationToken);
        ReplaceVideos(favorites);
        IsLoading = false;
        Message = HasVideos ? null : "В избранном пока нет видео.";
    }

    private async Task RemoveAsync(VideoSummary video, CancellationToken cancellationToken)
    {
        IsLoading = true;
        await _favoritesRepository.RemoveAsync(video.Key, cancellationToken);
        Videos.Remove(video);
        OnPropertyChanged(nameof(HasVideos));
        IsLoading = false;
        Message = HasVideos ? null : "В избранном пока нет видео.";
    }

    private void ReplaceVideos(IReadOnlyList<VideoSummary> videos)
    {
        Videos.Clear();
        foreach (var video in videos)
        {
            Videos.Add(video);
        }

        OnPropertyChanged(nameof(HasVideos));
    }
}

public sealed class SettingsPageViewModel : LocalDataPageViewModel
{
    private const int SettingsSchemaVersion = 1;
    private readonly ISettingsStore _settingsStore;
    private readonly IHistoryRepository _historyRepository;
    private readonly IThemeService _themeService;
    private readonly IWebViewProfileResetScheduler _webViewProfileResetScheduler;
    private string _selectedTheme = "Dark";

    public SettingsPageViewModel(
        ISettingsStore settingsStore,
        IHistoryRepository historyRepository,
        IThemeService themeService,
        IWebViewProfileResetScheduler webViewProfileResetScheduler)
        : base("Настройки", "Предпочтения и локальные данные управляются только на этом устройстве.")
    {
        _settingsStore = settingsStore ?? throw new ArgumentNullException(nameof(settingsStore));
        _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _webViewProfileResetScheduler = webViewProfileResetScheduler ?? throw new ArgumentNullException(nameof(webViewProfileResetScheduler));
        LoadCommand = new AsyncRelayCommand(LoadAsync, ReportFailure);
        SaveThemeCommand = new AsyncRelayCommand(SaveThemeAsync, ReportFailure);
        ClearHistoryCommand = new AsyncRelayCommand(ClearHistoryAsync, ReportFailure);
        ScheduleWebViewResetCommand = new AsyncRelayCommand(ScheduleWebViewResetAsync, ReportFailure);
    }

    public ReadOnlyCollection<string> ThemeOptions { get; } = new List<string> { "Dark", "Light" }.AsReadOnly();

    public string SelectedTheme
    {
        get => _selectedTheme;
        set => SetProperty(ref _selectedTheme, value);
    }

    public ICommand LoadCommand { get; }

    public ICommand SaveThemeCommand { get; }

    public ICommand ClearHistoryCommand { get; }

    public ICommand ScheduleWebViewResetCommand { get; }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        Message = null;
        var settings = await _settingsStore.LoadAsync(cancellationToken);
        SelectedTheme = ThemeOptions.Contains(settings.Theme) ? settings.Theme : "Dark";
        _themeService.Apply(SelectedTheme);
        IsLoading = false;
    }

    private async Task SaveThemeAsync(CancellationToken cancellationToken)
    {
        if (!ThemeOptions.Contains(SelectedTheme))
        {
            throw new ArgumentOutOfRangeException(nameof(SelectedTheme), "Unsupported theme.");
        }

        IsLoading = true;
        _themeService.Apply(SelectedTheme);
        await _settingsStore.SaveAsync(new AppSettings(SettingsSchemaVersion, SelectedTheme), cancellationToken);
        IsLoading = false;
        Message = "Тема применена и сохранена на этом устройстве.";
    }

    private async Task ClearHistoryAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        await _historyRepository.ClearAsync(cancellationToken);
        IsLoading = false;
        Message = "История очищена на этом устройстве.";
    }

    private async Task ScheduleWebViewResetAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        await _webViewProfileResetScheduler.ScheduleAsync(cancellationToken);
        IsLoading = false;
        Message = "Кэш и сессии встроенного плеера будут очищены при следующем запуске приложения.";
    }
}

public sealed class PlayerViewModel : ObservableObject
{
    private string _status = "Ожидание";
    private string _title = "Плеер";
    private string _message = "Выберите видео из доступного поставщика, чтобы открыть поддерживаемый playback descriptor.";
    private bool _isUnavailable;
    private Uri? _embedUri;
    private Uri? _browserUri;
    private VideoSummary? _currentVideo;
    private bool _isFavorite;
    private string? _dataMessage;
    private readonly RelayCommand _openInBrowserCommand;

    public PlayerViewModel()
    {
        ClearCommand = new RelayCommand(Clear);
        _openInBrowserCommand = new RelayCommand(RequestBrowser, () => BrowserUri is not null);
        OpenInBrowserCommand = _openInBrowserCommand;
    }

    public string Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public string Title
    {
        get => _title;
        private set => SetProperty(ref _title, value);
    }

    public string Message
    {
        get => _message;
        private set => SetProperty(ref _message, value);
    }

    public bool IsUnavailable
    {
        get => _isUnavailable;
        private set => SetProperty(ref _isUnavailable, value);
    }

    public Uri? EmbedUri
    {
        get => _embedUri;
        private set
        {
            if (SetProperty(ref _embedUri, value))
            {
                OnPropertyChanged(nameof(HasEmbed));
            }
        }
    }

    public Uri? BrowserUri
    {
        get => _browserUri;
        private set
        {
            if (SetProperty(ref _browserUri, value))
            {
                _openInBrowserCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool HasEmbed => EmbedUri is not null;

    public VideoSummary? CurrentVideo
    {
        get => _currentVideo;
        private set
        {
            if (SetProperty(ref _currentVideo, value))
            {
                OnPropertyChanged(nameof(HasCurrentVideo));
            }
        }
    }

    public bool HasCurrentVideo => CurrentVideo is not null;

    public bool IsFavorite
    {
        get => _isFavorite;
        private set
        {
            if (SetProperty(ref _isFavorite, value))
            {
                OnPropertyChanged(nameof(FavoriteButtonText));
            }
        }
    }

    public string FavoriteButtonText => IsFavorite ? "Убрать из избранного" : "Добавить в избранное";

    public string? DataMessage
    {
        get => _dataMessage;
        private set => SetProperty(ref _dataMessage, value);
    }

    public event EventHandler<Uri>? BrowserRequested;

    public ICommand ClearCommand { get; }

    public ICommand OpenInBrowserCommand { get; }

    public void OpenDemo(DemoVideo video)
    {
        Title = video.Title;
        EmbedUri = video.EmbedUri;
        BrowserUri = video.BrowserUri;
        CurrentVideo = video.LocalVideo;
        IsFavorite = false;
        DataMessage = null;

        if (EmbedUri is not null && BrowserUri is not null)
        {
            Status = "Загрузка виджета VK";
            IsUnavailable = false;
            Message = "Открывается официальный пример iframe из документации VK. Приложение не получает прямой поток, токены или cookies.";
            return;
        }

        Status = "Недоступно";
        IsUnavailable = true;
        Message = "Карточка помечена как демонстрационная и не содержит подтверждённого VK playback descriptor. Воспроизведение и переход в браузер намеренно недоступны.";
    }

    public void ReportNavigationFailure(string message)
    {
        Status = "Плеер недоступен";
        IsUnavailable = true;
        Message = message;
    }

    public void ReportNavigationReady()
    {
        Status = "Плеер VK открыт";
        IsUnavailable = false;
        Message = "Видео отображается внутри приложения через официальный VK Video widget. Используйте браузер, если виджету требуется внешний сценарий.";
    }

    public void SetFavorite(bool isFavorite)
    {
        IsFavorite = isFavorite;
        DataMessage = null;
    }

    public void ReportDataFailure()
    {
        DataMessage = "Не удалось обновить локальные данные. Попробуйте ещё раз.";
    }

    private void Clear()
    {
        Title = "Плеер";
        Status = "Ожидание";
        IsUnavailable = false;
        EmbedUri = null;
        BrowserUri = null;
        CurrentVideo = null;
        IsFavorite = false;
        DataMessage = null;
        Message = "Выберите видео из доступного поставщика, чтобы открыть поддерживаемый playback descriptor.";
    }

    private void RequestBrowser()
    {
        if (BrowserUri is not null)
        {
            BrowserRequested?.Invoke(this, BrowserUri);
        }
    }
}
