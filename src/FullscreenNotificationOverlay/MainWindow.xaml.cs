using System.Windows;

namespace FullscreenNotificationOverlay;

public partial class MainWindow : Window
{
    private readonly OverlayWindow _overlay = new();
    private readonly NotificationWatcher _watcher;
    private bool _settingsOpened;

    public MainWindow()
    {
        InitializeComponent();
        _watcher = new NotificationWatcher(OnNotification);
        Loaded += (_, _) =>
        {
            StatusText.Text = "Ready. Click Open notification settings first, enable notification access if Windows shows this app, then click the button again to start.";
            PermissionButton.Content = "Open notification settings";
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
        try
        {
            if (!_settingsOpened)
            {
                _settingsOpened = true;
                PermissionButton.Content = "Start listener";
                StatusText.Text = "I opened Windows notification settings. Enable notification access there if available, then come back and click Start listener.";
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "ms-settings:notifications",
                    UseShellExecute = true
                });
                return;
            }

            StatusText.Text = "Starting listener…";
            var status = await _watcher.StartAsync();
            StatusText.Text = status;
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Could not start: {ex.Message}";
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
