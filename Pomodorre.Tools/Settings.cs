using System;
using System.ComponentModel;
using System.Text.Json;
using Windows.Storage;

#nullable enable
namespace Pomodorre.Tools
{
    public static class Settings
    {
        private static readonly object _sync = new();
        private static readonly ApplicationDataContainer _local = ApplicationData.Current.LocalSettings;

        public static event PropertyChangedEventHandler? PropertyChanged;

        public static int FocusBlocks
        {
            get => Get(nameof(FocusBlocks), 5);
            set => Set(nameof(FocusBlocks), value);
        }

        public static int RestBlockMinutes
        {
            get => Get(nameof(RestBlockMinutes), 5);
            set => Set(nameof(RestBlockMinutes), value);
        }

        public static bool IsTimePickerCollapsed
        {
            get => Get(nameof(IsTimePickerCollapsed), false);
            set => Set(nameof(IsTimePickerCollapsed), value);
        }

        public static int FocusBlockMinutes
        {
            get => Get(nameof(FocusBlockMinutes), 20);
            set => Set(nameof(FocusBlockMinutes), value);
        }

        public static string EndSessionTimeString
        {
            get
            {
                try
                {
                    var totalMinutes =
                        (FocusBlockMinutes * FocusBlocks) +
                        (RestBlockMinutes * (FocusBlocks - 1));

                    return DateTime.Now.AddMinutes(totalMinutes).ToString("HH:mm");
                }
                catch
                {
                    return "--:--";
                }
            }
        }

        private static T Get<T>(string key, T? defaultValue = default)
        {
            lock (_sync)
            {
                if (_local.Values.ContainsKey(key))
                {
                    try
                    {
                        var raw = _local.Values[key];

                        if (raw is T t)
                            return t;

                        if (raw is string s)
                        {
                            if (typeof(T) == typeof(string))
                                return (T)(object)s;

                            return JsonSerializer.Deserialize<T>(s)!;
                        }

                        return (T)Convert.ChangeType(raw, typeof(T));
                    }
                    catch { }
                }

                return defaultValue!;
            }
        }

        private static void Set<T>(string key, T value)
        {
            lock (_sync)
            {
                object storeValue;

                if (value is string
                    || value is bool
                    || value is int
                    || value is long
                    || value is float
                    || value is double
                    || value is DateTime)
                {
                    storeValue = value!;
                }
                else
                {
                    storeValue = JsonSerializer.Serialize(value);
                }

                _local.Values[key] = storeValue;
            }

            PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(key));
            PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(EndSessionTimeString)));
        }
    }
}