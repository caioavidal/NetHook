using System.Diagnostics;
using System.Runtime.InteropServices;
using NetHook.Helpers;
using NetHook.Pipe;

namespace NetHook.Hook;

public class HookSession : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly PipeMessageQueue _pipeMessageQueue;

    public HookSession(Process process) : this(new RemoteProcess(process))
    {
    }

    public HookSession(RemoteProcess remoteProcess)
    {
        RemoteProcess = remoteProcess;
        _cancellationTokenSource = new CancellationTokenSource();

        var cancellationToken = _cancellationTokenSource.Token;

        var pipeServer = new HookPipeServer(remoteProcess);
        pipeServer.StartPipe(cancellationToken);

        _pipeMessageQueue = new PipeMessageQueue(pipeServer);
        _pipeMessageQueue.Run(cancellationToken);
    }

    public RemoteProcess RemoteProcess { get; }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
    }

    public HookResult Hook(byte[] pattern, string moduleName, HookCallbackDelegate callback)
    {
        var mask = new string('x', pattern.Length);
        return Hook(pattern, mask, moduleName, callback);
    }

    public HookResult Hook(byte[] pattern, string mask, string moduleName, HookCallbackDelegate callback)
    {
        var address = AOBScanner.ScanForPattern(RemoteProcess.Process, moduleName, pattern, mask);

        if (address == 0) return HookResult.PatternNotFound;

        return Hook(address, callback);
    }

    public HookResult Hook(IntPtr address, HookCallbackDelegate callback)
    {
        var functionPtr = Marshal.GetFunctionPointerForDelegate(callback);
        var addressStream = BitConverter.GetBytes((uint)address)[..4];
        var functionStream = BitConverter.GetBytes(functionPtr);

        var message = new byte[addressStream.Length + functionStream.Length + 1];
        var hookLength = AsmHelper.CalculateHookLength(RemoteProcess, address);

        message[0] = (byte)hookLength;
        Buffer.BlockCopy(addressStream, 0, message, 1, addressStream.Length);
        Buffer.BlockCopy(functionStream, 0, message, addressStream.Length + 1, functionStream.Length);

        _pipeMessageQueue.Enqueue(message);

        return HookResult.Success;
    }
}

public enum HookResult
{
    Success,
    PatternNotFound
}