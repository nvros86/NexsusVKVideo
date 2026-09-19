using System.IO;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using NexsusVKVideo.Infrastructure.Storage;
using NexsusVKVideo.Infrastructure.Vk;

namespace NexsusVKVideo.App;

public partial class MainWindow : Window
{
    private static readonly Uri VkVideoHomeUri = new("https://vkvideo.ru/", UriKind.Absolute);

    public MainWindow()
    {
        InitializeComponent();
        FitInitialWindowToWorkArea();
        Loaded += OnLoaded;
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
            await BrowserWebView.EnsureCoreWebView2Async(environment);
            BrowserWebView.CoreWebView2.NavigationStarting += OnNavigationStarting;
            BrowserWebView.CoreWebView2.NavigationCompleted += OnNavigationCompleted;
            BrowserWebView.CoreWebView2.NewWindowRequested += OnNewWindowRequested;
            BrowserWebView.CoreWebView2.Navigate(VkVideoHomeUri.AbsoluteUri);
        }
        catch (WebView2RuntimeNotFoundException)
        {
            ShowError("Среда выполнения Microsoft Edge WebView2 не найдена. Установите Evergreen Runtime и откройте приложение снова.");
        }
        catch (Exception)
        {
            ShowError("Не удалось открыть VK Video. Проверьте подключение к интернету и повторите попытку.");
        }
    }

    private void OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        if (Uri.TryCreate(e.Uri, UriKind.Absolute, out var target) && VkEmbedUriValidator.IsAllowedVkSite(target))
        {
            return;
        }

        e.Cancel = true;
        ShowError("Переход заблокирован: приложение открывает только защищённые сайты VK.");
    }

    private void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (!e.IsSuccess)
        {
            ShowError($"VK Video не загрузился ({e.WebErrorStatus}). Проверьте подключение к интернету.");
        }
    }

    private void OnNewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
    {
        e.Handled = true;
        if (Uri.TryCreate(e.Uri, UriKind.Absolute, out var target) && VkEmbedUriValidator.IsAllowedVkSite(target))
        {
            BrowserWebView.CoreWebView2?.Navigate(target.AbsoluteUri);
            return;
        }

        ShowError("Новое окно заблокировано: приложение открывает только защищённые сайты VK.");
    }

    private void ShowError(string message)
    {
        BrowserErrorText.Text = message;
        BrowserErrorPanel.Visibility = Visibility.Visible;
    }
}
