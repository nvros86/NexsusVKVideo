using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using NexsusVKVideo.App.ViewModels;
using NexsusVKVideo.Infrastructure.Storage;
using NexsusVKVideo.Infrastructure.Vk;

namespace NexsusVKVideo.App;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private bool _webViewInitialized;
    private bool _browserWebViewInitialized;
    private bool _browserHomeRequested;

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        FitInitialWindowToWorkArea();
        _viewModel = viewModel;
        DataContext = _viewModel;
        Loaded += OnLoaded;
        Closed += OnClosed;
        _viewModel.Player.PropertyChanged += OnPlayerPropertyChanged;
        _viewModel.Player.BrowserRequested += OnBrowserRequested;
        _viewModel.BrowserRequested += OnVkVideoBrowserRequested;
    }

    private void FitInitialWindowToWorkArea()
    {
        const double edgePadding = 16;
        var workArea = SystemParameters.WorkArea;
        var maximumWidth = Math.Max(1, workArea.Width - (edgePadding * 2));
        var maximumHeight = Math.Max(1, workArea.Height - (edgePadding * 2));

        MaxWidth = maximumWidth;
        MaxHeight = maximumHeight;
        MinWidth = Math.Min(MinWidth, maximumWidth);
        MinHeight = Math.Min(MinHeight, maximumHeight);
        Width = Math.Clamp(Width, MinWidth, maximumWidth);
        Height = Math.Clamp(Height, MinHeight, maximumHeight);
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var userDataFolder = ApplicationDataPaths.GetWebViewUserDataPath();
            Directory.CreateDirectory(userDataFolder);

            var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
            await PlayerWebView.EnsureCoreWebView2Async(environment);
            await BrowserWebView.EnsureCoreWebView2Async(environment);
            PlayerWebView.CoreWebView2.NavigationStarting += OnNavigationStarting;
            PlayerWebView.CoreWebView2.NavigationCompleted += OnNavigationCompleted;
            BrowserWebView.CoreWebView2.NavigationStarting += OnBrowserNavigationStarting;
            BrowserWebView.CoreWebView2.NavigationCompleted += OnBrowserNavigationCompleted;
            BrowserWebView.CoreWebView2.NewWindowRequested += OnBrowserNewWindowRequested;
            _webViewInitialized = true;
            _browserWebViewInitialized = true;
            NavigateToCurrentEmbed();
            NavigateBrowserHomeIfRequested();
        }
        catch (WebView2RuntimeNotFoundException)
        {
            _viewModel.Player.ReportNavigationFailure("Среда выполнения Microsoft Edge WebView2 не найдена. Установите Evergreen Runtime или откройте видео в системном браузере.");
        }
        catch (Exception)
        {
            _viewModel.Player.ReportNavigationFailure("Не удалось инициализировать встроенный плеер. Можно открыть подтверждённую страницу видео в системном браузере.");
        }
    }

    private void OnPlayerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PlayerViewModel.EmbedUri))
        {
            NavigateToCurrentEmbed();
        }
    }

    private void OnVkVideoBrowserRequested(object? sender, EventArgs e)
    {
        _browserHomeRequested = true;
        NavigateBrowserHomeIfRequested();
    }

    private void NavigateBrowserHomeIfRequested()
    {
        if (!_browserHomeRequested || !_browserWebViewInitialized || BrowserWebView.CoreWebView2 is null)
        {
            return;
        }

        _browserHomeRequested = false;
        if (_viewModel.CurrentPage is VkVideoBrowserPageViewModel page)
        {
            page.ClearMessage();
        }

        BrowserWebView.CoreWebView2.Navigate(VkVideoBrowserPageViewModel.HomeUri.AbsoluteUri);
    }

    private void NavigateToCurrentEmbed()
    {
        if (!_webViewInitialized || PlayerWebView.CoreWebView2 is null)
        {
            return;
        }

        var embedUri = _viewModel.Player.EmbedUri;
        if (embedUri is null)
        {
            PlayerWebView.CoreWebView2.Navigate("about:blank");
            return;
        }

        if (!VkEmbedUriValidator.IsAllowedEmbed(embedUri))
        {
            _viewModel.Player.ReportNavigationFailure("Ссылка плеера отклонена политикой безопасности. Разрешены только HTTPS-виджеты vk.ru/video_ext.php с oid и id.");
            return;
        }

        PlayerWebView.CoreWebView2.Navigate(embedUri.AbsoluteUri);
    }

    private void OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        if (string.Equals(e.Uri, "about:blank", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!Uri.TryCreate(e.Uri, UriKind.Absolute, out var target) || !VkEmbedUriValidator.IsAllowedEmbed(target))
        {
            e.Cancel = true;
            _viewModel.Player.ReportNavigationFailure("Плеер заблокировал переход за пределы разрешённого VK widget URL.");
        }
    }

    private void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (e.IsSuccess && _viewModel.Player.EmbedUri is not null)
        {
            _viewModel.Player.ReportNavigationReady();
        }
        else if (!e.IsSuccess && _viewModel.Player.EmbedUri is not null)
        {
            _viewModel.Player.ReportNavigationFailure($"VK widget не загрузился ({e.WebErrorStatus}). Откройте видео в системном браузере.");
        }
    }

    private void OnBrowserNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        if (Uri.TryCreate(e.Uri, UriKind.Absolute, out var target) && VkEmbedUriValidator.IsAllowedVkSite(target))
        {
            return;
        }

        e.Cancel = true;
        ReportBrowserNavigationFailure("Переход заблокирован: встроенный браузер открывает только защищённые сайты VK.");
    }

    private void OnBrowserNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (!e.IsSuccess)
        {
            ReportBrowserNavigationFailure($"Официальный сайт VK Video не загрузился ({e.WebErrorStatus}). Проверьте подключение и повторите попытку.");
        }
    }

    private void OnBrowserNewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
    {
        e.Handled = true;
        if (Uri.TryCreate(e.Uri, UriKind.Absolute, out var target) && VkEmbedUriValidator.IsAllowedVkSite(target))
        {
            BrowserWebView.CoreWebView2?.Navigate(target.AbsoluteUri);
            return;
        }

        ReportBrowserNavigationFailure("Новое окно заблокировано: разрешены только защищённые сайты VK.");
    }

    private void OnReloadBrowserClick(object sender, RoutedEventArgs e)
    {
        if (_browserWebViewInitialized)
        {
            BrowserWebView.CoreWebView2?.Reload();
        }
    }

    private void ReportBrowserNavigationFailure(string message)
    {
        if (_viewModel.CurrentPage is VkVideoBrowserPageViewModel page)
        {
            page.ReportNavigationFailure(message);
        }
    }

    private void OnBrowserRequested(object? sender, Uri uri)
    {
        if (!VkEmbedUriValidator.IsAllowedVideoPage(uri))
        {
            _viewModel.Player.ReportNavigationFailure("Ссылка браузера отклонена политикой безопасности.");
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
        }
        catch (Exception)
        {
            _viewModel.Player.ReportNavigationFailure("Не удалось открыть системный браузер.");
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _viewModel.Player.PropertyChanged -= OnPlayerPropertyChanged;
        _viewModel.Player.BrowserRequested -= OnBrowserRequested;
        _viewModel.BrowserRequested -= OnVkVideoBrowserRequested;
    }
}
