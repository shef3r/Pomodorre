using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
using System.Globalization;

namespace Pomodorre.WinUI.Pages
{
    public sealed partial class FocusPage : Page, INotifyPropertyChanged
    {
        private string _remainingTime = "00:00";
        private float _progress = 0;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string RemainingTime
        {
            get => _remainingTime;
            set
            {
                if (_remainingTime != value)
                {
                    _remainingTime = value;
                    OnPropertyChanged(nameof(RemainingTime));
                }
            }
        }

        public float Progress
        {
            get => _progress;
            set
            {
                if (Math.Abs(_progress - value) > 0.001f)
                {
                    _progress = value;
                    OnPropertyChanged(nameof(Progress));
                }
            }
        }

        public FocusPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void UpdateStatus(string status)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (status.StartsWith("TICK"))
                {
                    HandleTick(status);
                }
                else if (status.StartsWith("STATUS_RES"))
                {
                    HandleStatus(status);
                }
            });
        }

        private void HandleTick(string status)
        {
            string[] parts = status.Split('|');

            RemainingTime = parts[1];

            if (float.TryParse(parts[2],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float p))
            {
                Progress = p;
            }
        }

        private void HandleStatus(string status)
        {
            string[] parts = status.Split('|');

            RemainingTime = parts[3];

            if (float.TryParse(parts[4],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float p))
            {
                Progress = p;
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}