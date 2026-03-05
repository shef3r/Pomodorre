using System;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppNotifications;
using Windows.ApplicationModel.Activation;

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

            // Initialize main window
            _window = new MainWindow();
            _window.Activate();
        }

        private void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
        {
            Console.WriteLine("[Notification] Invoked");
            Console.WriteLine($"Arguments: {args.Argument}");
        }
    }
}