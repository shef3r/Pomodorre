using System.ComponentModel;
using Pomodorre.Tools;

namespace Pomodorre.WinUI.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public SettingsViewModel()
        {
            Settings.PropertyChanged += OnSettingsPropertyChanged;
        }

        private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        public string ThemeMode
        {
            get => Settings.ThemeMode;
            set
            {
                if (Settings.ThemeMode != value)
                {
                    Settings.ThemeMode = value;
                }
            }
        }

        public bool KillBackgroundProcessOnExit
        {
            get => Settings.KillBackgroundProcessOnExit;
            set
            {
                if (Settings.KillBackgroundProcessOnExit != value)
                {
                    Settings.KillBackgroundProcessOnExit = value;
                }
            }
        }

        public bool ExposeAppService
        {
            get => Settings.ExposeAppService;
            set
            {
                if (Settings.ExposeAppService != value)
                {
                    Settings.ExposeAppService = value;
                }
            }
        }

        public HomeItems HomeItems => Settings.HomeItems;
    }
}
