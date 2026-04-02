using System;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppNotifications;
using Windows.ApplicationModel.Activation;
using Windows.UI;
using Pomodorre.Tools;
using WinRT.Interop;

namespace Pomodorre.WinUI
{
    public partial class App : Application
    {
        private Window? _window;

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            try
            {
                AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;

                try
                {
                    AppNotificationManager.Default.Register();
                    Console.WriteLine("[Notifications] Registered successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Notifications] Registration failed: {ex.Message}");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[App] Failed to initialize notifications: {ex}");
            }

            _window = new MainWindow();
            _window.Activate();

            Settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Settings.ThemeMode))
                {
                    ApplyTheme();
                }
            };

            ApplyTheme();
        }

        private void ApplyTheme()
        {
            var themeMode = Settings.ThemeMode;
            var requestedTheme = themeMode switch
            {
                "Light" => ElementTheme.Light,
                "Dark" => ElementTheme.Dark,
                _ => ElementTheme.Default
            };

            if (_window?.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = requestedTheme;
            }

            ApplyTitleBarTheme(requestedTheme);
        }

        private void ApplyTitleBarTheme(ElementTheme theme)
        {
            try
            {
                if (_window == null) return;

                var hwnd = WindowNative.GetWindowHandle(_window);
                var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
                var appWindow = AppWindow.GetFromWindowId(windowId);

                if (appWindow?.TitleBar == null) return;

                var isDark = theme == ElementTheme.Dark || 
                    (theme == ElementTheme.Default && Application.Current.RequestedTheme == ApplicationTheme.Dark);

                if (isDark)
                {
                    appWindow.TitleBar.ButtonForegroundColor = Colors.White;
                    appWindow.TitleBar.ButtonHoverBackgroundColor = Color.FromArgb(20, 255, 255, 255);
                    appWindow.TitleBar.ButtonPressedBackgroundColor = Color.FromArgb(40, 255, 255, 255);
                }
                else
                {
                    appWindow.TitleBar.ButtonForegroundColor = Colors.Black;
                    appWindow.TitleBar.ButtonHoverBackgroundColor = Color.FromArgb(20, 0, 0, 0);
                    appWindow.TitleBar.ButtonPressedBackgroundColor = Color.FromArgb(40, 0, 0, 0);
                }
            }
            catch { }
        }

        private void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
        {
            Console.WriteLine("[Notification] Invoked");
            Console.WriteLine($"Arguments: {args.Argument}");
        }
    }
}
