using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;

namespace FullscreenNotificationOverlay;

public sealed class NotificationWatcher
{
    private readonly Action<NotificationItem> _onNotification;
    private UserNotificationListener? _listener;
    private readonly HashSet<uint> _seen = new();

    public NotificationWatcher(Action<NotificationItem> onNotification)
    {
        _onNotification = onNotification;
    }

    public async Task<string> StartAsync()
    {
        _listener = UserNotificationListener.Current;
        var access = _listener.GetAccessStatus();

        if (access != UserNotificationListenerAccessStatus.Allowed)
        {
            return "Notification access is not enabled yet. Click “Enable notification access”, then allow this app in Windows Settings if prompted.";
        }

        await PrimeSeenNotificationsAsync();
        _listener.NotificationChanged -= OnNotificationChanged;
        _listener.NotificationChanged += OnNotificationChanged;
        return "Running. I’ll show overlay banners for new notifications while you’re in fullscreen.";
    }

    public async Task<string> RequestPermissionAndStartAsync()
    {
        _listener = UserNotificationListener.Current;
        var access = await _listener.RequestAccessAsync();

        if (access != UserNotificationListenerAccessStatus.Allowed)
        {
            return $"Notification access was not granted ({access}). Open Windows Settings → Privacy & security → Notifications and allow this app.";
        }

        return await StartAsync();
    }

    private async Task PrimeSeenNotificationsAsync()
    {
        if (_listener is null) return;
        var notifications = await _listener.GetNotificationsAsync(NotificationKinds.Toast);
        foreach (var notification in notifications)
        {
            _seen.Add(notification.Id);
        }
    }

    private async void OnNotificationChanged(UserNotificationListener sender, UserNotificationChangedEventArgs args)
    {
        if (args.ChangeKind != UserNotificationChangedKind.Added) return;

        try
        {
            var notifications = await sender.GetNotificationsAsync(NotificationKinds.Toast);
            foreach (var notification in notifications.OrderByDescending(n => n.CreationTime))
            {
                if (!_seen.Add(notification.Id)) continue;

                var item = Convert(notification);
                if (!IsDiscordNotification(item)) continue;

                if (!string.IsNullOrWhiteSpace(item.Title) || !string.IsNullOrWhiteSpace(item.Body))
                {
                    _onNotification(item);
                }
            }
        }
        catch
        {
            // Notification APIs can throw for expired/inaccessible notifications. Ignore and keep listening.
        }
    }

    private static NotificationItem Convert(UserNotification notification)
    {
        var appName = notification.AppInfo.DisplayInfo.DisplayName;
        var binding = notification.Notification.Visual.GetBinding(KnownNotificationBindings.ToastGeneric);
        var text = binding?.GetTextElements().Select(t => t.Text).Where(t => !string.IsNullOrWhiteSpace(t)).ToArray() ?? Array.Empty<string>();

        var title = text.ElementAtOrDefault(0) ?? appName;
        var body = string.Join("\n", text.Skip(1));
        return new NotificationItem(title, body, appName);
    }

    private static bool IsDiscordNotification(NotificationItem item)
    {
        return item.AppName.Contains("Discord", StringComparison.OrdinalIgnoreCase);
    }
}
