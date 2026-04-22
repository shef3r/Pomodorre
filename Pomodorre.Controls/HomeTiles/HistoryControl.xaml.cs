using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Pomodorre.Statistics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pomodorre.Controls.HomeTiles
{
    public class SimpleSession
    {
        public Guid Id { get; set; }
        public string Time { get; set; }
        public string FocusMins { get; set; }
    }
    public sealed partial class HistoryControl : UserControl
    {
        public ObservableCollection<SimpleSession> HistoryStats = new ObservableCollection<SimpleSession>();
        
        public HistoryControl()
        {
            InitializeComponent();
            _ = LoadHistoryAsync();
        }

        private async Task LoadHistoryAsync()
        {
            PomodoroSession[] sessions = await HistoryTools.GetFlattenedSessionsAsync(DateTime.Now.Date.AddDays(-6), DateTime.Now.Date);
            foreach (var session in sessions)
            {
                HistoryStats.Add(new SimpleSession
                {
                    Id = session.Id,
                    Time = session.StartedAtUtc.ToLocalTime().ToString("d"),
                    FocusMins = $"{session.FocusMinutes} minutes focused" // todo get actual focused minutes
                });
            }
        }
    }
}
