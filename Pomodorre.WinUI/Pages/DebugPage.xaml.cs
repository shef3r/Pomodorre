using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Pomodorre.Statistics;
using Pomodorre.Tools;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Pomodorre.WinUI.Pages
{
    public sealed partial class DebugPage : Page
    {
        public static DebugPage? Current { get; private set; }

        public DebugPage()
        {
            InitializeComponent();
            this.Loaded += (s, e) => Current = this;
            this.Unloaded += (s, e) => Current = null;
            this.Loaded += async (s, e) => {
                jsonDebug_history.Text = string.Join("\n", JsonSerializer.Serialize((await SessionLogger.GetSessionsAsync(DateTime.Now, DateTime.Now)).Values.FirstOrDefault(), new JsonSerializerOptions() { WriteIndented = true }));
            };
        }

        public void UpdateDebugText(string info)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (dbox != null)
                {
                    dbox.Text = info;
                }
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Stars.Amount += 10;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Stars.Amount -= 10;
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;

            bool alreadyHasStreak = Streaks.History.Any(x => x.Key.Date == DateTime.Today);

            if (alreadyHasStreak)
            {
                btn.Content = "Streak done for today";
                await Task.Delay(1000);
                btn.Content = "Add streak for today";
            }
            else
            {
                Streaks.AddOrUpdate(DateTime.Today, true);
            }
        }
    }
}