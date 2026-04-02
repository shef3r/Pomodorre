using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using Pomodorre.Tools;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Pomodorre.WinUI.Pages
{
    public sealed partial class HomePage : Page
    {
        public HomeItems Settings => Pomodorre.Tools.Settings.HomeItems;

        public ObservableCollection<FrameworkElement> HomeItemsCollection { get; } = new();

        public HomePage()
        {
            InitializeComponent();

            Pomodorre.Tools.Settings.PropertyChanged += Settings_PropertyChanged;
            UpdateHomeItemsCollection();
        }

        private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(HomeItems.ShowSessionStats):
                    UpdateControlVisibility<Pomodorre.Controls.HomeTiles.StatsControl>(Settings.ShowSessionStats);
                    break;
            }
        }

        private void UpdateControlVisibility<T>(bool shouldShow) where T : FrameworkElement, new()
        {
            var existingControl = HomeItemsCollection.FirstOrDefault(c => c is T);

            if (shouldShow && existingControl == null)
            {
                HomeItemsCollection.Add(new T());
            }
            else if (!shouldShow && existingControl != null)
            {
                HomeItemsCollection.Remove(existingControl);
            }
        }

        private void UpdateHomeItemsCollection()
        {
            HomeItemsCollection.Clear();

            if (Settings.ShowSessionStats)
            {
                HomeItemsCollection.Add(new Pomodorre.Controls.HomeTiles.StatsControl());
            }
        }
    }
}
