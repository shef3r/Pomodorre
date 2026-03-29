using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace Pomodorre.Statistics
{
    public static class SessionLogger
    {
        public static async void LogOrUpdateSession(object data)
        {
            PomodoroSession session = (PomodoroSession)data;
            StorageFile fileToSave = await GetDailyFileAsync(session.StartedAtUtc.ToString("yy-MM-dd"));
            List<PomodoroSession> sessions = [.. JsonSerializer.Deserialize<List<PomodoroSession>>(await FileIO.ReadTextAsync(fileToSave))];
            if (sessions.Any(s => s.Id == session.Id))
            {
                int index = sessions.FindIndex(s => s.Id == session.Id);
                sessions[index] = session;
            }
            else
            {
                sessions.Add(session);
            }
            await FileIO.WriteTextAsync(fileToSave, JsonSerializer.Serialize(sessions));
        }

        internal async static Task<StorageFile> GetDailyFileAsync(string date)
        {
            string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, $"{date}.json");
            if (!File.Exists(path)) { File.WriteAllText(path, "[]"); }

            return await StorageFile.GetFileFromPathAsync(path);
        }

        public static async Task<Dictionary<DateTime, PomodoroSession[]>> GetSessionsAsync(DateTime startDate, DateTime endDate)
        {
            Dictionary<DateTime, PomodoroSession[]> db = new Dictionary<DateTime, PomodoroSession[]>();

            do
            {
                string dateKey = startDate.ToString("yy-MM-dd");
                StorageFile file = await GetDailyFileAsync(dateKey);
                string content = await FileIO.ReadTextAsync(file);
                PomodoroSession[] sessions = JsonSerializer.Deserialize<PomodoroSession[]>(content) ?? Array.Empty<PomodoroSession>();
                db.Add(startDate, sessions);
                startDate = startDate.AddDays(1);
            } while (startDate <= endDate);

            return db;
        }
    }
}
