using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using WinRT.Interop;
using Pomodorre.WinUI.Pages;
using Pomodorre.Tools;

namespace Pomodorre.WinUI
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Activated += MainWindow_Activated;

            ApplicationView.PreferredLaunchViewSize = new Size(500, 700);

            ContentFrame.Navigate(typeof(HomePage));
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

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
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
            }
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
    }
}