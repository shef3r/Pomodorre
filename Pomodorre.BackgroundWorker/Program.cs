using System.IO.Pipes;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Pomodorre.BackgroundWorker;

class Program
{
    private static CancellationTokenSource _cts = new();
    private static int _parentPid = -1;

    static void Main(string[] args)
    {
        // Parse parent PID if provided (optional)
        if (args.Length > 0 && int.TryParse(args[0], out int pid))
        {
            _parentPid = pid;
        }

        // Start monitoring the parent process (if PID known)
        if (_parentPid != -1)
        {
            Task.Run(() => MonitorParentProcess(_cts.Token));
        }

        // Run the pipe server on the main thread
        RunPipeServer(_cts.Token);
    }

    private static async void RunPipeServer(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var serverPipe = new NamedPipeServerStream(
                    "PomodorrePipe",
                    PipeDirection.InOut,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.None   // synchronous, no need for message pump
                );

                // Wait for a connection (blocks until client connects)
                serverPipe.WaitForConnection();

                using var reader = new StreamReader(serverPipe);
                using var writer = new StreamWriter(serverPipe) { AutoFlush = true };

                var handler = new PipeServerHandler(writer);

                // Process commands while pipe is connected
                while (serverPipe.IsConnected && !cancellationToken.IsCancellationRequested)
                {
                    string? line = reader.ReadLine();
                    if (line == null) break;

                    // Check for shutdown command
                    if (line == "shutdown")
                    {
                        _cts.Cancel();
                        break;
                    }

                    // Otherwise, let the handler process it
                    await handler.HandleCommand(line);
                }

                handler.Dispose();
            }
            catch (Exception)
            {
                // On error, wait a bit before retrying
                Thread.Sleep(1000);
            }
        }
    }

    private static void MonitorParentProcess(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Check if the parent process still exists
                var parent = Process.GetProcessById(_parentPid);
                if (parent.HasExited)
                {
                    _cts.Cancel();
                    break;
                }
            }
            catch (ArgumentException)
            {
                // Process doesn't exist (already gone)
                _cts.Cancel();
                break;
            }

            // Wait 5 seconds before checking again
            cancellationToken.WaitHandle.WaitOne(5000);
        }
    }
}