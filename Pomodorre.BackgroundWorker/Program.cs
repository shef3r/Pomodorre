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
            NamedPipeServerStream? serverPipe = null;
            try
            {
                serverPipe = new NamedPipeServerStream(
                    "PomodorrePipe",
                    PipeDirection.InOut,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous  // Use async I/O
                );

                await serverPipe.WaitForConnectionAsync(cancellationToken);

                // Fire and forget
                _ = HandleClientAsync(serverPipe, cancellationToken);
                serverPipe = null;
            }
            catch (OperationCanceledException)
            {
                serverPipe?.Dispose();
                break;
            }
            catch (Exception ex)
            {
                serverPipe?.Dispose();
                await Task.Delay(1000, cancellationToken);
            }
        }
    }

    private static async Task HandleClientAsync(NamedPipeServerStream serverPipe, CancellationToken cancellationToken)
    {
        try
        {
            using (serverPipe)
            using (var reader = new StreamReader(serverPipe))
            using (var writer = new StreamWriter(serverPipe) { AutoFlush = true })
            {
                using var handler = new PipeServerHandler(writer);

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
            }
        }
        catch (Exception)
        {
            // Client disconnected or error occurred
        }
    }
}