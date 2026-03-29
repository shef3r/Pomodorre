using Pomodorre.Statistics;
using System;
using System.Diagnostics;
using System.Threading;

namespace Pomodorre.TimerCore.Services
{
    public sealed class PomodoroService
    {
        public static PomodoroService Instance { get; } = new();

        private Timer? _timer;

        public event EventHandler<string>? DebugInfoUpdated;
        public event EventHandler? Tick;
        public event EventHandler<PomodoroSession>? SessionCompleted;

        public PomodoroSession? CurrentSession { get; private set; }

        private string DebugInfo
        {
            set
            {
                DebugInfoUpdated?.Invoke(this, value);
            }
        }

        private PomodoroService()
        {
            string currentProcess = Process.GetCurrentProcess().ProcessName;

            if (currentProcess.Equals("Pomodorre.WinUI", StringComparison.OrdinalIgnoreCase))
            {
                Environment.FailFast("Nie ma tak, komunikacja tylko przez serwer");
            }
        }

        public void Start(int blocks, int focusMinutes, int breakMinutes)
        {
            CurrentSession = new PomodoroSession
            {
                Id = Guid.NewGuid(),
                TotalBlocks = blocks,
                FocusMinutes = focusMinutes,
                BreakMinutes = breakMinutes,
                CurrentBlockIndex = 1,
                IsBreak = false,
                IsPaused = false,
                IsCompleted = false,
                IsCancelled = false,
                StartedAtUtc = DateTime.UtcNow,
            };

            StartPhase(TimeSpan.FromMinutes(focusMinutes));
            StartTimer();
            UpdateDebugInfo();

            SessionLogger.LogOrUpdateSession(CurrentSession!);

            Console.WriteLine($"[START] Blocks={blocks}, Focus={focusMinutes}, Break={breakMinutes}");
        }

        public void Pause()
        {
            if (CurrentSession == null) return;

            CurrentSession.IsPaused = true;
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            UpdateDebugInfo();

            SessionLogger.LogOrUpdateSession(CurrentSession!);

            Console.WriteLine("[PAUSE]");
        }

        public void Resume()
        {
            if (CurrentSession == null) return;

            CurrentSession.IsPaused = false;
            CurrentSession.PhaseStartUtc = DateTime.UtcNow -
                (CurrentSession.PhaseDuration - CurrentSession.Remaining);

            StartTimer();
            UpdateDebugInfo();

            SessionLogger.LogOrUpdateSession(CurrentSession!);

            Console.WriteLine("[RESUME]");
        }

        public void Stop()
        {
            if (CurrentSession == null) return;

            CurrentSession.IsCancelled = true;
            _timer?.Dispose();

            SessionLogger.LogOrUpdateSession(CurrentSession!);

            CurrentSession = null;
            DebugInfo = "[No active session]";

            Console.WriteLine("[STOP]");
        }

        private void StartPhase(TimeSpan duration)
        {
            var s = CurrentSession!;
            s.PhaseDuration = duration;
            s.PhaseStartUtc = DateTime.UtcNow;
            s.Remaining = duration;
        }

        private void StartTimer()
        {
            _timer?.Dispose();
            _timer = new Timer(OnTick, null, 0, 1000);
        }

        private void OnTick(object? state)
        {
            if (CurrentSession == null)
            {
                DebugInfo = "[No active session]";
                return;
            }

            var s = CurrentSession;

            if (s.IsPaused)
            {
                UpdateDebugInfo();
                return;
            }

            var elapsed = DateTime.UtcNow - s.PhaseStartUtc;
            var remaining = s.PhaseDuration - elapsed;

            if (remaining <= TimeSpan.Zero)
            {
                AdvancePhase();
            }
            else
            {
                s.Remaining = remaining;
            }

            Tick?.Invoke(this, EventArgs.Empty);
            UpdateDebugInfo();
        }

        private void AdvancePhase()
        {
            var s = CurrentSession!;
            bool wasBreak = s.IsBreak;

            SessionLogger.LogOrUpdateSession(s);

            if (!wasBreak)
            {
                if (s.CurrentBlockIndex >= s.TotalBlocks)
                {
                    s.Remaining = TimeSpan.Zero;
                    CompleteSession();
                    return;
                }

                s.IsBreak = true;
                StartPhase(TimeSpan.FromMinutes(s.BreakMinutes));
                Console.WriteLine("[PHASE] Break started");
            }
            else
            {
                s.IsBreak = false;
                s.CurrentBlockIndex++;

                if (s.CurrentBlockIndex > s.TotalBlocks)
                {
                    s.Remaining = TimeSpan.Zero;
                    CompleteSession();
                    return;
                }

                StartPhase(TimeSpan.FromMinutes(s.FocusMinutes));
                Console.WriteLine("[PHASE] Focus started");
            }
        }

        private void CompleteSession()
        {
            _timer?.Dispose();

            var completed = CurrentSession!;
            completed.IsCompleted = true;

            SessionCompleted?.Invoke(this, completed);

            SessionLogger.LogOrUpdateSession(completed);

            CurrentSession = null;
            DebugInfo = "[Session complete]\nNo active session";

            Console.WriteLine("[COMPLETE] Session finished");
        }

        public double GetProgress()
        {
            if (CurrentSession == null) return 0;

            var s = CurrentSession;
            return 1.0 - (s.Remaining.TotalSeconds / s.PhaseDuration.TotalSeconds);
        }

        private void UpdateDebugInfo()
        {
            if (CurrentSession == null)
            {
                DebugInfo = "[No active session]";
                return;
            }

            var s = CurrentSession;
            var elapsed = s.PhaseDuration - s.Remaining;

            DebugInfo =
$@"=== Pomodoro Dashboard ===
Session ID: {s.Id}
Block {s.CurrentBlockIndex} / {s.TotalBlocks}
Phase: {(s.IsBreak ? "Break" : "Focus")}
Time Remaining: {s.Remaining:mm\:ss}
Elapsed Time: {elapsed:mm\:ss} / {s.PhaseDuration:mm\:ss}
Phase Progress: {GetProgress():P1}
Paused: {s.IsPaused}
Completed: {s.IsCompleted}
Cancelled: {s.IsCancelled}
==========================";
        }
    }
}