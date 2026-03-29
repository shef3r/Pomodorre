using System.ComponentModel;
using System.Text.Json;
using Windows.Storage;

namespace Pomodorre.Statistics
{
    public static class Stars
    {
        private const string Key = "StarAmount";

        private static readonly object _sync = new();
        private static readonly ApplicationDataContainer _local = ApplicationData.Current.LocalSettings;

        public static event PropertyChangedEventHandler? PropertyChanged;

        private static void Notify()
        {
            PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Amount)));
        }

        private static int Get()
        {
            lock (_sync)
            {
                if (_local.Values.TryGetValue(Key, out object? raw))
                {
                    try
                    {
                        if (raw is int i)
                            return i;

                        if (raw is string s)
                            return JsonSerializer.Deserialize<int>(s);
                    }
                    catch { }
                }

                return 0;
            }
        }

        private static void Set(int value)
        {
            lock (_sync)
            {
                _local.Values[Key] = value;
            }

            Notify();
        }

        public static int Amount
        {
            get => Get();
            set => Set(value < 0 ? 0 : value);
        }

        public static void Add(int value)
        {
            if (value <= 0)
                return;

            Set(Get() + value);
        }

        public static bool TrySpend(int value)
        {
            if (value <= 0)
                return true;

            var current = Get();

            if (current < value)
                return false;

            Set(current - value);
            return true;
        }
    }
}