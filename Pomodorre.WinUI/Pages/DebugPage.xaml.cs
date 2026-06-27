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
    public class UIPageTraceListener : System.Diagnostics.TraceListener
    {
        public override void Write(string? message)
        {
            if (message == null) return;
            if (DebugPage.Current is { } page)
            {
                page.UpdateDebugText(message);
            }
            DebugPage.CachedLogs += message;
        }

        public override void WriteLine(string? message)
        {
            Write(message + Environment.NewLine);
        }
    }

    public sealed partial class DebugPage : Page
    {
        public static DebugPage? Current { get; private set; }
        public static string CachedLogs = "";
        private static bool _listenerAdded = false;

        public DebugPage()
        {
            InitializeComponent();
            
            if (!_listenerAdded)
            {
                System.Diagnostics.Trace.Listeners.Add(new UIPageTraceListener());
                _listenerAdded = true;
            }

            this.Loaded += async (s, e) => {
                Current = this;
                dbox.Text = CachedLogs;
                var sessions = await HistoryTools.GetSessionsAsync(DateTime.Now, DateTime.Now);
                jsonDebug_history.Text = JsonSerializer.Serialize(sessions.Values.FirstOrDefault(), new JsonSerializerOptions { WriteIndented = true });
            };
            this.Unloaded += (s, e) => Current = null;
        }

        public void UpdateDebugText(string info)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (dbox != null)
                {
                    dbox.Text += info;
                    // Scroll to bottom
                    dbox.SelectionStart = dbox.Text.Length;
                }
            });
        }

        private void CopyCrash_Click(object sender, RoutedEventArgs e)
        {
            var crash = Settings.LastCrash;
            if (string.IsNullOrEmpty(crash))
                crash = "No crash found.";
                
            var dp = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dp.SetText(crash);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dp);
        }

        private void CopyLogs_Click(object sender, RoutedEventArgs e)
        {
            var dp = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dp.SetText(dbox.Text);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dp);
        }

        private void ClearLogs_Click(object sender, RoutedEventArgs e)
        {
            CachedLogs = "";
            dbox.Text = "";
        }
    }
}