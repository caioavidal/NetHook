using System.Runtime.InteropServices;
using NetHook.Hook;

namespace NetHook.Pipe;

public class HookPipeServer(RemoteProcess process) : PipeServer
{
    public override string Name => $"HookPipeServer-{process.ProcessId}";

    protected override void OnMessageReceived(byte[] buffer)
    {
        var registers = MemoryMarshal.Cast<byte, uint>(buffer.AsSpan());

        var methodHandle = (IntPtr)(((long)registers[8] << 32) | registers[9]);
        if (methodHandle == IntPtr.Zero) return;

        var delFromPtr = Marshal.GetDelegateForFunctionPointer<HookCallbackDelegate>(methodHandle);
        delFromPtr.Invoke(this, new HookEventArgs(registers[..8].ToArray()));
    }
}