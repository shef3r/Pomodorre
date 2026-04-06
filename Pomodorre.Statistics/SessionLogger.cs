using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Pomodorre.Statistics
{
    public static class SessionLogger
    {
        private static readonly SemaphoreSlim _lock = new(1, 1);

        public static async Task LogOrUpdateSession(object data)
        {
            if (data is not PomodoroSession session)
                throw new ArgumentException(nameof(data));

            string dateKey = session.StartedAtUtc.ToLocalTime().ToString("yy-MM-dd");

            await _lock.WaitAsync();
            try
            {
                StorageFile file = await HistoryTools.GetDailyFileAsync(dateKey);
                string json = await FileIO.ReadTextAsync(file);

                List<PomodoroSession> sessions =
                    JsonSerializer.Deserialize<List<PomodoroSession>>(json) ?? new List<PomodoroSession>();

                int index = sessions.FindIndex(s => s.Id == session.Id);

                if (index >= 0)
                    sessions[index] = session;
                else
                    sessions.Add(session);

                string tempName = $"{dateKey}.tmp";
                StorageFile tempFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(tempName, CreationCollisionOption.ReplaceExisting);

                await FileIO.WriteTextAsync(tempFile, JsonSerializer.Serialize(sessions));

                StorageFolder folder = ApplicationData.Current.LocalFolder;
                await tempFile.RenameAsync($"{dateKey}.json", NameCollisionOption.ReplaceExisting);

                if (session.IsCompleted)
                    Stars.Add((int)Math.Round(session.FocusMinutes * 0.5));
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}