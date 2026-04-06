using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace Pomodorre.Statistics
{
    public class HistoryTools
    {
        public static async Task<StorageFile> GetDailyFileAsync(string date)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile file = await folder.CreateFileAsync($"{date}.json", CreationCollisionOption.OpenIfExists);

            string content = await FileIO.ReadTextAsync(file);
            if (string.IsNullOrWhiteSpace(content))
                await FileIO.WriteTextAsync(file, "[]");

            return file;
        }

        public static async Task<Dictionary<DateTime, PomodoroSession[]>> GetSessionsAsync(DateTime startDate, DateTime endDate)
        {
            Dictionary<DateTime, PomodoroSession[]> db = new();
            StorageFolder folder = ApplicationData.Current.LocalFolder;

            try
            {
                for (DateTime date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                {
                    string dateKey = date.ToString("yy-MM-dd");
                    StorageFile file = await folder.CreateFileAsync($"{dateKey}.json", CreationCollisionOption.OpenIfExists);

                    string content = await FileIO.ReadTextAsync(file);
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        PomodoroSession[] sessions =
                            JsonSerializer.Deserialize<PomodoroSession[]>(content) ?? Array.Empty<PomodoroSession>();

                        db[date] = sessions;
                    }
                }
            }
            catch
            {
            }

            return db;
        }

        public static async Task<PomodoroSession[]> GetFlattenedSessionsAsync(DateTime startDate, DateTime endDate)
        {
            var sessions = new List<PomodoroSession>(256);

            try
            {
                StorageFolder folder = ApplicationData.Current.LocalFolder;

                for (DateTime date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                {
                    string dateKey = date.ToString("yy-MM-dd");
                    StorageFile file = await folder.CreateFileAsync($"{dateKey}.json", CreationCollisionOption.OpenIfExists);

                    string content = await FileIO.ReadTextAsync(file);
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        PomodoroSession[] fileSessions =
                            JsonSerializer.Deserialize<PomodoroSession[]>(content) ?? Array.Empty<PomodoroSession>();

                        sessions.AddRange(fileSessions);
                    }
                }
            }
            catch
            {
            }

            return sessions.ToArray();
        }
    }
}
