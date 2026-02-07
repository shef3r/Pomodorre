using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using Windows.Storage;

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

        public static int FocusBlockMinutes
        {
            get => Get(nameof(FocusBlockMinutes), 20);
            set => Set(nameof(FocusBlockMinutes), value);
        }
        public static int StarAmount
        {
            get => Get(nameof(StarAmount), 0);
            set => Set(nameof(StarAmount), value);
        }
        public static Dictionary<DateTime, bool> StreakHistory
        {
            get => Get<Dictionary<DateTime, bool>>(nameof(StreakHistory), new Dictionary<DateTime, bool>());
            set => Set(nameof(StreakHistory), value);
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
                    catch
                    {
                    }
                }

                return defaultValue!;
            }
        }
        private static void Set<T>(string key, T value)
        {
            lock (_sync)
            {
                object storeValue;

                // Te można zapisywać bez bawienia się w serializację
                if (value is string
                    || value is bool
                    || value is byte
                    || value is sbyte
                    || value is short
                    || value is ushort
                    || value is int
                    || value is uint
                    || value is long
                    || value is ulong
                    || value is float
                    || value is double
                    || value is decimal
                    || value is DateTime
                    || value is Guid
                    || value is TimeSpan)
                {
                    storeValue = value!;
                }
                else
                {
                    storeValue = JsonSerializer.Serialize(value);
                }

                _local.Values[key] = storeValue;
            }

            RaisePropertyChanged(key);
        }

        private static void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
    }
}