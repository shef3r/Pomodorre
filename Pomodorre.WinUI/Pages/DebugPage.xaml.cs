using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Pomodorre.TimerCore.Services;
using Pomodorre.Tools;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Pomodorre.WinUI.Pages
{
    public sealed partial class DebugPage : Page
    {
        public DebugPage()
        {
            InitializeComponent();
            PomodoroService.Instance.DebugInfoUpdated += (s, info) =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    dbox.Text = info;
                });
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Settings.StarAmount += 10;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Settings.StarAmount -= 10;
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (Settings.StreakHistory.Any<KeyValuePair<DateTime, bool>>(x => x.Key.Date == DateTime.Today))
            {
                if (btn != null)
                {
                    btn.Content = "Streak done for today";
                    await Task.Delay(1000);
                    btn.Content = "Add streak for today";
                }
            }
            else
            {
                Settings.AddOrUpdateStreak(DateTime.Today, true);
            }
        }
    }
}
