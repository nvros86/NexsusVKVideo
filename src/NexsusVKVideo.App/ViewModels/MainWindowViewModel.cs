using System.Collections.ObjectModel;
using System.Windows.Input;
using NexsusVKVideo.App.Models;

namespace NexsusVKVideo.App.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly HomePageViewModel _homePage;
    private NavigationItemViewModel? _selectedNavigation;
    private PageViewModel _currentPage;

    public MainWindowViewModel()
    {
        Player = new PlayerViewModel();
        OpenDemoCommand = new RelayCommand<DemoVideo>(OpenDemo);
        _homePage = new HomePageViewModel(OpenDemoCommand);
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

    private void OpenDemo(DemoVideo video) => Player.OpenDemo(video);

    private PageViewModel CreatePage(string route) => route switch
    {
        "home" => _homePage,
        "search" => new PlaceholderPageViewModel("Поиск", "Поиск будет добавлен после подтверждения документированного способа доступа к данным поставщика.", "API-запросы и live search не выполняются в демонстрационном каркасе."),
        "library" => new PlaceholderPageViewModel("Библиотека", "Локальная история появится на этапе 0.2.", "В этой версии никакие данные просмотра не сохраняются."),
        "favorites" => new PlaceholderPageViewModel("Избранное", "Хранилище избранного появится на этапе 0.2.", "Core уже фиксирует правило уникальности provider + videoId."),
        "ai" => new PlaceholderPageViewModel("AI Center", "Скоро", "AI-функции и фоновые запросы не выполняются."),
        "settings" => new PlaceholderPageViewModel("Настройки", "Тема, приватность и очистка данных появятся на этапе 0.2.", "Настройки не сохраняются в этом демонстрационном каркасе."),
        _ => _homePage
    };
}
