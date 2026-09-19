using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using NexsusVKVideo.Infrastructure.Storage;
using NexsusVKVideo.Infrastructure.Vk;

namespace NexsusVKVideo.App;

public partial class MainWindow : Window
{
    private const int DwmUseImmersiveDarkMode = 20;
    private const int DwmUseImmersiveDarkModeBeforeWindows10_2004 = 19;
    private static readonly Uri VkVideoHomeUri = new("https://vkvideo.ru/", UriKind.Absolute);
    private WindowState _windowStateBeforeFullScreen;
    private WindowStyle _windowStyleBeforeFullScreen;
    private ResizeMode _resizeModeBeforeFullScreen;
    private bool _isWebViewFullScreen;

    public MainWindow()
    {
        InitializeComponent();
        FitInitialWindowToWorkArea();
        SourceInitialized += OnSourceInitialized;
        Loaded += OnLoaded;
        Closed += OnClosed;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        if (SystemParameters.HighContrast)
        {
            return;
        }

        var handle = new WindowInteropHelper(this).Handle;
        if (handle == IntPtr.Zero)
        {
            return;
        }

        var useDarkMode = IsWindowsAppsDarkTheme() ? 1 : 0;
        if (DwmSetWindowAttribute(handle, DwmUseImmersiveDarkMode, ref useDarkMode, sizeof(int)) != 0)
        {
            DwmSetWindowAttribute(handle, DwmUseImmersiveDarkModeBeforeWindows10_2004, ref useDarkMode, sizeof(int));
        }
    }

    private static bool IsWindowsAppsDarkTheme()
    {
        var value = Registry.GetValue(
            @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
            "AppsUseLightTheme",
            1);
        return value is int setting && setting == 0;
    }

    private void FitInitialWindowToWorkArea()
    {
        const double edgePadding = 16;
        var workArea = SystemParameters.WorkArea;
        var availableWidth = Math.Max(1, workArea.Width - (edgePadding * 2));
        var availableHeight = Math.Max(1, workArea.Height - (edgePadding * 2));
        var minimumWidth = Math.Min(MinWidth, availableWidth);
        var minimumHeight = Math.Min(MinHeight, availableHeight);

        MinWidth = minimumWidth;
        MinHeight = minimumHeight;
        Width = Math.Clamp(Width, minimumWidth, availableWidth);
        Height = Math.Clamp(Height, minimumHeight, availableHeight);
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
            BrowserWebView.CoreWebView2.ContainsFullScreenElementChanged += OnContainsFullScreenElementChanged;
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

    private void OnContainsFullScreenElementChanged(object? sender, object e)
    {
        if (BrowserWebView.CoreWebView2?.ContainsFullScreenElement == true)
        {
            EnterWebViewFullScreen();
            return;
        }

        ExitWebViewFullScreen();
    }

    private void EnterWebViewFullScreen()
    {
        if (_isWebViewFullScreen)
        {
            return;
        }

        _isWebViewFullScreen = true;
        _windowStateBeforeFullScreen = WindowState;
        _windowStyleBeforeFullScreen = WindowStyle;
        _resizeModeBeforeFullScreen = ResizeMode;
        WindowState = WindowState.Normal;
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        WindowState = WindowState.Maximized;
    }

    private void ExitWebViewFullScreen()
    {
        if (!_isWebViewFullScreen)
        {
            return;
        }

        _isWebViewFullScreen = false;
        WindowState = WindowState.Normal;
        WindowStyle = _windowStyleBeforeFullScreen;
        ResizeMode = _resizeModeBeforeFullScreen;
        WindowState = _windowStateBeforeFullScreen;
    }

    private void ShowError(string message)
    {
        BrowserErrorText.Text = message;
        BrowserErrorPanel.Visibility = Visibility.Visible;
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        if (BrowserWebView.CoreWebView2 is not null)
        {
            BrowserWebView.CoreWebView2.ContainsFullScreenElementChanged -= OnContainsFullScreenElementChanged;
        }
    }

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int attributeValue, int attributeSize);
}
