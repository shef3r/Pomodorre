using System.IO.Pipes;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Pomodorre.BackgroundWorker;

class Program
{
    private static readonly CancellationTokenSource _cts = new();
    private static int _parentPid = -1;

    static async Task Main(string[] args)
    {
        if (args.Length > 0 && int.TryParse(args[0], out int pid))
            _parentPid = pid;

        if (_parentPid != -1)
            _ = MonitorParentProcessAsync(_cts.Token);

        try
        {
            await RunPipeServerAsync(_cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Normal exit
        }
        catch (Exception ex)
        {
            // Log fatal error (optional)
            Console.Error.WriteLine($"Fatal error: {ex}");
        }
    }

    private static async Task RunPipeServerAsync(CancellationToken cancellationToken)
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
                    PipeOptions.Asynchronous  // Use async I/O
                );

                await serverPipe.WaitForConnectionAsync(cancellationToken);

                using var reader = new StreamReader(serverPipe);
                using var writer = new StreamWriter(serverPipe) { AutoFlush = true };

                var handler = new PipeServerHandler(writer);

                while (serverPipe.IsConnected && !cancellationToken.IsCancellationRequested)
                {
                    string? line = await reader.ReadLineAsync();
                    if (line == null) break;

                    if (line == "shutdown")
                    {
                        _cts.Cancel();
                        break;
                    }

                    await handler.HandleCommand(line);
                }

                handler.Dispose();
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                // Log error (optional) and wait before retrying
                await Task.Delay(1000, cancellationToken);
            }
        }
    }

    private static async Task MonitorParentProcessAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var parent = Process.GetProcessById(_parentPid);
                if (parent.HasExited)
                {
                    _cts.Cancel();
                    break;
                }
            }
            catch (ArgumentException)
            {
                _cts.Cancel();
                break;
            }

            await Task.Delay(5000, cancellationToken);
        }
    }
}