using System.Windows;

namespace FullscreenNotificationOverlay;

public partial class MainWindow : Window
{
    private readonly OverlayWindow _overlay = new();
    private readonly NotificationWatcher _watcher;

    public MainWindow()
    {
        InitializeComponent();
        _watcher = new NotificationWatcher(OnNotification);
        Loaded += async (_, _) => await StartAsync();
    }

    private async Task StartAsync()
    {
        var status = await _watcher.StartAsync();
        StatusText.Text = status;
    }

    private async void PermissionButton_Click(object sender, RoutedEventArgs e)
    {
        var status = await _watcher.RequestPermissionAndStartAsync();
        StatusText.Text = status;
    }

    private void TestButton_Click(object sender, RoutedEventArgs e)
    {
        _overlay.ShowBanner(new NotificationItem("Test notification", "This is how the fullscreen banner will look.", "Cozy Overlay"));
    }

    private void OnNotification(NotificationItem item)
    {
        Dispatcher.Invoke(() =>
        {
            if (FullscreenDetector.IsForegroundWindowFullscreen())
            {
                _overlay.ShowBanner(item);
            }
        });
    }
}
