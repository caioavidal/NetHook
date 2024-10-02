using System.Diagnostics;
using System.IO.Pipes;

namespace NetHook.Pipe;

public abstract class PipeServer
{
    private NamedPipeServerStream _server;
    public abstract string Name { get; }
    protected abstract void OnMessageReceived(byte[] buffer);

    public bool SendMessage(byte[] buffer)
    {
        if (_server is null) return false;
        if (!_server.IsConnected) return false;
        if (!_server.CanWrite) return false;

        _server.Write(buffer, 0, buffer.Length);
        return true;
    }

    public void StartPipe(CancellationToken cancellationToken)
    {
        Task.Run(() =>
        {
            try
            {
                _server ??= new NamedPipeServerStream(Name, PipeDirection.InOut, 10, PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                var buffer = new byte[sizeof(int) * 10];

                while (true)
                {
                    Thread.Sleep(1);

                    if (!_server.IsConnected)
                        try
                        {
                            _server.WaitForConnection();
                        }
                        catch (IOException)
                        {
                            _server.Disconnect();
                            continue;
                        }
                    
                    var bytesCount = _server.Read(buffer, 0, buffer.Length);

                    if (!_server.IsConnected || bytesCount <= 0) continue;

                    OnMessageReceived(buffer);
                    _server.WaitForPipeDrain();

                    if (cancellationToken.IsCancellationRequested) return;
                }
            }
            catch (Exception ex)
            {
                Thread.Sleep(1000);
                StartPipe(cancellationToken);
                Debug.WriteLine(ex.StackTrace + ex.Message);
            }
        }, cancellationToken);
    }
}