using Microsoft.UI.Xaml;
using Pomodorre.Statistics;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Pomodorre.Statistics;
public class HistoryHandler : ObservableCollection<PomodoroSession>
{
    private DateTime _weekStart;
    private bool _isLoading;

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(IsLoading)));
            }
        }
    }

    public DateTime WeekStart => _weekStart;
    public DateTime WeekEnd => _weekStart.AddDays(6).Date.AddHours(23).AddMinutes(59).AddSeconds(59);

    public bool CanNavigateNext
    {
        get
        {
            DateTime nextWeekStart = _weekStart.AddDays(7);
            return nextWeekStart <= DateTime.Now.Date;
        }
    }

    public HistoryHandler()
    {
        _weekStart = GetWeekStart(DateTime.Now);
        _ = RefreshAsync();
    }

    public async void PreviousWeek()
    {
        _weekStart = _weekStart.AddDays(-7);
        OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(WeekStart)));
        OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(WeekEnd)));
        OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(CanNavigateNext)));
        await RefreshAsync();
    }

    public async void NextWeek()
    {
        DateTime nextWeekStart = _weekStart.AddDays(7);
        if (nextWeekStart <= DateTime.Now.Date)
        {
            _weekStart = nextWeekStart;
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(WeekStart)));
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(WeekEnd)));
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(CanNavigateNext)));
            await RefreshAsync();
        }
    }

    private DateTime GetWeekStart(DateTime date)
    {
        int dayOfWeek = (int)date.DayOfWeek;
        int daysToSubtract = (dayOfWeek == 0) ? 6 : dayOfWeek - 1;
        return date.AddDays(-daysToSubtract).Date;
    }

    private async Task RefreshAsync()
    {
        IsLoading = true;

        try
        {
            var sessions = await HistoryTools.GetFlattenedSessionsAsync(WeekStart, WeekEnd);

            ReplaceRange(sessions);
        }
        catch
        {
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ReplaceRange(PomodoroSession[] newSessions)
    {
        Clear();

        for (int i = newSessions.Length - 1; i >= 0; i--)
        {
            Add(newSessions[i]);
        }
    }
}