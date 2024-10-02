using System.Diagnostics;
using NetHook.Interop;

namespace NetHook.Helpers;

public static class AOBScanner
{
    public static IntPtr ScanForPattern(Process process, string moduleName, byte[] pattern, string mask)
    {
        var processHandle = process.Handle;

        var module = FindModule(process, moduleName);

        var baseAddress = module.BaseAddress;
        var moduleSize = module.ModuleMemorySize;

        var addressFound = ScanForPattern(processHandle, baseAddress, moduleSize, pattern, mask);

        return addressFound;
    }

    private static ProcessModule FindModule(Process process, string moduleName)
    {
        foreach (ProcessModule module in process.Modules)
            if (module.ModuleName == moduleName)
                return module;

        return null;
    }

    public static IntPtr ScanForPattern(IntPtr processHandle, IntPtr baseAddress, int moduleSize, byte[] pattern,
        string mask)
    {
        var buffer = new byte[moduleSize];
        Kernel32.ReadProcessMemory(processHandle, baseAddress, buffer, moduleSize, out int bytesRead);

        for (var i = 0; i < moduleSize; i++)
            if (DataCompare(buffer, i, pattern, mask))
                return IntPtr.Add(baseAddress, i);

        return IntPtr.Zero;
    }

    private static bool DataCompare(byte[] data, int dataOffset, byte[] pattern, string mask)
    {
        for (var i = 0; i < pattern.Length; i++)
            if (mask[i] == 'x' && data[dataOffset + i] != pattern[i])
                return false;

        return true;
    }
}