using NetHook.Interop;
using SharpDisasm;

namespace NetHook.Helpers;

public static class AsmHelper
{
    public static int CalculateHookLength(RemoteProcess remoteProcess, IntPtr address)
    {
        var buffer = new byte[30];
        Kernel32.ReadProcessMemory(remoteProcess.Process.Handle, address, buffer, 30, out int bytesRead);

        var hookLength = 0;

        var mode = ArchitectureMode.x86_32;
        Disassembler.Translator.IncludeAddress = true;
        Disassembler.Translator.IncludeBinary = true;

        var disasm = new Disassembler(buffer, mode, 0, true);
        var instructions = disasm.Disassemble();

        foreach (var asm in instructions)
        {
            if (asm.Error) continue;
            if (hookLength >= 5) break;
            hookLength += asm.Length;
        }

        return hookLength;
    }
}