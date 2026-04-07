using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Pomodorre.Controls.HomeTiles
{
    public sealed partial class StatsControl : UserControl
    {
        public ObservableCollection<DayStatItem> DayStats { get; } = new();

        public StatsControl()
        {
            InitializeComponent();
            _ = LoadWeeklyStats();
        }

        private async Task LoadWeeklyStats()
        {
            try
            {
                var sessions = await Pomodorre.Statistics.HistoryTools.GetSessionsAsync(
                    DateTime.Now.AddDays(-6).Date,
                    DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59));

                var grouped = sessions
                    .Values
                    .SelectMany(s => s)
                    .Where(s => s.IsCompleted)
                    .GroupBy(s => s.StartedAtUtc.Date)
                    .OrderBy(g => g.Key)
                    .ToList();

                var dayData = new System.Collections.Generic.List<(string Day, int Minutes)>();

                for (int i = 0; i < 7; i++)
                {
                    var date = DateTime.Now.AddDays(-6 + i).Date;
                    var dayName = date.ToString("ddd");
                    var totalMinutes = grouped
                        .FirstOrDefault(g => g.Key == date)
                        ?.Sum(s => s.FocusMinutes) ?? 0;

                    dayData.Add((dayName, totalMinutes));
                }

                var maxMinutes = dayData.Max(d => d.Minutes);
                if (maxMinutes == 0)
                    maxMinutes = 1;

                DispatcherQueue.TryEnqueue(() =>
                {
                    DayStats.Clear();

                    foreach (var (day, minutes) in dayData)
                    {
                        DayStats.Add(new DayStatItem
                        {
                            Day = day,
                            Minutes = minutes,
                            MaxMinutes = maxMinutes
                        });
                    }
                });
            }
            catch
            {
            }
        }
    }

    public class DayStatItem
    {
        public string Day { get; set; }
        public int Minutes { get; set; }
        public int MaxMinutes { get; set; }
    }
}

