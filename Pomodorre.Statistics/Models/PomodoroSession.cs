using System;

public class PomodoroSession
{
    public Guid Id { get; set; }
    public int TotalBlocks { get; set; }
    public int FocusMinutes { get; set; }
    public int BreakMinutes { get; set; }
    public int CurrentBlockIndex { get; set; }
    public bool IsBreak { get; set; }
    public TimeSpan Remaining {  get; set; }
    public TimeSpan PhaseDuration { get; set; }
    public DateTime PhaseStartUtc { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public bool IsPaused { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsCancelled { get; set; }

    public string Status
    {
        get
        {
            if (IsCancelled)
                return "Cancelled";
            if (IsPaused)
                return "Paused";
            if (IsCompleted)
                return "Completed";
            return "In Progress";
        }
    }
}