using System;
using System.ComponentModel;
using System.Text.Json;
using Windows.Storage;

#nullable enable
namespace Pomodorre.Tools
{
    public static class Settings
    {
        internal static readonly object _sync = new();
        internal static readonly ApplicationDataContainer _local = ApplicationData.Current.LocalSettings;

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

        public static string ThemeMode
        {
            get => Get(nameof(ThemeMode), "System");
            set => Set(nameof(ThemeMode), value);
        }

        public static bool KillBackgroundProcessOnExit
        {
            get => Get(nameof(KillBackgroundProcessOnExit), true);
            set => Set(nameof(KillBackgroundProcessOnExit), value);
        }

        public static bool ExposeAppService
        {
            get => Get(nameof(ExposeAppService), false);
            set => Set(nameof(ExposeAppService), value);
        }

        public static HomeItems HomeItems => HomeItems.Instance;

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

        internal static void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class HomeItems
    {
        public static readonly HomeItems Instance = new();

        public bool ShowWelcomeMessage
        {
            get => Get(nameof(ShowWelcomeMessage), true);
            set => Set(nameof(ShowWelcomeMessage), value);
        }

        public bool ShowSessionStats
        {
            get => Get(nameof(ShowSessionStats), true);
            set => Set(nameof(ShowSessionStats), value);
        }

        public bool ShowHistoryStats
        {
            get => Get(nameof(ShowHistoryStats), true);
            set => Set(nameof(ShowHistoryStats), value);
        }

        private static T Get<T>(string key, T? defaultValue = default)
        {
            lock (Settings._sync)
            {
                var fullKey = $"HomeItems_{key}";
                if (Settings._local.Values.ContainsKey(fullKey))
                {
                    try
                    {
                        var raw = Settings._local.Values[fullKey];

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
            lock (Settings._sync)
            {
                var fullKey = $"HomeItems_{key}";
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

                Settings._local.Values[fullKey] = storeValue;
            }

            Settings.RaisePropertyChanged(key);
        }
    }
}
