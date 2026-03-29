using System.ComponentModel;

#nullable enable
namespace Pomodorre.Tools
{
    public sealed class SettingsBinding : INotifyPropertyChanged
    {
        public static SettingsBinding Instance { get; } = new SettingsBinding();

        private SettingsBinding()
        {
            Settings.PropertyChanged += (s, e) =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.PropertyName));
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        
        public int FocusBlocks
        {
            get => Settings.FocusBlocks;
            set => Settings.FocusBlocks = value;
        }

        public int RestBlockMinutes
        {
            get => Settings.RestBlockMinutes;
            set => Settings.RestBlockMinutes = value;
        }

        public int FocusBlockMinutes
        {
            get => Settings.FocusBlockMinutes;
            set => Settings.FocusBlockMinutes = value;
        }
    }
}
