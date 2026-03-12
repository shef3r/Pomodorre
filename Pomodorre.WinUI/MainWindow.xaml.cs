using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pomodorre.TimerCore.Services;
using Pomodorre.Tools;
using Pomodorre.WinUI.Pages;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using WinRT.Interop;

namespace Pomodorre.WinUI
{
    public sealed partial class MainWindow : Window
    {
        private NamedPipeClientStream _pipeClient;
        private StreamWriter _pipeWriter;

        private bool _allowClose = false;
        private IntPtr _hwnd = IntPtr.Zero;
        private IntPtr _prevWndProc = IntPtr.Zero;
        private WndProcDelegate? _wndProcDelegate;

        private const int GWL_WNDPROC = -4;
        private const uint WM_CLOSE = 0x0010;

        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        public MainWindow()
        {
            InitializeComponent();
            this.Activated += MainWindow_Activated;
            PomodoroService.Instance.Tick += OnTick;

            PomodoroService.Instance.SessionCompleted += async (s, completedSession) =>
            {
                Console.WriteLine("[UI] SessionCompleted event received");

                //server handles notifs

                DispatcherQueue.TryEnqueue(() =>
                {
                    StartStopSymbol.Symbol = Symbol.Play;
                    StartStopText.Text = "Start";

                    SessionTimePrefix.Text = "Session will end by";
                });
            };
            ApplicationView.PreferredLaunchViewSize = new Size(500, 700);

            ContentFrame.Navigate(typeof(DebugPage));
            try
            {
                SidebarListView.SelectedIndex = 0;
            }
            catch { }
            AnimateTimePicker();
        }

        private async void OnTick(object? sender, EventArgs e)
        {
            var session = PomodoroService.Instance.CurrentSession;
            if (session == null) return;

            DispatcherQueue.TryEnqueue(async () =>
            {
                Console.WriteLine($"[UI] Updating UI. Remaining={session.Remaining:mm\\:ss}");

                var progress = PomodoroService.Instance.GetProgress();

                //server handles notifs

                SessionTimePrefix.Text = "Block ends in";
                SessionTimeText.Text = session.Remaining.ToString(@"mm\:ss");
            });
        }

        private void ToggleOverlayForNumberBox(NumberBox source, bool visible)
        {
            if (source is null) return;
            if (source.Parent is Grid parentGrid)
            {
                foreach (var child in parentGrid.Children)
                {
                    if (child is TextBlock tb && tb.IsHitTestVisible == false)
                    {
                        tb.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
        }

        private void SidebarListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SidebarListView.SelectedItem is not ListViewItem lvi) return;
            if (lvi.Tag is not string tag) return;

            Type pageType = tag switch
            {
                "Debug" => typeof(DebugPage),
                "Home" => typeof(HomePage),
                "Goals" => typeof(GoalsPage),
                "History" => typeof(HistoryPage),
                "Stars" => typeof(StarsPage),
                "Stats" => typeof(StatsPage),
                "Settings" => typeof(SettingsPage),
                _ => null
            };

            if (pageType is not null && ContentFrame.Content?.GetType() != pageType)
            {
                ContentFrame.Navigate(pageType);
            }
        }

        private void FocusBlockBox_GotFocus(object sender, RoutedEventArgs e) => ToggleOverlayForNumberBox(FocusBlockBox, false);
        private void FocusBlockBox_LostFocus(object sender, RoutedEventArgs e) => ToggleOverlayForNumberBox(FocusBlockBox, true);

        private void RestBlockMinsBox_GotFocus(object sender, RoutedEventArgs e) => ToggleOverlayForNumberBox(RestBlockMinsBox, false);
        private void RestBlockMinsBox_LostFocus(object sender, RoutedEventArgs e) => ToggleOverlayForNumberBox(RestBlockMinsBox, true);

        private void FocusBlockMinsBox_GotFocus(object sender, RoutedEventArgs e) => ToggleOverlayForNumberBox(FocusBlockMinsBox, false);
        private void FocusBlockMinsBox_LostFocus(object sender, RoutedEventArgs e) => ToggleOverlayForNumberBox(FocusBlockMinsBox, true);

        private async void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            this.Activated -= MainWindow_Activated;
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(TitleBar);

            var hwnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);
            if (appWindow is not null)
            {
                appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
                appWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
                _hwnd = hwnd;
                _wndProcDelegate = new WndProcDelegate(WndProc);
                _prevWndProc = SetWindowLongPtr(_hwnd, GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(_wndProcDelegate));
            }

            await StartBackgroundServer();
        }

        private async Task StartBackgroundServer()
        {
            try
            {
                string serverExe = Path.Combine(AppContext.BaseDirectory, "Pomodorre.BackgroundWorker.exe");
                if (!File.Exists(serverExe)) return;

                var existing = Process.GetProcessesByName("Pomodorre.BackgroundWorker");
                foreach (var p in existing)
                {
                    try { p.Kill(); } catch { }
                }

                string myPid = Process.GetCurrentProcess().Id.ToString();

                Process.Start(new ProcessStartInfo
                {
                    FileName = serverExe,
                    Arguments = myPid,
                    WorkingDirectory = AppContext.BaseDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                await Task.Delay(2000); // daj mu sie wlaczyc

                _pipeClient = new NamedPipeClientStream(".", "PomodorrePipe", PipeDirection.InOut, PipeOptions.Asynchronous);
                await _pipeClient.ConnectAsync(10000);

                _pipeWriter = new StreamWriter(_pipeClient) { AutoFlush = true };
                await _pipeWriter.WriteLineAsync("CLIENT_CONNECTED");

                _ = ListenToServer();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Client Error]: {ex.Message}");
            }
        }

        private async Task ListenToServer()
        {
            using var reader = new StreamReader(_pipeClient);
            try
            {
                while (_pipeClient.IsConnected)
                {
                    var response = await reader.ReadLineAsync();
                    if (response == null) break;
                    Debug.WriteLine($"[Server Says]: {response}");
                }
            }
            catch (Exception ex) { Debug.WriteLine($"[Client Listen Error]: {ex.Message}"); }
        }

        private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WM_CLOSE && !_allowClose)
            {
                if (PomodoroService.Instance.CurrentSession == null)
                    return CallWindowProc(_prevWndProc, hWnd, msg, wParam, lParam);

                DispatcherQueue.TryEnqueue(async () =>
                {
                    var dlg = new ContentDialog
                    {
                        Title = "Session running",
                        Content = "A pomodoro session is currently running. Do you want to stop the session and exit?",
                        PrimaryButtonText = "Exit",
                        CloseButtonText = "Cancel"
                    };
                    
                    try
                    {
                        var root = this.Content as FrameworkElement;
                        if (root?.XamlRoot != null)
                            dlg.XamlRoot = root.XamlRoot;

                        var result = await dlg.ShowAsync();
                        if (result == ContentDialogResult.Primary)
                        {
                            PomodoroService.Instance.Stop();
                            _allowClose = true;
                            SetWindowLongPtr(_hwnd, GWL_WNDPROC, _prevWndProc);
                            DispatcherQueue.TryEnqueue(() => this.Close());
                        }
                    } catch { }
                    
                });
                return IntPtr.Zero;
            }

            return CallWindowProc(_prevWndProc, hWnd, msg, wParam, lParam);
        }

        private void MinuteToggle_Click(object sender, RoutedEventArgs e)
        {
            Settings.IsTimePickerCollapsed = !Settings.IsTimePickerCollapsed;
            AnimateTimePicker();
        }

        private void AnimateTimePicker()
        {
            var duration = new Duration(TimeSpan.FromMilliseconds(220));
            var easing = new CubicEase { EasingMode = EasingMode.EaseInOut };

            var root = this.Content as FrameworkElement;
            var minuteContent = root?.FindName("MinuteContent") as FrameworkElement;
            var chevron = root?.FindName("MinuteToggleChevronTransform") as RotateTransform;
            if (minuteContent is null)
                return;

            double desiredHeight;
            if (!Settings.IsTimePickerCollapsed)
            {
                minuteContent.Visibility = Visibility.Visible;
            }

            double fromHeight, toHeight;
            if (Settings.IsTimePickerCollapsed)
            {
                fromHeight = 230;
                toHeight = 0;
                minuteContent.Height = fromHeight;
            }
            else
            {
                fromHeight = 0;
                toHeight = 230;
                minuteContent.Visibility = Visibility.Visible;
            }

            var sb = new Storyboard();

            var heightAnim = new DoubleAnimation
            {
                From = fromHeight,
                To = toHeight,
                Duration = duration,
                EasingFunction = easing,
                EnableDependentAnimation = true
            };
            Storyboard.SetTarget(heightAnim, minuteContent);
            Storyboard.SetTargetProperty(heightAnim, "Height");
            sb.Children.Add(heightAnim);

            var opacityAnim = new DoubleAnimation
            {
                From = minuteContent.Opacity,
                To = Settings.IsTimePickerCollapsed ? 0 : 1,
                Duration = duration,
                EasingFunction = easing
            };
            Storyboard.SetTarget(opacityAnim, minuteContent);
            Storyboard.SetTargetProperty(opacityAnim, "Opacity");
            sb.Children.Add(opacityAnim);

            if (RootGrid.ActualHeight < 693 || (RootGrid.ActualHeight >= 693 && SidebarListView.Opacity == 0))
            {
                var opacityAnim2 = new DoubleAnimation
                {
                    From = SidebarListView.Opacity,
                    To = Settings.IsTimePickerCollapsed ? 1 : 0,
                    Duration = duration.Subtract(new Duration(TimeSpan.FromMilliseconds(100))),
                    EasingFunction = easing
                };
                Storyboard.SetTarget(opacityAnim2, SidebarListView);
                Storyboard.SetTargetProperty(opacityAnim2, "Opacity");
                sb.Children.Add(opacityAnim2);

                var opacityAnim3 = new DoubleAnimation
                {
                    From = SidebarListViewLower.Opacity,
                    To = Settings.IsTimePickerCollapsed ? 1 : 0,
                    Duration = duration.Subtract(new Duration(TimeSpan.FromMilliseconds(100))),
                    EasingFunction = easing
                };
                Storyboard.SetTarget(opacityAnim3, SidebarListViewLower);
                Storyboard.SetTargetProperty(opacityAnim3, "Opacity");
                sb.Children.Add(opacityAnim3);
            }

            var rotateAnim = new DoubleAnimation
            {
                From = chevron?.Angle ?? MinuteToggleChevronTransform.Angle,
                To = Settings.IsTimePickerCollapsed ? 180 : 0,
                Duration = duration,
                EasingFunction = easing
            };
            Storyboard.SetTarget(rotateAnim, chevron ?? MinuteToggleChevronTransform);
            Storyboard.SetTargetProperty(rotateAnim, "Angle");
            sb.Children.Add(rotateAnim);

            sb.Completed += (s, e) =>
            {
                if (Settings.IsTimePickerCollapsed)
                {
                    minuteContent.Visibility = Visibility.Collapsed;
                }
                else
                {
                    minuteContent.Height = double.NaN;
                }

                minuteContent.Opacity = 1;
            };

            sb.Begin();
        }

        private void SessionTime_Loaded(object sender, RoutedEventArgs e)
        {
            SessionTimeText.Text = Settings.EndSessionTimeString;
            Settings.PropertyChanged += (s, args) =>
            {
                SessionTimeText.Text = Settings.EndSessionTimeString;
            };
        }

        private void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            var service = PomodoroService.Instance;
            var session = service.CurrentSession;

            if (session == null)
            {
                Console.WriteLine("[UI] Start pressed");

                service.Start(
                    blocks: (int)FocusBlockBox.Value,
                    focusMinutes: (int)FocusBlockMinsBox.Value,
                    breakMinutes: (int)RestBlockMinsBox.Value);

                StartStopSymbol.Symbol = Symbol.Pause;
                StartStopText.Text = "Pause";
                return;
            }

            if (!session.IsPaused)
            {
                Console.WriteLine("[UI] Pause pressed");

                service.Pause();

                StartStopSymbol.Symbol = Symbol.Play;
                StartStopText.Text = "Resume";
            }
            else
            {
                Console.WriteLine("[UI] Resume pressed");

                service.Resume();

                StartStopSymbol.Symbol = Symbol.Pause;
                StartStopText.Text = "Pause";
            }
        }
    }
}