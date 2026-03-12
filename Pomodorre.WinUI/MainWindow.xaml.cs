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
using Pomodorre.TimerCore;
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

        private bool _isSessionActive = false;
        private bool _isPaused = false;

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

            ApplicationView.PreferredLaunchViewSize = new Size(500, 700);

            ContentFrame.Navigate(typeof(DebugPage));
            try
            {
                SidebarListView.SelectedIndex = 0;
            }
            catch { }

            AnimateTimePicker();
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
                var existing = Process.GetProcessesByName("Pomodorre.BackgroundWorker");
                if (existing.Length == 0)
                {
                    string serverExe = Path.Combine(AppContext.BaseDirectory, "Pomodorre.BackgroundWorker.exe");
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = serverExe,
                        Arguments = Process.GetCurrentProcess().Id.ToString(),
                        WorkingDirectory = AppContext.BaseDirectory,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                    await Task.Delay(500);
                }

                _pipeClient = new NamedPipeClientStream(".", "PomodorrePipe", PipeDirection.InOut, PipeOptions.Asynchronous);
                await _pipeClient.ConnectAsync(5000);
                _pipeWriter = new StreamWriter(_pipeClient) { AutoFlush = true };

                _ = ListenToServer();

                await _pipeWriter.WriteLineAsync(PipeProtocol.CMD_STATUS);
            }
            catch (Exception ex) { Debug.WriteLine($"Pipe Error: {ex.Message}"); }
        }

        private async Task ListenToServer()
        {
            using var reader = new StreamReader(_pipeClient);
            while (_pipeClient.IsConnected)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(line)) continue;

                var parts = line.Split('|');
                DispatcherQueue.TryEnqueue(() =>
                {
                    switch (parts[0])
                    {
                        case PipeProtocol.EVENT_STATUS:
                            // STATUS_RES|IsActive|IsPaused|Time|Progress
                            _isSessionActive = bool.Parse(parts[1]);
                            _isPaused = bool.Parse(parts[2]);
                            UpdateStartButtonUI(_isSessionActive, _isPaused);

                            if (_isSessionActive)
                            {
                                SessionTimePrefix.Text = "Block ends in";
                                SessionTimeText.Text = parts[3];
                            }
                            else
                            {
                                SessionTimePrefix.Text = "Session will end by";
                                SessionTimeText.Text = Settings.EndSessionTimeString;
                            }
                            break;

                        case PipeProtocol.EVENT_TICK:
                            _isSessionActive = true;
                            SessionTimePrefix.Text = "Block ends in";
                            SessionTimeText.Text = parts[1];
                            // Progress bar albo tego typu inne
                            break;

                        case PipeProtocol.EVENT_COMPLETED:
                            _isSessionActive = false;
                            SessionTimePrefix.Text = "Session will end by";
                            SessionTimeText.Text = Settings.EndSessionTimeString;
                            UpdateStartButtonUI(false, false);
                            break;
                    }
                });
            }
        }

        private void UpdateStartButtonUI(bool active, bool paused)
        {
            _isSessionActive = active;
            _isPaused = paused;

            if (!active)
            {
                StartStopSymbol.Symbol = Symbol.Play;
                StartStopText.Text = "Start";
            }
            else
            {
                StartStopSymbol.Symbol = paused ? Symbol.Play : Symbol.Pause;
                StartStopText.Text = paused ? "Resume" : "Pause";
            }
        }

        private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WM_CLOSE && !_allowClose)
            {
                if (!_isSessionActive)
                    return CallWindowProc(_prevWndProc, hWnd, msg, wParam, lParam);

                DispatcherQueue.TryEnqueue(async () =>
                {
                    var dlg = new ContentDialog
                    {
                        Title = "Session running",
                        Content = "A session is running in the background. Stop it and exit?",
                        PrimaryButtonText = "Exit & Stop",
                        CloseButtonText = "Keep Running & Close UI"
                    };

                    try
                    {
                        var root = this.Content as FrameworkElement;
                        if (root?.XamlRoot != null) dlg.XamlRoot = root.XamlRoot;

                        var result = await dlg.ShowAsync();
                        if (result == ContentDialogResult.Primary)
                        {
                            await _pipeWriter.WriteLineAsync(PipeProtocol.CMD_STOP);
                            _allowClose = true;
                            this.Close();
                        }
                        else
                        {
                            _allowClose = true;
                            this.Close();
                        }
                    }
                    catch { }
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

            if (minuteContent is null) return;

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
                var sideAnim = new DoubleAnimation
                {
                    From = SidebarListView.Opacity,
                    To = Settings.IsTimePickerCollapsed ? 1 : 0,
                    Duration = duration.Subtract(new Duration(TimeSpan.FromMilliseconds(100))),
                    EasingFunction = easing
                };
                Storyboard.SetTarget(sideAnim, SidebarListView);
                Storyboard.SetTargetProperty(sideAnim, "Opacity");
                sb.Children.Add(sideAnim);

                var sideLowerAnim = new DoubleAnimation
                {
                    From = SidebarListViewLower.Opacity,
                    To = Settings.IsTimePickerCollapsed ? 1 : 0,
                    Duration = duration.Subtract(new Duration(TimeSpan.FromMilliseconds(100))),
                    EasingFunction = easing
                };
                Storyboard.SetTarget(sideLowerAnim, SidebarListViewLower);
                Storyboard.SetTargetProperty(sideLowerAnim, "Opacity");
                sb.Children.Add(sideLowerAnim);
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

        private async void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (_pipeWriter == null) return;

            if (!_isSessionActive)
            {
                string cmd = $"{PipeProtocol.CMD_START}|{(int)FocusBlockBox.Value}|{(int)FocusBlockMinsBox.Value}|{(int)RestBlockMinsBox.Value}";
                await _pipeWriter.WriteLineAsync(cmd);

                UpdateStartButtonUI(true, false);
            }
            else
            {
                if (!_isPaused)
                {
                    await _pipeWriter.WriteLineAsync(PipeProtocol.CMD_PAUSE);
                }
                else
                {
                    await _pipeWriter.WriteLineAsync(PipeProtocol.CMD_RESUME);
                }
            }
        }
    }
}