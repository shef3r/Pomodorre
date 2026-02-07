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
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using WinRT.Interop;

namespace Pomodorre.WinUI
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Activated += MainWindow_Activated;

            ApplicationView.PreferredLaunchViewSize = new Size(500, 700);
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
    }
}