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
        Loaded += (_, _) =>
        {
            StatusText.Text = "Ready. Click Enable notification access to start Discord fullscreen overlay.";
        };
    }

    private async Task StartAsync()
    {
        StatusText.Text = "Starting listener…";
        var status = await _watcher.StartAsync();
        StatusText.Text = status;
    }

    private async void PermissionButton_Click(object sender, RoutedEventArgs e)
    {
        PermissionButton.IsEnabled = false;
        StatusText.Text = "Requesting notification access…";
        try
        {
            var status = await _watcher.RequestPermissionAndStartAsync();
            StatusText.Text = status;
        }
        finally
        {
            PermissionButton.IsEnabled = true;
        }
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
