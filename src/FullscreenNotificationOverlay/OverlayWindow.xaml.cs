using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace FullscreenNotificationOverlay;

public partial class OverlayWindow : Window
{
    private readonly DispatcherTimer _hideTimer = new() { Interval = TimeSpan.FromSeconds(5) };

    public OverlayWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => MakeNoActivateToolWindow();
        _hideTimer.Tick += (_, _) => HideBanner();
    }

    protected override bool ShowActivated => false;

    public void ShowBanner(NotificationItem item)
    {
        AppText.Text = item.AppName;
        TitleText.Text = item.Title;
        BodyText.Text = item.Body;

        var area = SystemParameters.WorkArea;
        Left = area.Right - Width - 22;
        Top = area.Bottom - Height - 22;

        if (!IsVisible) Show();
        ActivateTopmostWithoutFocus();

        _hideTimer.Stop();
        Root.BeginAnimation(OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(160)));
        _hideTimer.Start();
    }

    private void HideBanner()
    {
        _hideTimer.Stop();
        var animation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(180));
        animation.Completed += (_, _) => Hide();
        Root.BeginAnimation(OpacityProperty, animation);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        HideBanner();
    }

    private void MakeNoActivateToolWindow()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        var style = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, style | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_TOPMOST);
    }

    private void ActivateTopmostWithoutFocus()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        SetWindowPos(hwnd, HWND_TOPMOST, (int)Left, (int)Top, (int)Width, (int)Height, SWP_NOACTIVATE | SWP_SHOWWINDOW);
    }

    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_TOPMOST = 0x00000008;
    private const int WS_EX_NOACTIVATE = 0x08000000;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_SHOWWINDOW = 0x0040;
    private static readonly IntPtr HWND_TOPMOST = new(-1);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
}
