using Pomodorre.Statistics;
using Pomodorre.TimerCore;
using Pomodorre.TimerCore.Services;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Pomodorre.BackgroundWorker;

public class PipeServerHandler : IDisposable
{
    private readonly StreamWriter _writer;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public PipeServerHandler(StreamWriter writer)
    {
        _writer = writer;

        PomodoroService.Instance.Tick += OnServiceTick;
        PomodoroService.Instance.SessionCompleted += OnServiceCompleted;
    }
    private async Task SendMessageAsync(string message)
    {
        await _writeLock.WaitAsync();
        try
        {
            await _writer.WriteLineAsync(message);
        }
        catch
        {
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task HandleCommand(string? commandLine)
    {
        if (string.IsNullOrEmpty(commandLine)) return;

        var parts = commandLine.Split('|');
        var cmd = parts[0];

        switch (cmd)
        {
            case PipeProtocol.CMD_START:
                if (parts.Length == 4)
                {
                    int blocks = int.Parse(parts[1]);
                    int focus = int.Parse(parts[2]);
                    int rest = int.Parse(parts[3]);
                    PomodoroService.Instance.Start(blocks, focus, rest);
                }
                break;

            case PipeProtocol.CMD_PAUSE:
                PomodoroService.Instance.Pause();
                break;

            case PipeProtocol.CMD_RESUME:
                PomodoroService.Instance.Resume();
                break;

            case PipeProtocol.CMD_STOP:
                PomodoroService.Instance.Stop();
                break;

            case PipeProtocol.CMD_STATUS:
                var session = PomodoroService.Instance.CurrentSession;
                bool isActive = session != null;
                bool isPaused = session?.IsPaused ?? false;
                string time = session?.Remaining.ToString(@"mm\:ss") ?? "00:00";
                double progress = PomodoroService.Instance.GetProgress();
                bool isBreak = session?.IsBreak ?? false;

                await SendMessageAsync($"{PipeProtocol.EVENT_STATUS}|{isActive}|{isPaused}|{time}|{progress.ToString(CultureInfo.InvariantCulture)}|{isBreak}");
                break;
        }
    }
    private async void OnServiceTick(object? sender, EventArgs e)
    {
        var session = PomodoroService.Instance.CurrentSession;
        if (session == null) return;

        string time = session.Remaining.ToString(@"mm\:ss");
        double progress = PomodoroService.Instance.GetProgress();
        bool isBreak = session.IsBreak;

        await SendMessageAsync($"{PipeProtocol.EVENT_TICK}|{time}|{progress.ToString(CultureInfo.InvariantCulture)}|{isBreak}");
    }

    private async void OnServiceCompleted(object? sender, PomodoroSession e)
    {
        await SessionLogger.LogOrUpdateSession(e);
        int currentStars = Stars.Amount;

        await SendMessageAsync($"{PipeProtocol.EVENT_COMPLETED}|{currentStars}");
    }

    public void Dispose()
    {
        PomodoroService.Instance.Tick -= OnServiceTick;
        PomodoroService.Instance.SessionCompleted -= OnServiceCompleted;
        _writeLock.Dispose();
    }
}