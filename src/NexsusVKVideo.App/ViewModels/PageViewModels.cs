using System.Collections.ObjectModel;
using System.Windows.Input;
using NexsusVKVideo.App.Models;

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
            new Uri("https://vk.ru/video-22822305_456241864")),
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

public sealed class PlayerViewModel : ObservableObject
{
    private string _status = "Ожидание";
    private string _title = "Плеер";
    private string _message = "Выберите видео из доступного поставщика, чтобы открыть поддерживаемый playback descriptor.";
    private bool _isUnavailable;
    private Uri? _embedUri;
    private Uri? _browserUri;
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

    public event EventHandler<Uri>? BrowserRequested;

    public ICommand ClearCommand { get; }

    public ICommand OpenInBrowserCommand { get; }

    public void OpenDemo(DemoVideo video)
    {
        Title = video.Title;
        EmbedUri = video.EmbedUri;
        BrowserUri = video.BrowserUri;

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

    private void Clear()
    {
        Title = "Плеер";
        Status = "Ожидание";
        IsUnavailable = false;
        EmbedUri = null;
        BrowserUri = null;
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
