using System.Collections.Concurrent;

namespace NetHook.Pipe;

public class PipeMessageQueue(HookPipeServer hookPipeServer)
{
    private readonly ConcurrentQueue<byte[]> _queue = new();

    public void Enqueue(byte[] data)
    {
        _queue.Enqueue(data);
    }

    public void Run(CancellationToken token)
    {
        Task.Run(() =>
        {
            while (true)
            {
                Task.Delay(50, token);
                if (!_queue.TryPeek(out var data)) continue;

                var sent = hookPipeServer.SendMessage(data);
                if (sent == false) continue;

                _queue.TryDequeue(out _);

                if (token.IsCancellationRequested) break;
            }
        }, token);
    }
}