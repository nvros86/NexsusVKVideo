using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Input;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using NexsusVKVideo.Infrastructure.Storage;
using NexsusVKVideo.Infrastructure.Vk;

namespace NexsusVKVideo.App;

public partial class MainWindow : Window
{
    private const int DwmUseImmersiveDarkMode = 20;
    private const int DwmUseImmersiveDarkModeBeforeWindows10_2004 = 19;
    private const double DefaultZoomFactor = 1.0;
    private const double MinimumZoomFactor = 0.5;
    private const double MaximumZoomFactor = 3.0;
    private const double ZoomStepMultiplier = 1.1;
    private static readonly Uri VkVideoHomeUri = new("https://vkvideo.ru/", UriKind.Absolute);
    private static readonly Uri WebView2RuntimeDownloadUri = new("https://developer.microsoft.com/microsoft-edge/webview2/", UriKind.Absolute);
    private WindowState _windowStateBeforeFullScreen;
    private WindowStyle _windowStyleBeforeFullScreen;
    private ResizeMode _resizeModeBeforeFullScreen;
    private bool _isWebViewFullScreen;
    private bool _isBrowserInitializing;

    public MainWindow()
    {
        InitializeComponent();
        FitInitialWindowToWorkArea();
        SourceInitialized += OnSourceInitialized;
        Loaded += OnLoaded;
        Closed += OnClosed;
        PreviewKeyDown += OnPreviewKeyDown;
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
        await InitializeBrowserAsync();
    }

    private async Task InitializeBrowserAsync()
    {
        if (_isBrowserInitializing)
        {
            return;
        }

        _isBrowserInitializing = true;
        try
        {
            HideError();

            if (BrowserWebView.CoreWebView2 is not null)
            {
                BrowserWebView.CoreWebView2.Navigate(VkVideoHomeUri.AbsoluteUri);
                return;
            }

            var userDataFolder = ApplicationDataPaths.GetWebViewUserDataPath();
            Directory.CreateDirectory(userDataFolder);

            var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
            await BrowserWebView.EnsureCoreWebView2Async(environment);
            var coreWebView2 = BrowserWebView.CoreWebView2;
            if (coreWebView2 is null)
            {
                ShowError("Не удалось подготовить встроенный браузер. Повторите попытку.");
                return;
            }

            coreWebView2.NavigationStarting += OnNavigationStarting;
            coreWebView2.NavigationCompleted += OnNavigationCompleted;
            coreWebView2.NewWindowRequested += OnNewWindowRequested;
            coreWebView2.ContainsFullScreenElementChanged += OnContainsFullScreenElementChanged;
            coreWebView2.Navigate(VkVideoHomeUri.AbsoluteUri);
        }
        catch (WebView2RuntimeNotFoundException)
        {
            ShowError("Среда выполнения Microsoft Edge WebView2 не найдена. Установите Evergreen Runtime, затем нажмите «Повторить».");
        }
        catch (Exception)
        {
            ShowError("Не удалось открыть VK Video. Проверьте подключение к интернету и повторите попытку.");
        }
        finally
        {
            _isBrowserInitializing = false;
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
            return;
        }

        HideError();
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

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        var coreWebView2 = BrowserWebView.CoreWebView2;
        if (coreWebView2 is null)
        {
            return;
        }

        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        var modifiers = Keyboard.Modifiers;

        if (modifiers == ModifierKeys.Alt && key == Key.Left && coreWebView2.CanGoBack)
        {
            coreWebView2.GoBack();
            e.Handled = true;
            return;
        }

        if (modifiers == ModifierKeys.Alt && key == Key.Right && coreWebView2.CanGoForward)
        {
            coreWebView2.GoForward();
            e.Handled = true;
            return;
        }

        if ((modifiers == ModifierKeys.None && key == Key.F5) || (modifiers == ModifierKeys.Control && key == Key.R))
        {
            coreWebView2.Reload();
            e.Handled = true;
            return;
        }

        if (modifiers != ModifierKeys.Control)
        {
            return;
        }

        switch (key)
        {
            case Key.Add:
            case Key.OemPlus:
                SetZoomFactor(BrowserWebView.ZoomFactor * ZoomStepMultiplier);
                e.Handled = true;
                break;
            case Key.Subtract:
            case Key.OemMinus:
                SetZoomFactor(BrowserWebView.ZoomFactor / ZoomStepMultiplier);
                e.Handled = true;
                break;
            case Key.D0:
            case Key.NumPad0:
                SetZoomFactor(DefaultZoomFactor);
                e.Handled = true;
                break;
        }
    }

    private void SetZoomFactor(double zoomFactor) =>
        BrowserWebView.ZoomFactor = Math.Clamp(zoomFactor, MinimumZoomFactor, MaximumZoomFactor);

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

    private void HideError() => BrowserErrorPanel.Visibility = Visibility.Collapsed;

    private async void OnRetryClicked(object sender, RoutedEventArgs e)
    {
        await InitializeBrowserAsync();
    }

    private void OnInstallWebView2Clicked(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo(WebView2RuntimeDownloadUri.AbsoluteUri) { UseShellExecute = true });
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
