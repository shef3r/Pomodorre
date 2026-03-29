using System.ComponentModel;

namespace Pomodorre.Statistics
{
    public sealed class StatisticsBinding : INotifyPropertyChanged
    {
        public static StatisticsBinding Instance { get; } = new StatisticsBinding();

        private StatisticsBinding()
        {
            Stars.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(Stars.Amount))
                    OnPropertyChanged(nameof(StarAmount));
            };

            Streaks.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(Streaks.Current))
                    OnPropertyChanged(nameof(CurrentStreak));

                if (e.PropertyName == nameof(Streaks.Longest))
                    OnPropertyChanged(nameof(LongestStreak));
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