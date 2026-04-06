using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Pomodorre.Statistics;

namespace Pomodorre.WinUI.Pages
{
    public sealed partial class HistoryPage : Page
    {
        public HistoryHandler handler { get; private set; }

        public HistoryPage()
        {
            InitializeComponent();
            handler = new HistoryHandler();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            // Clear handler when navigating away
            handler?.Clear();
        }

        public string FormatWeekRange(DateTime start, DateTime end)
        {
            return $"{start:ddd, MMM d} - {end:ddd, MMM d}";
        }

        private void PreviousWeek_Click(object sender, RoutedEventArgs e)
        {
            handler.PreviousWeek();
        }

        private void NextWeek_Click(object sender, RoutedEventArgs e)
        {
            handler.NextWeek();
        }
    }
}
