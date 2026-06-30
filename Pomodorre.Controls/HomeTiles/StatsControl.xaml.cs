using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.Kernel.Sketches;
using SkiaSharp;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Pomodorre.Statistics;

namespace Pomodorre.Controls.HomeTiles
{
    public sealed partial class StatsControl : UserControl
    {
        public ObservableCollection<ISeries> Series { get; } = new ObservableCollection<ISeries>();
        public ObservableCollection<ICartesianAxis> XAxes { get; } = new ObservableCollection<ICartesianAxis>();
        public ObservableCollection<ICartesianAxis> YAxes { get; } = new ObservableCollection<ICartesianAxis>();

        public StatsControl()
        {
            InitializeComponent();
            SetupChart();
            _ = LoadWeeklyStats();
        }

        private void SetupChart()
        {
            YAxes.Add(new Axis
            {
                MinLimit = 0,
                Labeler = value => $"{value}m",
                TextSize = 12,
                SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200, 50)) { StrokeThickness = 1 }
            });

            XAxes.Add(new Axis
            {
                Labels = new string[7],
                TextSize = 12
            });
        }

        private async Task LoadWeeklyStats()
        {
            try
            {
                var sessions = await HistoryTools.GetSessionsAsync(
                    DateTime.Now.AddDays(-6).Date,
                    DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59));

                var grouped = sessions
                    .Values
                    .SelectMany(s => s)
                    .Where(s => s.IsCompleted)
                    .GroupBy(s => s.StartedAtUtc.Date)
                    .OrderBy(g => g.Key)
                    .ToList();

                var values = new double[7];
                var labels = new string[7];

                for (int i = 0; i < 7; i++)
                {
                    var date = DateTime.Now.AddDays(-6 + i).Date;
                    labels[i] = date.ToString("ddd");
                    
                    var totalMinutes = grouped
                        .FirstOrDefault(g => g.Key == date)
                        ?.Sum(s => s.FocusMinutes) ?? 0;

                    values[i] = totalMinutes;
                }

                DispatcherQueue.TryEnqueue(() =>
                {
                    if (values.All(v => v == 0))
                    {
                        Chart.Visibility = Visibility.Collapsed;
                        EmptyText.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        Chart.Visibility = Visibility.Visible;
                        EmptyText.Visibility = Visibility.Collapsed;

                        var skColor = SKColor.Parse("#0078D7");
                        try 
                        {
                            var accentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColor"];
                            skColor = new SKColor(accentColor.R, accentColor.G, accentColor.B, 255);
                        } 
                        catch { }
                        
                        Series.Clear();
                        Series.Add(new ColumnSeries<double>
                        {
                            Values = values,
                            Name = "Focus Minutes",
                            Fill = new SolidColorPaint(skColor),
                            Rx = 4,
                            Ry = 4,
                            MaxBarWidth = 40
                        });

                        XAxes[0].Labels = labels;
                    }
                });
            }
            catch
            {
            }
        }
    }
}
