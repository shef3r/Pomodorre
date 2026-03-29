using Microsoft.UI.Dispatching;
using System.ComponentModel;

namespace Pomodorre.Statistics
{
    public sealed class StatisticsBinding : INotifyPropertyChanged
    {
        public static StatisticsBinding Instance { get; } = new StatisticsBinding();
        private readonly DispatcherQueue? _dispatcher = DispatcherQueue.GetForCurrentThread();

        private void RaiseOnUI(string propertyName)
        {
            if (_dispatcher != null && _dispatcher.HasThreadAccess)
            {
                OnPropertyChanged(propertyName);
            }
            else if (_dispatcher != null)
            {
                _dispatcher.TryEnqueue(() => OnPropertyChanged(propertyName));
            }
        }

        private StatisticsBinding()
        {
            Stars.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(Stars.Amount))
                    RaiseOnUI(nameof(StarAmount));
            };

            Streaks.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(Streaks.Current))
                    RaiseOnUI(nameof(CurrentStreak));

                if (e.PropertyName == nameof(Streaks.Longest))
                    RaiseOnUI(nameof(LongestStreak));
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public int StarAmount
        {
            get => Stars.Amount;
            set => Stars.Amount = value;
        }

        public Dictionary<DateTime, bool> StreakHistory => Streaks.History;

        public int CurrentStreak => Streaks.Current;

        public int LongestStreak => Streaks.Longest;
    }
}