using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using Windows.Storage;

namespace Pomodorre.Statistics
{
    public static class Streaks
    {
        private const string Key = "StreakHistory";

        private static readonly object _sync = new();
        private static readonly ApplicationDataContainer _local = ApplicationData.Current.LocalSettings;

        public static event PropertyChangedEventHandler? PropertyChanged;

        private static void Notify(params string[] properties)
        {
            foreach (var prop in properties)
                PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(prop));
        }

        private static Dictionary<DateTime, bool> Get()
        {
            lock (_sync)
            {
                if (_local.Values.TryGetValue(Key, out object? raw))
                {
                    try
                    {
                        if (raw is string s)
                            return JsonSerializer.Deserialize<Dictionary<DateTime, bool>>(s)!;
                    }
                    catch { }
                }

                return new Dictionary<DateTime, bool>();
            }
        }

        private static void Set(Dictionary<DateTime, bool> value)
        {
            lock (_sync)
            {
                _local.Values[Key] = JsonSerializer.Serialize(value);
            }

            // 🔥 IMPORTANT: notify ALL dependent properties
            Notify(nameof(History), nameof(Current), nameof(Longest));
        }

        public static Dictionary<DateTime, bool> History
        {
            get => new(Get());
            set => Set(value);
        }

        public static void AddOrUpdate(DateTime day, bool done)
        {
            var history = Get();
            history[day.Date] = done;
            Set(history);
        }

        public static void Remove(DateTime day)
        {
            var history = Get();
            if (history.Remove(day.Date))
                Set(history);
        }

        public static int Current
        {
            get
            {
                var history = Get();

                if (history.Count == 0)
                    return 0;

                var latestDay = history.Keys.Max().Date;

                int streak = 0;
                var day = latestDay;

                while (history.TryGetValue(day, out bool done) && done)
                {
                    streak++;
                    day = day.AddDays(-1);
                }

                return streak;
            }
        }

        public static int Longest
        {
            get
            {
                var history = Get();

                var days = history
                    .Where(x => x.Value)
                    .Select(x => x.Key.Date)
                    .Distinct()
                    .OrderBy(d => d)
                    .ToList();

                int longest = 0;
                int current = 0;

                for (int i = 0; i < days.Count; i++)
                {
                    if (i == 0 || (days[i] - days[i - 1]).Days == 1)
                        current++;
                    else
                        current = 1;

                    if (current > longest)
                        longest = current;
                }

                return longest;
            }
        }
    }
}