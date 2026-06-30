using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pomodorre.Statistics;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Pomodorre.Controls.HomeTiles
{
    public class DetailedSession
    {
        public string Status { get; set; }
        public string TimeStr { get; set; }
        public int FocusMins { get; set; }
    }

    public sealed partial class HistoryControl : UserControl
    {
        public ObservableCollection<DetailedSession> HistoryStats { get; } = new ObservableCollection<DetailedSession>();
        
        public HistoryControl()
        {
            InitializeComponent();
            try {
                var rl = new Microsoft.Windows.ApplicationModel.Resources.ResourceLoader();
                TitleText.Text = rl.GetString("HistoryTitle/Text");
                EmptyText.Text = rl.GetString("HistoryEmpty/Text");
            } catch { }
            _ = LoadHistoryAsync();
        }

        private async Task LoadHistoryAsync()
        {
            try
            {
                PomodoroSession[] sessions = await HistoryTools.GetFlattenedSessionsAsync(DateTime.Now.Date.AddDays(-7), DateTime.Now.Date);
                var recent = sessions
                    .Where(s => s.StartedAtUtc.Year > 2000)
                    .OrderByDescending(s => s.StartedAtUtc)
                    .Take(5)
                    .ToList();

                DispatcherQueue.TryEnqueue(() =>
                {
                    HistoryStats.Clear();
                    foreach (var session in recent)
                    {
                        var rl = new Microsoft.Windows.ApplicationModel.Resources.ResourceLoader();
                        string timeFormat = session.StartedAtUtc.ToLocalTime().ToString("g");
                        
                        HistoryStats.Add(new DetailedSession
                        {
                            Status = rl.GetString("Status" + session.Status.Replace(" ", "")),
                            TimeStr = timeFormat,
                            FocusMins = session.FocusMinutes
                        });
                    }

                    if (HistoryStats.Count == 0)
                    {
                        StatsList.Visibility = Visibility.Collapsed;
                        EmptyText.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        StatsList.Visibility = Visibility.Visible;
                        EmptyText.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch
            {
            }
        }
    }
}
