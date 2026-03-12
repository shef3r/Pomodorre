using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pomodorre.TimerCore;

public static class PipeProtocol
{
    // do serwera
    public const string CMD_START = "START";     // START|blocks|focusMins|breakMins
    public const string CMD_PAUSE = "PAUSE";
    public const string CMD_RESUME = "RESUME";
    public const string CMD_STOP = "STOP";
    public const string CMD_STATUS = "STATUS_REQ";

    // z serwera
    public const string EVENT_TICK = "TICK";     // TICK|remaining_mm:ss|progress_float
    public const string EVENT_COMPLETED = "COMPLETED";
    public const string EVENT_STATUS = "STATUS_RES"; // STATUS_RES|IsActive|IsPaused|Time|Progress
}