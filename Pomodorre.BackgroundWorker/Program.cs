using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Windows.Forms;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;

namespace Pomodorre.BackgroundWorker;

class Program
{
    private static CancellationTokenSource _cts = new();
    private static NotifyIcon? _trayIcon;
    private static int _parentPid = -1;

    [STAThread]
    static void Main(string[] args)
    {
        if (args.Length > 0 && int.TryParse(args[0], out int pid))
        {
            _parentPid = pid;
            StartParentWatcher(_parentPid);
        }
        ApplicationConfiguration.Initialize();
        InitializeTray();
        _ = Task.Run(() => RunPipeServer());

        Application.Run();
    }

    private static void InitializeTray()
    {
        _trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "Pomodorre Background Worker",
            Visible = true
        };

        var menu = new ContextMenuStrip();

        menu.Items.Add("Open App", null, (s, e) => {
            FocusParent();
        });

        menu.Items.Add("-");

        menu.Items.Add("Exit Server", null, (s, e) => {
            _cts.Cancel();
            _trayIcon.Visible = false;
            Application.Exit();
        });

        _trayIcon.ContextMenuStrip = menu;
    }

    private static void FocusParent()
    {
        if (_parentPid == -1) return;
        try
        {
            var proc = Process.GetProcessById(_parentPid);
            IntPtr handle = proc.MainWindowHandle;
            if (handle != IntPtr.Zero)
            {
                SetForegroundWindow(handle);
            }
        }
        catch { }
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    private static async Task RunPipeServer()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                using var serverPipe = new NamedPipeServerStream("PomodorrePipe",
                    PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                await serverPipe.WaitForConnectionAsync(_cts.Token);

                using var reader = new StreamReader(serverPipe);
                using var writer = new StreamWriter(serverPipe) { AutoFlush = true };

                while (serverPipe.IsConnected && !_cts.Token.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync();
                    if (line == "PING") await writer.WriteLineAsync("PONG");
                }
            }
            catch { await Task.Delay(1000); }
        }
    }

    private static void StartParentWatcher(int pid)
    {
        _ = Task.Run(async () => {
            try
            {
                var parent = Process.GetProcessById(pid);
                parent.WaitForExit();
                Environment.Exit(0);
            }
            catch { Environment.Exit(0); }
        });
    }
}